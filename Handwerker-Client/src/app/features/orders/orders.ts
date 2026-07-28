import { ChangeDetectionStrategy, Component, computed, inject, OnInit, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { Router } from '@angular/router';
import {I18nService, OrderService} from '../../core/services';
import {Order} from '../../core/entities';

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [DatePipe],
  templateUrl: './orders.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class Orders implements OnInit {
  orderService = inject(OrderService);
  router = inject(Router);
  i18nService = inject(I18nService);

  orders = signal<Order[]>([]);
  loading = signal(true);
  searchTerm = signal('');
  statusFilter = signal<string>('all');

  filteredOrders = computed(() => {
    const search = this.searchTerm().toLowerCase();
    const status = this.statusFilter();
    let result = this.orders();

    if (status !== 'all') {
      result = result.filter(o => o.status === status);
    }

    if (search) {
      result = result.filter(o =>
        o.orderNumber.toLowerCase().includes(search) ||
        o.customerNumber.toLowerCase().includes(search) ||
        o.recipient?.name?.toLowerCase().includes(search) ||
        o.description?.toLowerCase().includes(search)
      );
    }

    return result.sort((a, b) => new Date(b.orderDate).getTime() - new Date(a.orderDate).getTime());
  });

  ngOnInit() {
    this.loadOrders();
  }

  loadOrders() {
    this.loading.set(true);
    this.orderService.getOrders().subscribe({
      next: (orders) => {
        this.orders.set(orders);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error loading orders:', err);
        this.loading.set(false);
      }
    });
  }

  onSearchChange(event: Event) {
    const input = event.target as HTMLInputElement;
    this.searchTerm.set(input.value);
  }

  onStatusChange(event: Event) {
    const select = event.target as HTMLSelectElement;
    this.statusFilter.set(select.value);
  }

  onCreate() {
    this.router.navigate(['/orders/new']);
  }

  onView(id: number) {
    this.router.navigate(['/orders', id]);
  }

  onEdit(id: number) {
    this.router.navigate(['/orders', id], { queryParams: { mode: 'edit' } });
  }

  onDelete(id: number, orderNumber: string) {
    if (confirm(`Auftrag ${orderNumber} wirklich löschen?`)) {
      this.orderService.deleteOrder(id).subscribe({
        next: () => this.loadOrders(),
        error: (err) => {
          console.error('Error deleting order:', err);
          alert('Fehler beim Löschen des Auftrags');
        }
      });
    }
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat(this.i18nService.currentLanguage().locale, {
      style: 'currency',
      currency: 'EUR'
    }).format(value);
  }

  getStatusLabel(status: string): string {
    return this.orderService.getStatusLabel(status as any);
  }

  getStatusColor(status: string): string {
    return this.orderService.getStatusColor(status as any);
  }

  getPriorityLabel(priority: string): string {
    return this.orderService.getPriorityLabel(priority);
  }

  getPriorityColor(priority: string): string {
    return this.orderService.getPriorityColor(priority);
  }
}
