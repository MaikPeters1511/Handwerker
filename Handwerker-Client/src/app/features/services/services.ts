import { Component, ChangeDetectionStrategy, OnInit, ViewChild, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl, FormGroup, Validators } from '@angular/forms';
import { TranslatePipe } from '../../shared';
import { ServiceItemService } from '../../core/services';
import { TranslationService } from '../../core/services';
import { ServiceItem, CreateServiceItemRequest, UpdateServiceItemRequest } from '../../core/entities';
import { ServiceItemFormModel } from '../../core/interfaces/form/IServiceItemFormModel';
import { DeleteComponent } from '../../shared/components/delete-component/delete-component';

@Component({
  selector: 'app-services',
  imports: [CommonModule, ReactiveFormsModule, TranslatePipe, DeleteComponent],
  templateUrl: './services.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class Services implements OnInit {
  private serviceItemService = inject(ServiceItemService);
  private translationService = inject(TranslationService);

  @ViewChild('deleteModal') deleteModal!: DeleteComponent;

  services = signal<ServiceItem[]>([]);
  isLoadingList = signal<boolean>(false);

  searchTerm = signal<string>('');
  searchResults = signal<ServiceItem[]>([]);
  showDropdown = signal<boolean>(false);

  selectedService = signal<ServiceItem | null>(null);
  isEditing = signal<boolean>(false);
  isSaving = signal<boolean>(false);
  isDeleting = signal<boolean>(false);
  lastSaved = signal<Date | null>(null);

  readonly saveSuccess = signal(false);
  readonly saveError = signal<string | null>(null);
  readonly successMessage = signal<string>('');

  serviceForm = new FormGroup<ServiceItemFormModel>({
    id: new FormControl<number>(0, { nonNullable: true }),
    serviceNumber: new FormControl<string>('', { nonNullable: true }),
    name: new FormControl<string>('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(255)] }),
    description: new FormControl<string>('', { nonNullable: true, validators: [Validators.maxLength(2000)] }),
    unit: new FormControl<string>('', { nonNullable: true, validators: [Validators.maxLength(50)] }),
    unitPrice: new FormControl<number>(0, { nonNullable: true, validators: [Validators.required, Validators.min(0)] }),
    taxRate: new FormControl<number>(19, { nonNullable: true, validators: [Validators.required, Validators.min(0), Validators.max(100)] }),
    isActive: new FormControl<boolean>(true, { nonNullable: true })
  });

  ngOnInit() {
    this.loadServices();
  }

  loadServices() {
    this.isLoadingList.set(true);
    this.serviceItemService.getServices().subscribe({
      next: (services) => {
        this.services.set(services);
        this.isLoadingList.set(false);
      },
      error: () => this.isLoadingList.set(false)
    });
  }

  onSearchInput(event: Event) {
    const term = (event.target as HTMLInputElement).value;
    this.searchTerm.set(term);

    if (term.length < 2) {
      this.searchResults.set([]);
      this.showDropdown.set(false);
      return;
    }

    setTimeout(() => {
      if (this.searchTerm() !== term) return;

      this.serviceItemService.searchServices(term).subscribe({
        next: (services) => {
          this.searchResults.set(services);
          this.showDropdown.set(services.length > 0);
        },
        error: () => this.searchResults.set([])
      });
    }, 300);
  }

  manualSearch() {
    const term = this.searchTerm();
    if (term.length < 2) {
      this.searchResults.set([]);
      this.showDropdown.set(false);
      return;
    }

    this.serviceItemService.searchServices(term).subscribe({
      next: (services) => {
        this.searchResults.set(services);
        this.showDropdown.set(services.length > 0);
      },
      error: () => this.searchResults.set([])
    });
  }

  selectService(service: ServiceItem) {
    this.selectedService.set(service);
    this.isEditing.set(true);
    this.lastSaved.set(null);
    this.serviceForm.patchValue(service, { emitEvent: false });
    this.showDropdown.set(false);
    this.searchTerm.set('');
  }

  addNewService() {
    this.selectedService.set(null);
    this.isEditing.set(false);
    this.lastSaved.set(null);
    this.serviceForm.reset({
      id: 0,
      serviceNumber: '',
      name: '',
      description: '',
      unit: '',
      unitPrice: 0,
      taxRate: 19,
      isActive: true
    }, { emitEvent: false });
  }

  saveService() {
    if (this.serviceForm.invalid) {
      this.serviceForm.markAllAsTouched();
      this.saveError.set(this.translationService.translate('services.toast.validationError'));
      setTimeout(() => this.saveError.set(null), 5000);
      return;
    }

    const formValue = this.serviceForm.getRawValue();
    this.isSaving.set(true);

    if (this.isEditing() && formValue.id) {
      const request: UpdateServiceItemRequest = {
        id: formValue.id,
        name: formValue.name,
        description: formValue.description,
        unit: formValue.unit,
        unitPrice: formValue.unitPrice,
        taxRate: formValue.taxRate,
        isActive: formValue.isActive
      };

      this.serviceItemService.updateService(formValue.id, request).subscribe({
        next: () => {
          this.isSaving.set(false);
          this.lastSaved.set(new Date());
          this.successMessage.set(this.translationService.translate('services.toast.updated'));
          this.saveSuccess.set(true);
          setTimeout(() => this.saveSuccess.set(false), 5000);
          this.loadServices();
        },
        error: (err) => {
          this.isSaving.set(false);
          this.saveError.set(`${this.translationService.translate('services.toast.errorSave')}: ${err?.error?.message || err?.message || 'Unknown error'}`);
          setTimeout(() => this.saveError.set(null), 5000);
        }
      });
    } else {
      const request: CreateServiceItemRequest = {
        name: formValue.name,
        description: formValue.description,
        unit: formValue.unit,
        unitPrice: formValue.unitPrice,
        taxRate: formValue.taxRate
      };

      this.serviceItemService.createService(request).subscribe({
        next: (created) => {
          this.isSaving.set(false);
          this.lastSaved.set(new Date());
          this.successMessage.set(this.translationService.translate('services.toast.created'));
          this.saveSuccess.set(true);
          setTimeout(() => this.saveSuccess.set(false), 5000);
          this.selectService(created);
          this.loadServices();
        },
        error: (err) => {
          this.isSaving.set(false);
          this.saveError.set(`${this.translationService.translate('services.toast.errorCreate')}: ${err?.error?.message || err?.message || 'Unknown error'}`);
          setTimeout(() => this.saveError.set(null), 5000);
        }
      });
    }
  }

  deleteSelectedService() {
    const service = this.selectedService();
    if (!service) return;
    this.deleteModal.open(String(service.id));
  }

  onDeleteConfirmed(id: string | null) {
    if (!id) return;
    const serviceId = Number(id);

    this.isDeleting.set(true);
    this.serviceItemService.deleteService(serviceId).subscribe({
      next: () => {
        this.isDeleting.set(false);
        this.successMessage.set(this.translationService.translate('services.toast.deleted'));
        this.saveSuccess.set(true);
        setTimeout(() => this.saveSuccess.set(false), 5000);
        this.addNewService();
        this.loadServices();
      },
      error: (err) => {
        this.isDeleting.set(false);
        this.saveError.set(`${this.translationService.translate('services.toast.errorDelete')}: ${err?.error?.message || err?.message || 'Unknown error'}`);
        setTimeout(() => this.saveError.set(null), 5000);
      }
    });
  }
}
