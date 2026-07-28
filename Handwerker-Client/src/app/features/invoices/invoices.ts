import { ChangeDetectionStrategy, Component, computed, inject, signal, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Invoice } from '../../core/entities';
import { I18nService, InvoiceService, TranslationService } from '../../core/services';
import { TranslatePipe } from '../../shared';

type FilterType = 'all' | 'paid' | 'unpaid';

@Component({
  selector: 'app-invoices',
  imports: [TranslatePipe],
  templateUrl: './invoices.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class Invoices implements OnInit {
  invoiceService = inject(InvoiceService);
  router = inject(Router);
  translationService = inject(TranslationService);
  i18nService = inject(I18nService);

  invoices = signal<Invoice[]>([]);
  filter = signal<FilterType>('all');
  loading = signal(true);
  searchTerm = signal('');

  filteredInvoices = computed(() => {
    const filterValue = this.filter();
    const search = this.searchTerm().toLowerCase();
    let result = this.invoices();

    // Filter nach Status
    if (filterValue === 'paid') {
      result = result.filter(inv => inv.isPaid);
    } else if (filterValue === 'unpaid') {
      result = result.filter(inv => !inv.isPaid);
    }

    // Suche
    if (search) {
      result = result.filter(inv =>
        inv.invoiceNumber.toLowerCase().includes(search) ||
        inv.recipient.name.toLowerCase().includes(search) ||
        inv.customerNumber.toLowerCase().includes(search)
      );
    }

    // Sortierung nach Datum (neueste zuerst)
    return result.sort((a, b) =>
      new Date(b.invoiceDate).getTime() - new Date(a.invoiceDate).getTime()
    );
  });

  paidCount = computed(() => this.invoices().filter(inv => inv.isPaid).length);
  unpaidCount = computed(() => this.invoices().filter(inv => !inv.isPaid).length);

  ngOnInit() {
    this.loadInvoices();
  }

  loadInvoices() {
    this.loading.set(true);
    this.invoiceService.getInvoices().subscribe({
      next: (invoices) => {
        this.invoices.set(invoices);
        this.loading.set(false);
      },
      error: (err) => {
        console.error(this.translationService.translate('invoices.list.errors.loadFailed'), err);
        this.loading.set(false);
      }
    });
  }

  onFilterChange(filter: FilterType) {
    this.filter.set(filter);
  }

  onSearchChange(event: Event) {
    const input = event.target as HTMLInputElement;
    this.searchTerm.set(input.value);
  }

  onCreate() {
    this.router.navigate(['/invoices/new']);
  }

  onView(id: number) {
    this.router.navigate(['/invoices', id]);
  }

  onEdit(id: number) {
    this.router.navigate(['/invoices', id], { queryParams: { mode: 'edit' } });
  }

  onDelete(id: number, invoiceNumber: string) {
    if (confirm(`${this.translationService.translate('invoices.list.confirm.deletePrefix')} ${invoiceNumber} ${this.translationService.translate('invoices.list.confirm.deleteSuffix')}`)) {
      this.invoiceService.deleteInvoice(id).subscribe({
        next: () => {
          this.loadInvoices();
        },
        error: (err) => {
          console.error(this.translationService.translate('invoices.list.errors.deleteFailed'), err);
          alert(this.translationService.translate('invoices.list.errors.deleteFailed'));
        }
      });
    }
  }

  onConvertFromOffer() {
    const value = prompt(this.translationService.translate('invoices.list.prompt.offerId'));
    if (!value) {
      return;
    }

    const offerId = Number(value);
    if (Number.isNaN(offerId) || offerId <= 0) {
      alert(this.translationService.translate('invoices.list.errors.invalidOfferId'));
      return;
    }

    this.invoiceService.convertFromOffer(offerId, true).subscribe({
      next: created => {
        this.loadInvoices();
        alert(`${this.translationService.translate('invoices.list.success.convertPrefix')} ${created.invoiceNumber} ${this.translationService.translate('invoices.list.success.convertMiddle')} ${offerId} ${this.translationService.translate('invoices.list.success.convertSuffix')}`);
      },
      error: err => {
        console.error(this.translationService.translate('invoices.list.errors.convertFailed'), err);
        alert(this.translationService.translate('invoices.list.errors.convertFailed'));
      }
    });
  }

  formatDate(dateString: string): string {
    if (!dateString) return '-';
    const date = new Date(dateString);
    return date.toLocaleDateString(this.i18nService.currentLanguage().locale);
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat(this.i18nService.currentLanguage().locale, {
      style: 'currency',
      currency: 'EUR'
    }).format(value);
  }

  isOverdue(invoice: Invoice): boolean {
    if (invoice.isPaid) return false;
    const dueDate = new Date(invoice.dueDate);
    const today = new Date();
    return dueDate < today;
  }
}
