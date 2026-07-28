import { ChangeDetectionStrategy, Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, FormArray, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { RecipientService } from '../../recipients/services/recipient.service';
import { CompanyService } from '../../company/services/company.service';
import { ProductService } from '../../products/services/product.service';
import { Invoice } from '../../../core/entities';
import { Recipient } from '../../../core/entities';
import { Company } from '../../../core/entities';
import { Product } from '../../../core/entities';
import { I18nService, InvoiceService, TranslationService } from '../../../core/services';
import { TranslatePipe } from '../../../shared';


@Component({
  selector: 'app-invoice-detail',
  imports: [ReactiveFormsModule, TranslatePipe],
  templateUrl: './invoice-detail.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class InvoiceDetail implements OnInit {
  fb = inject(FormBuilder);
  route = inject(ActivatedRoute);
  router = inject(Router);
  invoiceService = inject(InvoiceService);
  translationService = inject(TranslationService);
  i18nService = inject(I18nService);
  recipientService = inject(RecipientService);
  companyService = inject(CompanyService);
  productService = inject(ProductService);

  invoiceId = signal<number | null>(null);
  mode = signal<'view' | 'edit' | 'new'>('new');
  loading = signal(false);

  recipients = signal<Recipient[]>([]);
  companies = signal<Company[]>([]);
  availableProducts = signal<Product[]>([]);
  selectedProviderCompany = signal<Company | null>(null);

  recipientSearchTerm = signal('');
  showRecipientOptions = signal(false);
  activeRecipientOptionIndex = signal(-1);

  productSuggestions = signal<Record<number, Product[]>>({});
  showProductOptions = signal<Record<number, boolean>>({});
  activeProductOptionIndex = signal<Record<number, number>>({});

  form!: FormGroup;

  paymentTermOptions = [
    { value: 'Zahlbar sofort ohne Abzug', labelKey: 'invoices.paymentTerms.immediate' },
    { value: '14 Tage netto', labelKey: 'invoices.paymentTerms.net14' },
    { value: '30 Tage netto', labelKey: 'invoices.paymentTerms.net30' },
    { value: '10 Tage 2% Skonto, 30 Tage netto', labelKey: 'invoices.paymentTerms.discount10' }
  ];

  totalNet = signal(0);
  totalTax = signal(0);
  totalGross = signal(0);

  filteredRecipients = computed(() => {
    const term = this.recipientSearchTerm().trim().toLowerCase();
    if (!term)
    {
      return this.recipients().slice(0, 8);
    }

    return this.recipients()
      .filter(r =>
        r.name.toLowerCase().includes(term) ||
        (r.customerNumber ?? '').toLowerCase().includes(term) ||
        (r.email ?? '').toLowerCase().includes(term))
      .slice(0, 8);
  });

  ngOnInit() {
    this.initForm();
    this.performCalculations();
    this.loadMasterData();

    // Auf Änderungen der Anbieter-Auswahl abonnieren, um vollständige Firmendaten zu laden
    this.form.get('providerId')?.valueChanges.subscribe(providerId => {
      if (providerId) {
        this.companyService.getCompany(providerId).subscribe(company => {
          this.selectedProviderCompany.set(company);
        });
      } else {
        this.selectedProviderCompany.set(null);
      }
    });

    const id = this.route.snapshot.paramMap.get('id');
    const queryMode = this.route.snapshot.queryParamMap.get('mode');

    if (id && id !== 'new') {
      this.invoiceId.set(Number(id));
      this.mode.set(queryMode === 'edit' ? 'edit' : 'view');
      this.loadInvoice(Number(id));
    } else {
      this.mode.set('new');
      // Lade die nächste Rechnungsnummer
      this.loadNextInvoiceNumber();
      this.focusFirstInput();
    }

    if (this.mode() === 'view') {
      this.form.disable();
    }
  }

  performCalculations() {
    // Effekt für die Berechnung der Gesamtbeträge
    // Da wir Signals für die Summen nutzen, können wir diese reaktiv lassen oder computed nutzen.
    // Aber die Logik ist aktuell in updateTotals() und wird manuell aufgerufen. Das ist okay für onPush.
  }

  initForm() {
    // Current date as YYYY-MM-DD string without time adjustments issue
    const now = new Date();
    const today = now.toISOString().split('T')[0];

    // Due date calculation
    const dueDateObj = new Date(now);
    dueDateObj.setDate(now.getDate() + 14);
    const dueDateStr = dueDateObj.toISOString().split('T')[0];

    this.form = this.fb.group({
      invoiceNumber: [{ value: '', disabled: true }],
      invoiceDate: [today, Validators.required],
      servicePeriod: ['', Validators.required],
      customerNumber: [''],
      recipientId: [null, Validators.required],
      providerId: [null, Validators.required],
      dueDate: [dueDateStr, Validators.required],
      paymentTerms: ['14 Tage netto'],
      isPaid: [false],
      introText: [this.translationService.translate('invoices.detail.defaults.introText')],
      outroText: [''],
      products: this.fb.array([], [Validators.required, this.minLengthArray(1)])
    });
  }

  // Custom validator for FormArray length
  minLengthArray(min: number) {
    return (c: import('@angular/forms').AbstractControl): {[key: string]: any} | null => {
      if (c.value.length >= min)
        return null;
      return { 'minLengthArray': {valid: false, minLength: min} };
    }
  }

  get productsArray(): FormArray {
    return this.form.get('products') as FormArray;
  }

  loadMasterData() {
    this.recipientService.getRecipients().subscribe(data => this.recipients.set(data));
    this.companyService.getCompanies().subscribe(data => this.companies.set(data));
    this.productService.getProducts().subscribe(data => this.availableProducts.set(data));
  }

  loadNextInvoiceNumber() {
    this.invoiceService.getNextInvoiceNumber().subscribe({
      next: (invoiceNumber) => {
        this.form.patchValue({ invoiceNumber });
      },
      error: (err) => {
        console.error(this.translationService.translate('invoices.detail.errors.loadNextNumberFailed'), err);
      }
    });
  }

  loadInvoice(id: number) {
    this.loading.set(true);
    this.invoiceService.getInvoice(id).subscribe({
      next: (invoice) => {
        this.patchFormWithInvoice(invoice);
        this.loading.set(false);
        if (this.mode() === 'edit')
        {
          this.focusFirstInput();
        }
      },
      error: (err) => {
        console.error(this.translationService.translate('invoices.detail.errors.loadFailed'), err);
        alert(this.translationService.translate('invoices.detail.errors.loadFailed'));
        this.router.navigate(['/invoices']);
      }
    });
  }

  patchFormWithInvoice(invoice: Invoice) {
    // Temporär das Feld invoiceNumber aktivieren, um es im Bearbeitungsmodus zu patchen
    this.form.get('invoiceNumber')?.enable();

    this.form.patchValue({
      invoiceNumber: invoice.invoiceNumber,
      invoiceDate: invoice.invoiceDate.split('T')[0],
      servicePeriod: invoice.servicePeriod,
      customerNumber: invoice.customerNumber,
      recipientId: invoice.recipient.id,
      providerId: invoice.provider.id,
      dueDate: invoice.dueDate.split('T')[0],
      paymentTerms: invoice.paymentTerms,
      isPaid: invoice.isPaid,
      introText: invoice.introText,
      outroText: invoice.outroText
    });

    this.recipientSearchTerm.set(invoice.recipient.name);

    // Produkte laden
    invoice.products.forEach(product => {
      this.productsArray.push(this.createProductFormGroup(product));
    });

    // Update totals after loading products
    this.updateTotals();
  }

  createProductFormGroup(product?: Product): FormGroup {
    return this.fb.group({
      id: [product?.id || 0],
      articleNumber: [product?.articleNumber || ''],
      name: [product?.name || '', Validators.required],
      position: [product?.position || 0],
      quantity: [product?.quantity || 1, [Validators.required, Validators.min(0.01)]],
      unit: [product?.unit || 'Stk.'],
      description: [product?.description || ''],
      unitPrice: [product?.unitPrice || 0, [Validators.required, Validators.min(0)]],
      taxRate: [product?.taxRate || 19, Validators.required],
      discountPercent: [product?.discountPercent || 0],
      discountAmount: [product?.discountAmount || 0],
      taxAmount: [product?.taxAmount || 0],
      totalNet: [product?.totalNet || 0],
      totalGross: [product?.totalGross || 0]
    });
  }

  addProduct() {
    this.productsArray.push(this.createProductFormGroup());
    this.updateTotals();
  }

  removeProduct(index: number) {
    this.productsArray.removeAt(index);
    this.productSuggestions.set({});
    this.showProductOptions.set({});
    this.activeProductOptionIndex.set({});
    this.updateTotals();
  }

  calculateProductTotals(index: number) {
    const productGroup = this.productsArray.at(index) as FormGroup;
    const quantity = productGroup.get('quantity')?.value || 0;
    const unitPrice = productGroup.get('unitPrice')?.value || 0;
    const taxRate = productGroup.get('taxRate')?.value || 0;
    const discountPercent = productGroup.get('discountPercent')?.value || 0;

    const subtotal = quantity * unitPrice;
    const discountAmount = subtotal * (discountPercent / 100);
    const totalNet = subtotal - discountAmount;
    const taxAmount = totalNet * (taxRate / 100);
    const totalGross = totalNet + taxAmount;

    productGroup.patchValue({
      discountAmount,
      taxAmount,
      totalNet,
      totalGross
    }, { emitEvent: false });

    this.updateTotals();
  }

  updateTotals() {
    const products = this.productsArray.value;
    const net = products.reduce((sum: number, p: any) => sum + (p.totalNet || 0), 0);
    const tax = products.reduce((sum: number, p: any) => sum + (p.taxAmount || 0), 0);
    const gross = net + tax;

    this.totalNet.set(net);
    this.totalTax.set(tax);
    this.totalGross.set(gross);
  }

  onProductChange(index: number) {
    this.calculateProductTotals(index);
  }

  onRecipientSearchInput(event: Event) {
    const value = (event.target as HTMLInputElement).value;
    this.recipientSearchTerm.set(value);
    this.showRecipientOptions.set(true);
    this.activeRecipientOptionIndex.set(this.filteredRecipients().length > 0 ? 0 : -1);

    const exactMatch = this.recipients().find(r => r.name.toLowerCase() === value.trim().toLowerCase());
    if (!exactMatch)
    {
      this.form.patchValue({ recipientId: null });
    }
  }

  onRecipientSearchFocus() {
    this.showRecipientOptions.set(true);
    this.activeRecipientOptionIndex.set(this.filteredRecipients().length > 0 ? 0 : -1);
  }

  onRecipientSearchBlur() {
    setTimeout(() => this.showRecipientOptions.set(false), 120);
  }

  onRecipientSearchKeydown(event: KeyboardEvent) {
    const options = this.filteredRecipients();
    if (options.length === 0)
    {
      return;
    }

    if (event.key === 'ArrowDown')
    {
      event.preventDefault();
      const next = Math.min(this.activeRecipientOptionIndex() + 1, options.length - 1);
      this.activeRecipientOptionIndex.set(next);
      return;
    }

    if (event.key === 'ArrowUp')
    {
      event.preventDefault();
      const next = Math.max(this.activeRecipientOptionIndex() - 1, 0);
      this.activeRecipientOptionIndex.set(next);
      return;
    }

    if (event.key === 'Enter')
    {
      const active = this.activeRecipientOptionIndex();
      if (active >= 0 && active < options.length)
      {
        event.preventDefault();
        this.selectRecipientFromAutocomplete(options[active]);
      }
      return;
    }

    if (event.key === 'Escape')
    {
      this.showRecipientOptions.set(false);
    }
  }

  selectRecipientFromAutocomplete(recipient: Recipient) {
    this.form.patchValue({
      recipientId: recipient.id,
      customerNumber: recipient.customerNumber ?? this.form.get('customerNumber')?.value ?? ''
    });
    this.recipientSearchTerm.set(recipient.name);
    this.showRecipientOptions.set(false);
  }

  recipientOptionId(index: number): string {
    return `recipient-option-${index}`;
  }

  activeRecipientOptionId(): string | null {
    const index = this.activeRecipientOptionIndex();
    return index >= 0 ? this.recipientOptionId(index) : null;
  }

  onProductSearchInput(index: number, event: Event) {
    const value = (event.target as HTMLInputElement).value;
    if (value.trim().length < 2)
    {
      this.setProductSuggestions(index, []);
      this.setProductOptionsVisibility(index, false);
      this.setProductActiveOptionIndex(index, -1);
      return;
    }

    this.productService.searchProducts(value).subscribe({
      next: products => {
        const limited = products.slice(0, 8);
        this.setProductSuggestions(index, limited);
        this.setProductOptionsVisibility(index, true);
        this.setProductActiveOptionIndex(index, limited.length > 0 ? 0 : -1);
      },
      error: () => {
        this.setProductSuggestions(index, []);
        this.setProductOptionsVisibility(index, false);
        this.setProductActiveOptionIndex(index, -1);
      }
    });
  }

  onProductSearchFocus(index: number) {
    const items = this.getProductSuggestions(index);
    if (items.length > 0)
    {
      this.setProductOptionsVisibility(index, true);
      this.setProductActiveOptionIndex(index, 0);
    }
  }

  onProductSearchBlur(index: number) {
    setTimeout(() => this.setProductOptionsVisibility(index, false), 120);
  }

  onProductSearchKeydown(index: number, event: KeyboardEvent) {
    const options = this.getProductSuggestions(index);
    if (options.length === 0)
    {
      return;
    }

    const active = this.getProductActiveOptionIndex(index);

    if (event.key === 'ArrowDown')
    {
      event.preventDefault();
      this.setProductActiveOptionIndex(index, Math.min(active + 1, options.length - 1));
      return;
    }

    if (event.key === 'ArrowUp')
    {
      event.preventDefault();
      this.setProductActiveOptionIndex(index, Math.max(active - 1, 0));
      return;
    }

    if (event.key === 'Enter')
    {
      if (active >= 0 && active < options.length)
      {
        event.preventDefault();
        this.selectSuggestedProduct(index, options[active]);
      }
      return;
    }

    if (event.key === 'Escape')
    {
      this.setProductOptionsVisibility(index, false);
    }
  }

  selectSuggestedProduct(index: number, product: Product) {
    const productGroup = this.productsArray.at(index) as FormGroup;
    productGroup.patchValue({
      articleNumber: product.articleNumber,
      name: product.name,
      unit: product.unit,
      description: product.description,
      unitPrice: product.unitPrice,
      taxRate: product.taxRate
    });

    this.calculateProductTotals(index);
    this.setProductOptionsVisibility(index, false);
  }

  getProductSuggestions(index: number): Product[] {
    return this.productSuggestions()[index] ?? [];
  }

  isProductOptionsVisible(index: number): boolean {
    return this.showProductOptions()[index] ?? false;
  }

  getProductActiveOptionIndex(index: number): number {
    return this.activeProductOptionIndex()[index] ?? -1;
  }

  productOptionId(index: number, optionIndex: number): string {
    return `product-${index}-option-${optionIndex}`;
  }

  activeProductOptionId(index: number): string | null {
    const active = this.getProductActiveOptionIndex(index);
    return active >= 0 ? this.productOptionId(index, active) : null;
  }

  private setProductSuggestions(index: number, suggestions: Product[]) {
    this.productSuggestions.update(current => ({ ...current, [index]: suggestions }));
  }

  private setProductOptionsVisibility(index: number, isVisible: boolean) {
    this.showProductOptions.update(current => ({ ...current, [index]: isVisible }));
  }

  private setProductActiveOptionIndex(index: number, optionIndex: number) {
    this.activeProductOptionIndex.update(current => ({ ...current, [index]: optionIndex }));
  }

  enableEdit() {
    this.mode.set('edit');
    this.form.enable();
  }

  onCancel() {
    if (this.invoiceId()) {
      this.router.navigate(['/invoices', this.invoiceId()]);
    } else {
      this.router.navigate(['/invoices']);
    }
  }

  onSave() {
    if (this.form.invalid) {
      this.form.markAllAsTouched(); // Mark fields as touched so validation errors show up
      alert(this.translationService.translate('invoices.detail.errors.invalidForm'));
      return;
    }

    // invoiceNumber-Feld aktivieren, damit es im Form-Wert enthalten ist
    this.form.get('invoiceNumber')?.enable();
    const formValue = this.form.getRawValue();

    // Build invoice object
    const recipient = this.recipients().find(r => r.id === formValue.recipientId);
    const provider = this.selectedProviderCompany() || this.companies().find(c => c.id === formValue.providerId);

    if (!recipient || !provider) {
      console.error(this.translationService.translate('invoices.detail.errors.missingRecipientOrProviderDebug'), {
        recipientId: formValue.recipientId,
        providerId: formValue.providerId,
        recipients: this.recipients(),
        selectedProvider: this.selectedProviderCompany(),
        companies: this.companies()
      });
      alert(this.translationService.translate('invoices.detail.errors.selectRecipientAndProvider'));
      return;
    }

    // Warnung wenn Bankdaten fehlen
    const hasCompleteBankData = provider.iban && provider.bic && provider.bankName;
    if (!hasCompleteBankData) {
      console.warn(this.translationService.translate('invoices.detail.errors.incompleteBankDataDebug'), provider);
      alert(this.translationService.translate('invoices.detail.errors.incompleteBankData'));
      return;
    }

    // Korrektes Anbieter-Objekt aus Company erstellen inklusive Bank-Objekt
    const providerObj = {
      id: provider.id,
      name: provider.name,
      company: provider.name,
      street: provider.street || '',
      zipCode: provider.zipCode || '',
      city: provider.city || '',
      phone: provider.phone || '',
      email: provider.email || '',
      taxId: provider.taxId || '',
      taxNumber: provider.taxNumber || '',
      commercialRegister: provider.commercialRegister || '',
      registerCourt: provider.registerCourt || '',
      bank: {
        id: 0,
        iban: provider.iban!,
        name: provider.bankName!,
        plz: provider.zipCode || '00000',
        ort: provider.city || 'Stadt',
        bic: provider.bic!
      }
    };

    // Produkte Positionsnummern zuweisen
    const productsWithPosition = (formValue.products || []).map((p: any, index: number) => ({
      ...p,
      position: index + 1
    }));

    const invoice = {
      id: 0,
      invoiceNumber: formValue.invoiceNumber || '',
      invoiceDate: formValue.invoiceDate,
      servicePeriod: formValue.servicePeriod,
      customerNumber: formValue.customerNumber || '',
      recipient: recipient,
      provider: providerObj,
      products: productsWithPosition,
      totalNet: this.totalNet(),
      totalTaxAmount: this.totalTax(),
      totalGross: this.totalGross(),
      dueDate: formValue.dueDate,
      paymentTerms: formValue.paymentTerms || '',
      isPaid: formValue.isPaid || false,
      introText: formValue.introText || '',
      outroText: formValue.outroText || ''
    };

    this.loading.set(true);

    if (this.mode() === 'new') {
      console.log(this.translationService.translate('invoices.detail.logs.sendingInvoiceData'), JSON.stringify(invoice, null, 2));
      this.invoiceService.createInvoice(invoice).subscribe({
        next: (created) => {
          alert(this.translationService.translate('invoices.detail.success.created'));
          this.router.navigate(['/invoices', created.id]);
        },
        error: (err) => {
          console.error(this.translationService.translate('invoices.detail.errors.createFailed'), err);
          console.error(this.translationService.translate('invoices.detail.logs.errorDetails'), JSON.stringify(err.error, null, 2));
          console.error(this.translationService.translate('invoices.detail.logs.invoicePayload'), JSON.stringify(invoice, null, 2));
          if (err.error?.errors) {
            console.error(this.translationService.translate('invoices.detail.logs.validationErrors'), err.error.errors);
            const errorMessages = Object.entries(err.error.errors)
              .map(([key, value]) => `${key}: ${Array.isArray(value) ? value.join(', ') : value}`)
              .join('\n');
            alert(`${this.translationService.translate('invoices.detail.errors.validationPrefix')}\n${errorMessages}`);
          } else {
            const errorMsg = err.error?.message || err.message || this.translationService.translate('invoices.detail.errors.unknown');
            alert(`${this.translationService.translate('invoices.detail.errors.createFailed')}\n${errorMsg}`);
          }
          this.loading.set(false);
        }
      });
    } else {
      const id = this.invoiceId();
      if (id) {
        this.invoiceService.updateInvoice(id, { ...invoice, id }).subscribe({
          next: () => {
            alert(this.translationService.translate('invoices.detail.success.updated'));
            this.router.navigate(['/invoices', id]);
          },
          error: (err) => {
            console.error(this.translationService.translate('invoices.detail.errors.updateFailed'), err);
            alert(this.translationService.translate('invoices.detail.errors.updateFailed'));
            this.loading.set(false);
          }
        });
      }
    }
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat(this.i18nService.currentLanguage().locale, {
      style: 'currency',
      currency: 'EUR'
    }).format(value);
  }

  private focusFirstInput() {
    setTimeout(() => {
      const firstInput = document.getElementById('invoiceDate') as HTMLInputElement | null;
      firstInput?.focus();
    }, 0);
  }
}
