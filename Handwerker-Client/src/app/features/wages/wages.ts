import { Component, ChangeDetectionStrategy, OnInit, ViewChild, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl, FormGroup, Validators } from '@angular/forms';
import { TranslatePipe } from '../../shared';
import { WageTypeService } from '../../core/services';
import { TranslationService } from '../../core/services';
import { WageType, CreateWageTypeRequest, UpdateWageTypeRequest } from '../../core/entities';
import { WageTypeFormModel } from '../../core/interfaces/form/IWageTypeFormModel';
import { DeleteComponent } from '../../shared/components/delete-component/delete-component';

@Component({
  selector: 'app-wages',
  imports: [CommonModule, ReactiveFormsModule, TranslatePipe, DeleteComponent],
  templateUrl: './wages.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class Wages implements OnInit {
  private wageTypeService = inject(WageTypeService);
  private translationService = inject(TranslationService);

  @ViewChild('deleteModal') deleteModal!: DeleteComponent;

  wageTypes = signal<WageType[]>([]);
  isLoadingList = signal<boolean>(false);

  searchTerm = signal<string>('');
  searchResults = signal<WageType[]>([]);
  showDropdown = signal<boolean>(false);

  selectedWageType = signal<WageType | null>(null);
  isEditing = signal<boolean>(false);
  isSaving = signal<boolean>(false);
  isDeleting = signal<boolean>(false);
  lastSaved = signal<Date | null>(null);

  readonly saveSuccess = signal(false);
  readonly saveError = signal<string | null>(null);
  readonly successMessage = signal<string>('');

  wageForm = new FormGroup<WageTypeFormModel>({
    id: new FormControl<number>(0, { nonNullable: true }),
    wageNumber: new FormControl<string>('', { nonNullable: true }),
    name: new FormControl<string>('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(255)] }),
    description: new FormControl<string>('', { nonNullable: true, validators: [Validators.maxLength(2000)] }),
    hourlyRate: new FormControl<number>(0, { nonNullable: true, validators: [Validators.required, Validators.min(0)] }),
    taxRate: new FormControl<number>(19, { nonNullable: true, validators: [Validators.required, Validators.min(0), Validators.max(100)] }),
    isActive: new FormControl<boolean>(true, { nonNullable: true })
  });

  ngOnInit() {
    this.loadWageTypes();
  }

  loadWageTypes() {
    this.isLoadingList.set(true);
    this.wageTypeService.getWageTypes().subscribe({
      next: (wageTypes) => {
        this.wageTypes.set(wageTypes);
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

      this.wageTypeService.searchWageTypes(term).subscribe({
        next: (wageTypes) => {
          this.searchResults.set(wageTypes);
          this.showDropdown.set(wageTypes.length > 0);
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

    this.wageTypeService.searchWageTypes(term).subscribe({
      next: (wageTypes) => {
        this.searchResults.set(wageTypes);
        this.showDropdown.set(wageTypes.length > 0);
      },
      error: () => this.searchResults.set([])
    });
  }

  selectWageType(wageType: WageType) {
    this.selectedWageType.set(wageType);
    this.isEditing.set(true);
    this.lastSaved.set(null);
    this.wageForm.patchValue(wageType, { emitEvent: false });
    this.showDropdown.set(false);
    this.searchTerm.set('');
  }

  addNewWageType() {
    this.selectedWageType.set(null);
    this.isEditing.set(false);
    this.lastSaved.set(null);
    this.wageForm.reset({
      id: 0,
      wageNumber: '',
      name: '',
      description: '',
      hourlyRate: 0,
      taxRate: 19,
      isActive: true
    }, { emitEvent: false });
  }

  saveWageType() {
    if (this.wageForm.invalid) {
      this.wageForm.markAllAsTouched();
      this.saveError.set(this.translationService.translate('wages.toast.validationError'));
      setTimeout(() => this.saveError.set(null), 5000);
      return;
    }

    const formValue = this.wageForm.getRawValue();
    this.isSaving.set(true);

    if (this.isEditing() && formValue.id) {
      const request: UpdateWageTypeRequest = {
        id: formValue.id,
        name: formValue.name,
        description: formValue.description,
        hourlyRate: formValue.hourlyRate,
        taxRate: formValue.taxRate,
        isActive: formValue.isActive
      };

      this.wageTypeService.updateWageType(formValue.id, request).subscribe({
        next: () => {
          this.isSaving.set(false);
          this.lastSaved.set(new Date());
          this.successMessage.set(this.translationService.translate('wages.toast.updated'));
          this.saveSuccess.set(true);
          setTimeout(() => this.saveSuccess.set(false), 5000);
          this.loadWageTypes();
        },
        error: (err) => {
          this.isSaving.set(false);
          this.saveError.set(`${this.translationService.translate('wages.toast.errorSave')}: ${err?.error?.message || err?.message || 'Unknown error'}`);
          setTimeout(() => this.saveError.set(null), 5000);
        }
      });
    } else {
      const request: CreateWageTypeRequest = {
        name: formValue.name,
        description: formValue.description,
        hourlyRate: formValue.hourlyRate,
        taxRate: formValue.taxRate
      };

      this.wageTypeService.createWageType(request).subscribe({
        next: (created) => {
          this.isSaving.set(false);
          this.lastSaved.set(new Date());
          this.successMessage.set(this.translationService.translate('wages.toast.created'));
          this.saveSuccess.set(true);
          setTimeout(() => this.saveSuccess.set(false), 5000);
          this.selectWageType(created);
          this.loadWageTypes();
        },
        error: (err) => {
          this.isSaving.set(false);
          this.saveError.set(`${this.translationService.translate('wages.toast.errorCreate')}: ${err?.error?.message || err?.message || 'Unknown error'}`);
          setTimeout(() => this.saveError.set(null), 5000);
        }
      });
    }
  }

  deleteSelectedWageType() {
    const wageType = this.selectedWageType();
    if (!wageType) return;
    this.deleteModal.open(String(wageType.id));
  }

  onDeleteConfirmed(id: string | null) {
    if (!id) return;
    const wageTypeId = Number(id);

    this.isDeleting.set(true);
    this.wageTypeService.deleteWageType(wageTypeId).subscribe({
      next: () => {
        this.isDeleting.set(false);
        this.successMessage.set(this.translationService.translate('wages.toast.deleted'));
        this.saveSuccess.set(true);
        setTimeout(() => this.saveSuccess.set(false), 5000);
        this.addNewWageType();
        this.loadWageTypes();
      },
      error: (err) => {
        this.isDeleting.set(false);
        this.saveError.set(`${this.translationService.translate('wages.toast.errorDelete')}: ${err?.error?.message || err?.message || 'Unknown error'}`);
        setTimeout(() => this.saveError.set(null), 5000);
      }
    });
  }
}
