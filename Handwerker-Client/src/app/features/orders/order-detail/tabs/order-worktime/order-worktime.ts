import { ChangeDetectionStrategy, Component, computed, inject, input, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import type { Order, WorkTimeEntry, WorkTimeEntryRequest } from '../../../../../core/entities/order.model';
import { OrderService } from '../../../../../core/services';

@Component({
  selector: 'app-order-worktime',
  standalone: true,
  imports: [ReactiveFormsModule, DatePipe],
  templateUrl: './order-worktime.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class OrderWorktime {
  private fb = inject(FormBuilder);
  private orderService = inject(OrderService);

  order = input<Order | null>(null);

  entries = computed(() => this.order()?.workTimeEntries || []);
  loading = signal(false);
  showAddForm = signal(false);
  editingEntry = signal<WorkTimeEntry | null>(null);

  form: FormGroup = this.fb.group({
    date: ['', Validators.required],
    startTime: ['08:00', Validators.required],
    endTime: ['16:00', Validators.required],
    breakDuration: ['00:30', Validators.required],
    description: ['', Validators.required],
    isBillable: [true],
    hourlyRate: ['']
  });

  totalHours = computed(() => {
    return this.entries().reduce((sum: number, entry: WorkTimeEntry) => {
      const [hours, minutes] = entry.totalHours.split(':').map(Number);
      return sum + hours + minutes / 60;
    }, 0);
  });

  billableHours = computed(() => {
    return this.entries()
      .filter((e: WorkTimeEntry) => e.isBillable)
      .reduce((sum: number, entry: WorkTimeEntry) => {
        const [hours, minutes] = entry.totalHours.split(':').map(Number);
        return sum + hours + minutes / 60;
      }, 0);
  });

  onAddEntry() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const orderId = this.order()?.id;
    if (!orderId) return;

    this.loading.set(true);

    const request: WorkTimeEntryRequest = {
      date: this.form.value.date,
      startTime: this.form.value.startTime,
      endTime: this.form.value.endTime,
      breakDuration: this.form.value.breakDuration,
      description: this.form.value.description,
      isBillable: this.form.value.isBillable,
      hourlyRate: this.form.value.hourlyRate ? parseFloat(this.form.value.hourlyRate) : undefined
    };

    this.orderService.addWorkTimeEntry(orderId, request).subscribe({
      next: () => {
        this.loading.set(false);
        this.showAddForm.set(false);
        this.form.reset({
          date: '',
          startTime: '08:00',
          endTime: '16:00',
          breakDuration: '00:30',
          description: '',
          isBillable: true,
          hourlyRate: ''
        });
        window.location.reload();
      },
      error: (err) => {
        this.loading.set(false);
        alert('Fehler beim Buchen: ' + err.error?.error);
      }
    });
  }

  onDeleteEntry(entryId: number) {
    if (!confirm('Diesen Zeiteintrag wirklich löschen?')) return;

    this.orderService.deleteWorkTimeEntry(entryId).subscribe({
      next: () => window.location.reload(),
      error: (err) => alert('Fehler beim Löschen: ' + err.error?.error)
    });
  }

  calculateDuration(start: string, end: string, breakTime: string): string {
    const [startH, startM] = start.split(':').map(Number);
    const [endH, endM] = end.split(':').map(Number);
    const [breakH, breakM] = breakTime.split(':').map(Number);

    let totalMinutes = (endH * 60 + endM) - (startH * 60 + startM) - (breakH * 60 + breakM);
    if (totalMinutes < 0) totalMinutes += 24 * 60;

    const hours = Math.floor(totalMinutes / 60);
    const minutes = totalMinutes % 60;

    return `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}`;
  }

  formatTime(timeStr: string): string {
    return timeStr.substring(0, 5);
  }
}
