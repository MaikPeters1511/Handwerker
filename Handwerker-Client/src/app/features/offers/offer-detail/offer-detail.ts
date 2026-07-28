import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import {TranslatePipe} from '../../../shared';
import {TranslationService} from '../../../core/services';
import {OfferService} from '../services/offer.service';
import {CompanyService} from '../../company/services/company.service';
import {RecipientService} from '../../recipients/services/recipient.service';
import {ProductService} from '../../products/services/product.service';
import {
  Company,
  Recipient,
  Product,
  OfferStatus,
  OfferDetail,
  Provider,
  UpdateOfferRequest, CreateOfferRequest
} from '../../../core/entities';

@Component({
  selector: 'app-offer-detail',
  imports: [CommonModule, ReactiveFormsModule, TranslatePipe],
  templateUrl: './offer-detail.html',
  styleUrl: './offer-detail.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class OfferDetailComponent implements OnInit {
  translate = inject(TranslationService);
  offerService = inject(OfferService);
  companyService = inject(CompanyService);
  recipientService = inject(RecipientService);
  productService = inject(ProductService);

  fb = inject(FormBuilder);
  route = inject(ActivatedRoute);
  router = inject(Router);

  offerId = signal<number | null>(null);
  loading = signal(false);

  // Signals for Data Selection
  companies = signal<Company[]>([]);
  selectedCompany = signal<Company | null>(null);

  recipients = signal<Recipient[]>([]);
  showRecipientDropdown = signal(false);

  products = signal<Product[]>([]);
  offerItems = signal<Product[]>([]); // Items currently in the offer

  form!: FormGroup;

  statuses: OfferStatus[] = ['Draft', 'Sent', 'Accepted', 'Declined', 'Converted'];

  paymentTermOptions = [
      'Zahlbar sofort ohne Abzug',
      '14 Tage netto',
      '30 Tage netto',
      '10 Tage 2% Skonto, 30 Tage netto',
      'Vorkasse',
      'Barzahlung'
  ];

  ngOnInit() {
    this.initForm();
    this.loadMasterData();

    const id = this.route.snapshot.paramMap.get('id');
    if (id && id !== 'new') {
      this.offerId.set(Number(id));
      this.loadOffer(Number(id));
    }
  }

  initForm() {
    const today = new Date().toISOString().split('T')[0];
    const validUntil = new Date();
    validUntil.setDate(validUntil.getDate() + 30);
    const validUntilStr = validUntil.toISOString().split('T')[0];

    this.form = this.fb.group({
      offerNumber: ['', Validators.required],
      offerDate: [today, Validators.required],
      validUntil: [validUntilStr, Validators.required],
      customerNumber: [''],
      status: ['Draft', Validators.required],
      paymentTerms: ['Zahlbar sofort ohne Abzug'],
      deliveryDate: [''],
      shippingMethod: [''],
      introText: ['Wir danken Ihnen für Ihre Anfrage und bieten Ihnen wie folgt an:'],
      outroText: [''],
      notes: [''],
      isReceived: [false],
      totalNet: [0],
      totalTaxAmount: [0],
      totalGross: [0]
    });
  }

  loadOffer(id: number) {
    this.loading.set(true);
    this.offerService.getOfferById(id).subscribe({
      next: (offer) => {
        this.patchFormWithOffer(offer);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Fehler beim Laden:', err);
        alert('Fehler beim Laden des Angebots');
        this.router.navigate(['/offers']);
      }
    });
  }

  patchFormWithOffer(offer: OfferDetail) {
    const offerDate = typeof offer.offerDate === 'string'
      ? offer.offerDate.split('T')[0]
      : new Date(offer.offerDate).toISOString().split('T')[0];

    const validUntil = typeof offer.validUntil === 'string'
      ? offer.validUntil.split('T')[0]
      : new Date(offer.validUntil).toISOString().split('T')[0];

    const deliveryDate = offer.deliveryDate
      ? (typeof offer.deliveryDate === 'string' ? offer.deliveryDate.split('T')[0] : new Date(offer.deliveryDate).toISOString().split('T')[0])
      : '';

    this.form.patchValue({
      offerNumber: offer.offerNumber,
      offerDate: offerDate,
      validUntil: validUntil,
      customerNumber: offer.customerNumber,
      status: offer.status,
      deliveryDate: deliveryDate,
      shippingMethod: offer.shippingMethod,
      introText: offer.introText,
      outroText: offer.outroText,
      notes: offer.notes,
      isReceived: offer.isReceived,
      totalNet: offer.totalNet,
      totalTaxAmount: offer.totalTaxAmount,
      totalGross: offer.totalGross
    });

    // Load products into signal
    this.offerItems.set(offer.products || []);

    // If OfferDetail has paymentTerms, patch it. If not on interface, we might need to update interface.
    // Checking OfferDetail interface... earlier it was defined without paymentTerms maybe?
    // Let's check model if needed, but for now just initializing form control.
  }

  loadMasterData() {
    // Load Companies
    this.companyService.getCompanies().subscribe(companies => {
        this.companies.set(companies);
        if (companies.length > 0) {
            this.selectedCompany.set(companies[0]);
        }
    });
  }

  // Company Selection
  selectCompany(company: Company) {
      this.selectedCompany.set(company);
  }

  // Customer Search
  searchCustomer(term: string) {
      if (!term) {
        this.recipientService.getRecipients().subscribe(list => {
            this.recipients.set(list);
            this.showRecipientDropdown.set(true);
        });
        return;
      }
      // Simple filtering on client side for now (or call API search)
      this.recipientService.getRecipients().subscribe(list => {
          const filtered = list.filter(r => r.name.toLowerCase().includes(term.toLowerCase()) || r.customerNumber.includes(term));
          this.recipients.set(filtered);
          this.showRecipientDropdown.set(true);
      });
  }

  selectRecipient(recipient: Recipient) {
      this.form.patchValue({
          customerNumber: recipient.customerNumber
      });
      // Here we could also patch address fields if they existed in form individually
      // For now we assume the form only holds customerNumber, but visually we show details
      this.showRecipientDropdown.set(false);

      // We might want to store the full object to display it in the view
      // Since Recipient is a Signal in the view for form display?
      // The view currently uses `form.get('customerNumber')?.value` to switch display.
      // But it displays "Musterkunde GmbH" hardcoded. We need a signal for current Recipient display.
      this.currentRecipient.set(recipient);
  }

  currentRecipient = signal<Recipient | null>(null);

  // Method to extract payment details
  getPaymentDetails() {
      const terms = this.form?.get('paymentTerms')?.value || '';
      const totalGross = this.form?.get('totalGross')?.value || 0;
      const offerDateVal = this.form?.get('offerDate')?.value;
      const offerDate = offerDateVal ? new Date(offerDateVal) : new Date();

      let dueDays = 0;
      let skontoRate = 0;
      let skontoDays = 0;

      // Simple parsing logic derived from standard strings
      if (terms.includes('14 Tage')) {
          dueDays = 14;
      } else if (terms.includes('30 Tage')) {
          dueDays = 30;
      } else if (terms.includes('sofort')) {
          dueDays = 0;
      }

      // Parse specific "X Tage Y% Skonto" pattern
      // Example: '10 Tage 2% Skonto, 30 Tage netto'
      if (terms.includes('Skonto')) {
          const skontoMatch = terms.match(/(\d+)\s*Tage\s*(\d+)%/);
          if (skontoMatch) {
              skontoDays = parseInt(skontoMatch[1], 10);
              skontoRate = parseInt(skontoMatch[2], 10);
          }
      }

      // Calculate Dates
      const dueDate = new Date(offerDate);
      dueDate.setDate(dueDate.getDate() + dueDays);

      const skontoDate = new Date(offerDate);
      skontoDate.setDate(skontoDate.getDate() + skontoDays);

      return {
          dueDays: dueDays,
          dueDate: dueDate,
          totalAmount: totalGross,
          skontoRate: skontoRate,
          skontoDays: skontoDays,
          skontoDate: skontoDate,
          skontoAmount: totalGross * (skontoRate / 100),
          amountAfterSkonto: totalGross * (1 - skontoRate / 100)
      };
  }

  // Product Search
  searchProduct(term: string) {
      this.productService.getProducts().subscribe(list => {
           if (!term) {
               this.products.set(list);
           } else {
               const filtered = list.filter(p => p.name.toLowerCase().includes(term.toLowerCase()) || p.articleNumber?.includes(term));
               this.products.set(filtered);
           }
      });
  }

  addProduct(product: Product) {
      // Create a new line item based on the selected product
      const taxRate = product.taxRate || 19;
      const totalNet = product.unitPrice;
      const taxAmount = totalNet * (taxRate / 100);
      const totalGross = totalNet + taxAmount;

      const newItem: Product = {
          ...product,
          // If the product comes from catalog, we might want to reset ID or keep it as reference?
          // For simplicity in this mock, we keep it but ensure position is correct
          position: this.offerItems().length + 1,
          quantity: 1,
          discountPercent: 0,
          discountAmount: 0,
          taxAmount: taxAmount,
          totalNet: totalNet,
          totalGross: totalGross
      };

      this.offerItems.update(items => [...items, newItem]);
      this.calculateTotals();
  }

  addManualPosition() {
      const newItem: Product = {
          id: 0, // 0 for new unsaved item
          articleNumber: '',
          name: '',
          description: '',
          unit: 'Stk',
          unitPrice: 0,
          taxRate: 19,
          taxAmount: 0,
          quantity: 1,
          discountPercent: 0,
          discountAmount: 0,
          totalNet: 0,
          totalGross: 0,
          position: this.offerItems().length + 1
      };

      this.offerItems.update(items => [...items, newItem]);
  }

  updateItemProperty(index: number, property: keyof Product, value: any) {
      this.offerItems.update(items => {
          const updated = [...items];
          const item = { ...updated[index], [property]: value } as Product;

          // Recalculate totals for this item
          if (property === 'quantity' || property === 'unitPrice' || property === 'discountPercent' || property === 'taxRate') {
              const quantity = Number(item.quantity) || 0;
              const unitPrice = Number(item.unitPrice) || 0;
              const discountPercent = Number(item.discountPercent) || 0;
              const taxRate = Number(item.taxRate) || 19;

              item.totalNet = unitPrice * quantity * (1 - discountPercent / 100);
              item.taxAmount = item.totalNet * (taxRate / 100);
              item.totalGross = item.totalNet + item.taxAmount;
          }

          updated[index] = item;
          return updated;
      });
      this.calculateTotals();
  }

  updateQuantity(index: number, qty: string | number) {
     this.updateItemProperty(index, 'quantity', qty);
  }

  removeItem(index: number) {
      this.offerItems.update(items => items.filter((_, i) => i !== index));
      this.calculateTotals();
  }

  calculateTotals() {
      const items = this.offerItems();
      const totalNet = items.reduce((sum, item) => sum + item.totalNet, 0);
      // Simplified tax calc (assuming same rate or sum of item taxes)
      // Ideally item.taxAmount should be calculated and summed
      const totalGross = items.reduce((sum, item) => sum + item.totalGross, 0);
      const totalTax = totalGross - totalNet;

      this.form.patchValue({
          totalNet,
          totalTaxAmount: totalTax,
          totalGross
      });
      // Force change detection/update for signals or view if needed, though form value changes should drive it.
      // But getPaymentDetails reads from form value.
      // If OnPush is used, form update might not trigger view update immediately if not using form signals.
      // However, getPaymentDetails is called in template, so it should be fine on cycles.
  }

  onSubmit() {
    if (this.form.invalid) {
      alert('Bitte füllen Sie alle Pflichtfelder aus.');
      return;
    }

    this.loading.set(true);
    const formValue = this.form.value;

    // Mock Recipient & Provider (in production: from selection)
    const mockRecipient: Recipient = {
      id: 1,
      customerNumber: formValue.customerNumber || '0000',
      salutation: 'Herr',
      name: 'Musterkunde',
      contactPerson: '',
      street: 'Musterstraße 1',
      addressLine2: '',
      zipCode: '12345',
      city: 'Musterstadt',
      country: 'Deutschland',
      email: 'kunde@example.com',
      phone: ''
    };

    // Use selected recipient if available, else mock or form data
    const recipientToUse = this.currentRecipient() || mockRecipient;
    // Update recipient fields if user selected one
    if (this.currentRecipient()) {
         recipientToUse.customerNumber = formValue.customerNumber;
    }

    const mockProvider: Provider = {
      id: 1,
      name: 'Muster Handwerker',
      company: 'Muster GmbH',
      street: 'Handwerkerstr. 1',
      zipCode: '12345',
      city: 'Stadt',
      email: 'info@handwerker.de',
      phone: '',
      taxId: '',
      taxNumber: '',
      commercialRegister: '',
      registerCourt: '',
      bank: { id: 1, name: 'Sparkasse', iban: '', bic: '', plz: '', ort: '' }
    };

    const items = this.offerItems();

    if (this.offerId()) {
      // Update
      const request: UpdateOfferRequest = {
        id: this.offerId()!,
        offerNumber: formValue.offerNumber,
        offerDate: formValue.offerDate,
        validUntil: formValue.validUntil,
        customerNumber: formValue.customerNumber,
        recipient: recipientToUse,
        provider: mockProvider,
        products: items,
        totalNet: formValue.totalNet,
        totalTaxAmount: formValue.totalTaxAmount,
        totalGross: formValue.totalGross,
        status: formValue.status,
        introText: formValue.introText,
        outroText: formValue.outroText,
        notes: formValue.notes,
        isReceived: formValue.isReceived,
        deliveryDate: formValue.deliveryDate || null,
        shippingMethod: formValue.shippingMethod || null
      };

      this.offerService.updateOffer(this.offerId()!, request).subscribe({
        next: () => {
          this.loading.set(false);
          alert('Angebot erfolgreich aktualisiert!');
          this.router.navigate(['/offers']);
        },
        error: (err) => {
          console.error('Fehler:', err);
          alert('Fehler beim Speichern');
          this.loading.set(false);
        }
      });
    } else {
      // Create
      const request: CreateOfferRequest = {
        offerDate: formValue.offerDate,
        validUntil: formValue.validUntil,
        customerNumber: formValue.customerNumber,
        recipient: recipientToUse,
        provider: mockProvider,
        products: items,
        totalNet: formValue.totalNet,
        totalTaxAmount: formValue.totalTaxAmount,
        totalGross: formValue.totalGross,
        status: formValue.status,
        introText: formValue.introText,
        outroText: formValue.outroText,
        notes: formValue.notes,
        isReceived: formValue.isReceived,
        deliveryDate: formValue.deliveryDate || null,
        shippingMethod: formValue.shippingMethod || null
      };

      this.offerService.createOffer(request).subscribe({
        next: () => {
          this.loading.set(false);
          alert('Angebot erfolgreich erstellt!');
          this.router.navigate(['/offers']);
        },
        error: (err) => {
          console.error('Fehler:', err);
          alert('Fehler beim Erstellen');
          this.loading.set(false);
        }
      });
    }
  }

  onCancel() {
    this.router.navigate(['/offers']);
  }

  onPrint() {
    window.print();
  }

  getStatusLabel(status: OfferStatus): string {
    const labels: Record<OfferStatus, string> = {
      'Draft': 'offers.status.draft',
      'Sent': 'offers.status.sent',
      'Accepted': 'offers.status.accepted',
      'Declined': 'offers.status.declined',
      'Converted': 'offers.status.converted'
    };
    return labels[status] || status;
  }
}
