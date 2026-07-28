import { ChangeDetectionStrategy, Component, computed, effect, inject, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Order, OrderStatus, Priority, Recipient, Provider, Product } from '../../../core/entities/';
import { OrderService } from '../../../core/services';
import { I18nService } from '../../../core/services';
import { OrderMaterials } from './tabs/order-materials/order-materials';
import { OrderWorktime } from './tabs/order-worktime/order-worktime';

@Component({
  selector: 'app-order-detail',
  standalone: true,
  imports: [ReactiveFormsModule, OrderMaterials, OrderWorktime],
  templateUrl: './order-detail.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class OrderDetail {
  private fb = inject(FormBuilder);
  private orderService = inject(OrderService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private i18nService = inject(I18nService);

  order = signal<Order | null>(null);
  loading = signal(true);
  saving = signal(false);
  isEditMode = signal(false);
  isNew = signal(false);
  activeTab = signal<'overview' | 'materials' | 'worktime'>('overview');

  form: FormGroup = this.fb.group({
    orderDate: ['', Validators.required],
    customerNumber: ['', Validators.required],
    recipientName: ['', Validators.required],
    recipientStreet: [''],
    recipientPostalCode: [''],
    recipientCity: [''],
    priority: ['Normal', Validators.required],
    status: ['Draft', Validators.required],
    plannedStartDate: [''],
    plannedEndDate: [''],
    estimatedHours: [0, [Validators.required, Validators.min(0)]],
    description: [''],
    internalNotes: ['']
  });

  availableStatuses = ['Draft', 'Planned', 'InProgress', 'Completed', 'Invoiced', 'Cancelled'] as OrderStatus[];
  availablePriorities = ['Low', 'Normal', 'High', 'Urgent'] as Priority[];

  constructor() {
    effect(() => {
      const id = this.route.snapshot.params['id'];
      const mode = this.route.snapshot.queryParams['mode'];
      const routePath = this.route.snapshot.routeConfig?.path;
      const isNewRoute = id === 'new' || routePath === 'orders/new';

      if (isNewRoute) {
        this.isNew.set(true);
        this.isEditMode.set(true);
        this.loading.set(false);
      } else if (id && !Number.isNaN(Number(id))) {
        this.loadOrder(Number(id));
        this.isEditMode.set(mode === 'edit');
      } else {
        // Defensive fallback: Route ohne gültigen Auftrag darf nie im Loader hängen bleiben
        this.loading.set(false);
      }
    });
  }

  loadOrder(id: number) {
    this.loading.set(true);
    this.orderService.getOrder(id).subscribe({
      next: (order) => {
        this.order.set(order);
        this.form.patchValue({
          orderDate: order.orderDate,
          customerNumber: order.customerNumber,
          recipientName: order.recipient?.name,
          recipientStreet: order.recipient?.street,
          recipientPostalCode: order.recipient?.postalCode,
          recipientCity: order.recipient?.city,
          priority: order.priority,
          status: order.status,
          plannedStartDate: order.plannedStartDate,
          plannedEndDate: order.plannedEndDate,
          estimatedHours: order.estimatedHours,
          description: order.description,
          internalNotes: order.internalNotes
        });
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error loading order:', err);
        this.loading.set(false);
        alert('Fehler beim Laden des Auftrags');
        this.router.navigate(['/orders']);
      }
    });
  }

  onEdit() {
    this.isEditMode.set(true);
  }

  onCancel() {
    if (this.isNew()) {
      this.router.navigate(['/orders']);
    } else {
      this.isEditMode.set(false);
      const order = this.order();
      if (order) {
        this.form.patchValue({
          orderDate: order.orderDate,
          customerNumber: order.customerNumber,
          recipientName: order.recipient?.name,
          recipientStreet: order.recipient?.street,
          recipientPostalCode: order.recipient?.postalCode,
          recipientCity: order.recipient?.city,
          priority: order.priority,
          status: order.status,
          plannedStartDate: order.plannedStartDate,
          plannedEndDate: order.plannedEndDate,
          estimatedHours: order.estimatedHours,
          description: order.description,
          internalNotes: order.internalNotes
        });
      }
    }
  }

  onSave() {
    if (this.form.invalid) {
      this.markFormGroupTouched(this.form);
      return;
    }

    this.saving.set(true);
    const formValue = this.form.value;

    const recipient: Recipient = {
      id: 0,
      customerNumber: formValue.customerNumber || '',
      salutation: '',
      name: formValue.recipientName,
      contactPerson: '',
      street: formValue.recipientStreet,
      addressLine2: '',
      zipCode: formValue.recipientPostalCode || '',
      postalCode: formValue.recipientPostalCode,
      city: formValue.recipientCity,
      country: '',
      email: '',
      phone: ''
    };

    if (this.isNew()) {
      // Für neue Aufträge brauchen wir auch Provider - hier vereinfacht
      const provider: Provider = {
        id: 0,
        name: 'Ihr Handwerker',
        company: '',
        street: '',
        zipCode: '',
        city: '',
        phone: '',
        email: '',
        website: '',
        taxId: '',
        taxNumber: '',
        commercialRegister: '',
        registerCourt: '',
        bank: {
          id: 0,
          name: '',
          iban: '',
          bic: '',
          plz: '',
          ort: ''
        }
      };

      this.orderService.createOrder({
        orderDate: formValue.orderDate,
        customerNumber: formValue.customerNumber,
        recipient: recipient,
        provider: provider,
        products: [],
        priority: formValue.priority,
        plannedStartDate: formValue.plannedStartDate,
        plannedEndDate: formValue.plannedEndDate,
        estimatedHours: formValue.estimatedHours,
        description: formValue.description,
        internalNotes: formValue.internalNotes
      }).subscribe({
        next: (order) => {
          this.saving.set(false);
          this.router.navigate(['/orders', order.id]);
        },
        error: (err) => {
          console.error('Error creating order:', err);
          this.saving.set(false);
          alert('Fehler beim Erstellen des Auftrags');
        }
      });
    } else {
      const order = this.order();
      if (!order) return;

      this.orderService.updateOrder(order.id, {
        id: order.id,
        orderDate: formValue.orderDate,
        customerNumber: formValue.customerNumber,
        recipient: recipient,
        provider: order.provider,
        products: order.products,
        priority: formValue.priority,
        plannedStartDate: formValue.plannedStartDate,
        plannedEndDate: formValue.plannedEndDate,
        estimatedHours: formValue.estimatedHours,
        description: formValue.description,
        internalNotes: formValue.internalNotes
      }).subscribe({
        next: () => {
          this.saving.set(false);
          this.isEditMode.set(false);
          this.loadOrder(order.id);
        },
        error: (err) => {
          console.error('Error updating order:', err);
          this.saving.set(false);
          alert('Fehler beim Aktualisieren des Auftrags');
        }
      });
    }
  }

  onBack() {
    this.router.navigate(['/orders']);
  }

  onStatusChange(newStatus: OrderStatus) {
    const order = this.order();
    if (!order) return;

    this.orderService.updateStatus(order.id, { status: newStatus }).subscribe({
      next: () => {
        this.loadOrder(order.id);
      },
      error: (err) => {
        alert('Fehler beim Ändern des Status: ' + err.error?.error);
      }
    });
  }

  onConvertToInvoice() {
    const order = this.order();
    if (!order) return;

    if (!confirm('Möchten Sie diesen Auftrag wirklich in eine Rechnung umwandeln?')) return;

    this.orderService.convertToInvoice(order.id).subscribe({
      next: (result) => {
        alert(`Rechnung ${result.invoiceNumber} wurde erstellt`);
        this.loadOrder(order.id);
      },
      error: (err) => {
        alert('Fehler: ' + err.error?.error);
      }
    });
  }

  markFormGroupTouched(formGroup: FormGroup) {
    Object.values(formGroup.controls).forEach(control => {
      control.markAsTouched();
      if ((control as any).controls) {
        this.markFormGroupTouched(control as FormGroup);
      }
    });
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat(this.i18nService.currentLanguage().locale, {
      style: 'currency',
      currency: 'EUR'
    }).format(value);
  }

  getStatusLabel(status: OrderStatus): string {
    return this.orderService.getStatusLabel(status);
  }

  getStatusColor(status: OrderStatus): string {
    return this.orderService.getStatusColor(status);
  }

  getPriorityLabel(priority: Priority): string {
    return this.orderService.getPriorityLabel(priority);
  }

  getPriorityColor(priority: Priority): string {
    return this.orderService.getPriorityColor(priority);
  }

  setTab(tab: 'overview' | 'materials' | 'worktime') {
    this.activeTab.set(tab);
  }
}
