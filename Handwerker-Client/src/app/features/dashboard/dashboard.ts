import { ChangeDetectionStrategy, Component, OnInit, signal, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { take } from 'rxjs';
import { DashboardService, type DashboardStats, type MonthlyStats, type MonthlyAmounts, type InvoiceStats } from './services/dashboard.service';
import {AuthService} from '../../core/services';
import {TranslatePipe} from '../../shared';
@Component({
  selector: 'app-dashboard',
  imports: [CommonModule, TranslatePipe],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class Dashboard implements OnInit {
  authService = inject(AuthService);
  dashboardService = inject(DashboardService);
  router = inject(Router);
  stats = signal<DashboardStats>({
    offers: 0,
    invoices: 0,
    products: 0,
    recipients: 0
  });
  monthlyData = signal<MonthlyStats[]>([]);
  monthlyAmounts = signal<MonthlyAmounts[]>([]);
  invoiceStats = signal<InvoiceStats>({
    totalInvoices: 0,
    paidInvoices: 0,
    unpaidInvoices: 0,
    totalAmount: 0,
    paidAmount: 0,
    unpaidAmount: 0
  });
  // Computed für Prozentsätze
  paidPercentage = computed(() => {
    const stats = this.invoiceStats();
    return stats.totalInvoices > 0 
      ? Math.round((stats.paidInvoices / stats.totalInvoices) * 100)
      : 0;
  });
  unpaidPercentage = computed(() => {
    const stats = this.invoiceStats();
    return stats.totalInvoices > 0 
      ? Math.round((stats.unpaidInvoices / stats.totalInvoices) * 100)
      : 0;
  });
  tasks = signal<any[]>([]);
  userName = this.authService.userName;
  ngOnInit() {
    // Lade Dashboard-Daten
    this.loadDashboardData();
    this.loadMonthlyData();
    this.loadMonthlyAmounts();
    this.loadInvoiceStats();
  }
  private loadDashboardData() {
    console.log('🔄 Lade Dashboard-Statistiken...');
    this.dashboardService
      .getStats()
      .pipe(take(1))
      .subscribe({
        next: (stats) => {
          console.log('✅ Dashboard-Statistiken geladen:', stats);
          this.stats.set(stats);
        },
        error: (err) => {
          console.error('❌ Fehler beim Laden der Dashboard-Statistiken:', err);
          console.error('Error Details:', {
            status: err.status,
            statusText: err.statusText,
            message: err.message,
            url: err.url
          });
          // Fallback auf Mock-Daten bei Fehler
          this.stats.set({
            offers: 0,
            invoices: 0,
            products: 0,
            recipients: 0
          });
        }
      });
  }
  private loadMonthlyData() {
    console.log('🔄 Lade monatliche Statistiken...');
    this.dashboardService
      .getMonthlyStats()
      .pipe(take(1))
      .subscribe({
        next: (data) => {
          console.log('✅ Monatliche Statistiken geladen:', data);
          console.log('📊 Anzahl Monate:', data.length);
          if (data.length > 0) {
            console.log('📅 Erste Monatsdaten:', data[0]);
            console.log('📅 Letzte Monatsdaten:', data[data.length - 1]);
          }
          this.monthlyData.set(data);
        },
        error: (err) => {
          console.error('❌ Fehler beim Laden der monatlichen Statistiken:', err);
          console.error('Error Details:', {
            status: err.status,
            statusText: err.statusText,
            message: err.message,
            url: err.url
          });
          // Fallback auf leeres Array bei Fehler
          this.monthlyData.set([]);
        }
      });
  }
  private loadMonthlyAmounts() {
    console.log('🔄 Lade monatliche Rechnungsbeträge...');
    this.dashboardService
      .getMonthlyAmounts()
      .pipe(take(1))
      .subscribe({
        next: (data) => {
          console.log('✅ Monatliche Rechnungsbeträge geladen:', data);
          this.monthlyAmounts.set(data);
        },
        error: (err) => {
          console.error('❌ Fehler beim Laden der monatlichen Rechnungsbeträge:', err);
          this.monthlyAmounts.set([]);
        }
      });
  }
  private loadInvoiceStats() {
    console.log('🔄 Lade Rechnungsstatistik...');
    this.dashboardService
      .getInvoiceStats()
      .pipe(take(1))
      .subscribe({
        next: (data) => {
          console.log('✅ Rechnungsstatistik geladen:', data);
          this.invoiceStats.set(data);
        },
        error: (err) => {
          console.error('❌ Fehler beim Laden der Rechnungsstatistik:', err);
          this.invoiceStats.set({
            totalInvoices: 0,
            paidInvoices: 0,
            unpaidInvoices: 0,
            totalAmount: 0,
            paidAmount: 0,
            unpaidAmount: 0
          });
        }
      });
  }
  createOffer() {
    this.router.navigate(['/offers/new']);
  }
  createInvoice() {
    this.router.navigate(['/invoices/new']);
  }
  createProduct() {
    this.router.navigate(['/product']);
  }
  createRecipient() {
    this.router.navigate(['/recipients']);
  }
  createTask() {
    // TODO: Aufgaben-Feature implementieren
    console.log('Aufgabe erstellen - Feature noch nicht implementiert');
  }
  getMaxValue(): number {
    const allValues = this.monthlyData().flatMap(d => [d.invoices, d.offers]);
    return Math.max(...allValues, 1); // Mindestens 1 für Balkenberechnung
  }
  getBarHeight(value: number): number {
    const max = this.getMaxValue();
    return (value / max) * 100;
  }
  getMaxAmount(): number {
    const allAmounts = this.monthlyAmounts().map(d => d.totalGross);
    return Math.max(...allAmounts, 1);
  }
  getAmountBarHeight(amount: number): number {
    const max = this.getMaxAmount();
    return (amount / max) * 100;
  }
  formatCurrency(value: number): string {
    return new Intl.NumberFormat('de-DE', {
      style: 'currency',
      currency: 'EUR'
    }).format(value);
  }
  getTotalNet(): number {
    return this.monthlyAmounts().reduce((sum, m) => sum + m.totalNet, 0);
  }
  getTotalTax(): number {
    return this.monthlyAmounts().reduce((sum, m) => sum + m.totalTax, 0);
  }
  getTotalGross(): number {
    return this.monthlyAmounts().reduce((sum, m) => sum + m.totalGross, 0);
  }
}
