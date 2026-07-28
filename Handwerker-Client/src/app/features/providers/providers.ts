import { Component, ChangeDetectionStrategy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl, FormGroup, Validators } from '@angular/forms';
import { ProviderService } from './services/provider.service';
import {TranslatePipe} from '../../shared';
import {TranslationService} from '../../core/services';
import {Provider} from '../../core/entities';
import {ProviderFormModel} from '../../core/interfaces/form/IProviderFormModel';

@Component({
  selector: 'app-providers',
  imports: [CommonModule, ReactiveFormsModule, TranslatePipe],
  templateUrl: './providers.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  styles: [`
    @keyframes slideInRight {
      from {
        opacity: 0;
        transform: translateX(100%);
      }
      to {
        opacity: 1;
        transform: translateX(0);
      }
    }

    .toast .alert {
      animation: slideInRight 0.3s ease-out;
    }
  `]
})
export class Providers {
  private providerService = inject(ProviderService);
  private translationService = inject(TranslationService);

  // Search State
  searchTerm = signal<string>('');
  searchResults = signal<Provider[]>([]);
  showDropdown = signal<boolean>(false);

  // Selected Provider State
  selectedProvider = signal<Provider | null>(null);
  isEditing = signal<boolean>(false);
  isSaving = signal<boolean>(false);
  isDeleting = signal<boolean>(false);
  lastSaved = signal<Date | null>(null);

  // Toast State
  readonly saveSuccess = signal(false);
  readonly saveError = signal<string | null>(null);
  readonly successMessage = signal<string>('');

  // Signal-based Form
  providerForm = new FormGroup<ProviderFormModel>({
    id: new FormControl<number>(0, { nonNullable: true }),
    name: new FormControl<string>('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(250)] }),
    company: new FormControl<string>('', { nonNullable: true, validators: [Validators.maxLength(250)] }),
    street: new FormControl<string>('', { nonNullable: true, validators: [Validators.maxLength(250)] }),
    zipCode: new FormControl<string>('', { nonNullable: true, validators: [Validators.maxLength(10)] }),
    city: new FormControl<string>('', { nonNullable: true, validators: [Validators.maxLength(250)] }),
    email: new FormControl<string>('', { nonNullable: true, validators: [Validators.email] }),
    phone: new FormControl<string>('', { nonNullable: true }),
    taxId: new FormControl<string>('', { nonNullable: true, validators: [Validators.maxLength(50)] }),
    taxNumber: new FormControl<string>('', { nonNullable: true, validators: [Validators.maxLength(50)] }),
    commercialRegister: new FormControl<string>('', { nonNullable: true, validators: [Validators.maxLength(100)] }),
    registerCourt: new FormControl<string>('', { nonNullable: true, validators: [Validators.maxLength(100)] }),
    bankId: new FormControl<number | null>(null),
    bankName: new FormControl<string>('', { nonNullable: true, validators: [Validators.maxLength(250)] }),
    bankIban: new FormControl<string>('', { nonNullable: true, validators: [Validators.maxLength(34)] }),
    bankBic: new FormControl<string>('', { nonNullable: true, validators: [Validators.maxLength(11)] }),
    bankPlz: new FormControl<string>('', { nonNullable: true, validators: [Validators.maxLength(10)] }),
    bankOrt: new FormControl<string>('', { nonNullable: true, validators: [Validators.maxLength(100)] })
  });

  onSearchInput(event: Event) {
    const input = event.target as HTMLInputElement;
    const term = input.value;
    this.searchTerm.set(term);

    if (term.length < 2) {
      this.providerService.getProviders().subscribe({
        next: (providers) => {
          this.searchResults.set(providers);
          this.showDropdown.set(providers.length > 0);
        }
      });
      return;
    }

    // Filter locally
    this.providerService.getProviders().subscribe({
      next: (providers) => {
        const filtered = providers.filter(p =>
          p.name.toLowerCase().includes(term.toLowerCase()) ||
          p.company.toLowerCase().includes(term.toLowerCase()) ||
          p.email.toLowerCase().includes(term.toLowerCase())
        );
        this.searchResults.set(filtered);
        this.showDropdown.set(filtered.length > 0);
      },
      error: () => {
        this.searchResults.set([]);
      }
    });
  }

  selectProvider(provider: Provider) {
    this.selectedProvider.set(provider);
    this.isEditing.set(true);
    this.lastSaved.set(null);
    this.providerForm.patchValue({
      ...provider,
      bankId: provider.bank?.id || null,
      bankName: provider.bank?.name || '',
      bankIban: provider.bank?.iban || '',
      bankBic: provider.bank?.bic || '',
      bankPlz: provider.bank?.plz || '',
      bankOrt: provider.bank?.ort || ''
    }, { emitEvent: false });
    this.showDropdown.set(false);
    this.searchTerm.set('');
  }

  addNewProvider() {
    this.selectedProvider.set(null);
    this.isEditing.set(false);
    this.lastSaved.set(null);
    this.providerForm.reset({
      id: 0,
      name: '',
      company: '',
      street: '',
      zipCode: '',
      city: '',
      email: '',
      phone: '',
      taxId: '',
      taxNumber: '',
      commercialRegister: '',
      registerCourt: '',
      bankId: null,
      bankName: '',
      bankIban: '',
      bankBic: '',
      bankPlz: '',
      bankOrt: ''
    }, { emitEvent: false });
  }

  saveProvider() {
    if (this.providerForm.invalid) {
      this.providerForm.markAllAsTouched();

      const invalidFields: string[] = [];
      Object.keys(this.providerForm.controls).forEach(key => {
        const control = this.providerForm.get(key);
        if (control && control.invalid) {
          invalidFields.push(`${key}: ${JSON.stringify(control.errors)}`);
        }
      });

      console.error('Form is invalid. Invalid fields:', invalidFields);
      console.error('Form value:', this.providerForm.getRawValue());

      const validationError = this.translationService.translate('providers.toast.validationError');
      this.saveError.set(`${validationError}: ${invalidFields.map(f => f.split(':')[0]).join(', ')}`);
      setTimeout(() => {
        this.saveError.set(null);
      }, 5000);
      return;
    }

    const formValue = this.providerForm.getRawValue();
    const providerData: Provider = {
      id: formValue.id,
      name: formValue.name,
      company: formValue.company,
      street: formValue.street,
      zipCode: formValue.zipCode,
      city: formValue.city,
      email: formValue.email,
      phone: formValue.phone,
      taxId: formValue.taxId,
      taxNumber: formValue.taxNumber,
      commercialRegister: formValue.commercialRegister,
      registerCourt: formValue.registerCourt,
      bank: {
        id: formValue.bankId || 0,
        name: formValue.bankName,
        iban: formValue.bankIban,
        bic: formValue.bankBic,
        plz: formValue.bankPlz,
        ort: formValue.bankOrt
      }
    };

    console.log('Saving provider:', providerData);
    this.isSaving.set(true);

    if (this.isEditing() && providerData.id) {
      console.log('Updating provider with ID:', providerData.id);
      this.providerService.updateProvider(providerData.id, providerData).subscribe({
        next: () => {
          this.isSaving.set(false);
          this.lastSaved.set(new Date());

          this.successMessage.set(this.translationService.translate('providers.toast.updated'));
          this.saveSuccess.set(true);
          this.saveError.set(null);

          setTimeout(() => {
            this.saveSuccess.set(false);
          }, 5000);

          const currentTerm = this.searchTerm();
          if (currentTerm && currentTerm.length >= 2) {
            this.onSearchInput({ target: { value: currentTerm } } as any);
          }
        },
        error: (err) => {
          this.isSaving.set(false);
          console.error('Error updating provider:', err);
          const errorMsg = this.translationService.translate('providers.toast.errorSave');
          this.saveError.set(`${errorMsg}: ${err?.error?.message || err?.message || 'Unknown error'}`);

          setTimeout(() => {
            this.saveError.set(null);
          }, 5000);
        }
      });
    } else {
      console.log('Creating new provider');
      const { id, ...newProvider } = providerData;
      this.providerService.createProvider(newProvider).subscribe({
        next: (created) => {
          this.isSaving.set(false);
          this.lastSaved.set(new Date());

          this.successMessage.set(this.translationService.translate('providers.toast.created'));
          this.saveSuccess.set(true);
          this.saveError.set(null);

          setTimeout(() => {
            this.saveSuccess.set(false);
          }, 5000);

          this.selectProvider(created);
        },
        error: (err) => {
          this.isSaving.set(false);
          console.error('Error creating provider:', err);
          const errorMsg = this.translationService.translate('providers.toast.errorCreate');
          this.saveError.set(`${errorMsg}: ${err?.error?.message || err?.message || 'Unknown error'}`);

          setTimeout(() => {
            this.saveError.set(null);
          }, 5000);
        }
      });
    }
  }

  deleteProvider() {
    const provider = this.selectedProvider();
    if (!provider) return;

    if (!confirm(`Möchten Sie den Lieferanten "${provider.name}" wirklich löschen?`)) {
      return;
    }

    this.isDeleting.set(true);
    this.providerService.deleteProvider(provider.id).subscribe({
      next: () => {
        this.isDeleting.set(false);
        this.successMessage.set(this.translationService.translate('providers.toast.deleted'));
        this.saveSuccess.set(true);
        this.saveError.set(null);

        setTimeout(() => {
          this.saveSuccess.set(false);
        }, 5000);

        this.addNewProvider();

        // Refresh search results
        const currentTerm = this.searchTerm();
        if (currentTerm) {
          this.onSearchInput({ target: { value: currentTerm } } as any);
        }
      },
      error: (err) => {
        this.isDeleting.set(false);
        console.error('Error deleting provider:', err);
        const errorMsg = this.translationService.translate('providers.toast.errorDelete');
        this.saveError.set(`${errorMsg}: ${err?.error?.message || err?.message || 'Unknown error'}`);

        setTimeout(() => {
          this.saveError.set(null);
        }, 5000);
      }
    });
  }
}
