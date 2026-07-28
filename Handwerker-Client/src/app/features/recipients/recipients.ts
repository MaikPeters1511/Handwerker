import { Component, ChangeDetectionStrategy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl, FormGroup, Validators } from '@angular/forms';
import { RecipientService, Recipient } from './services/recipient.service';
import {TranslatePipe} from '../../shared';
import {TranslationService} from '../../core/services';
import {RecipientFormModel} from '../../core/interfaces/form/IRecipientFormModel';

@Component({
  selector: 'app-recipients',
  imports: [CommonModule, ReactiveFormsModule, TranslatePipe],
  templateUrl: './recipients.html',
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
export class Recipients {
  private recipientService = inject(RecipientService);
  private translationService = inject(TranslationService);

  // Search State
  searchTerm = signal<string>('');
  searchResults = signal<Recipient[]>([]);
  showDropdown = signal<boolean>(false);

  // Selected Recipient State
  selectedRecipient = signal<Recipient | null>(null);
  isEditing = signal<boolean>(false);
  isSaving = signal<boolean>(false);
  isDeleting = signal<boolean>(false);
  lastSaved = signal<Date | null>(null);

  // Toast State
  readonly saveSuccess = signal(false);
  readonly saveError = signal<string | null>(null);
  readonly successMessage = signal<string>('');

  // Signal-based Form
  recipientForm = new FormGroup<RecipientFormModel>({
    id: new FormControl<number>(0, { nonNullable: true }),
    customerNumber: new FormControl<string>('', { nonNullable: true, validators: [Validators.maxLength(150)] }),
    salutation: new FormControl<string>('', { nonNullable: true, validators: [Validators.maxLength(30)] }),
    name: new FormControl<string>('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(250)] }),
    contactPerson: new FormControl<string>('', { nonNullable: true, validators: [Validators.maxLength(250)] }),
    street: new FormControl<string>('', { nonNullable: true, validators: [Validators.maxLength(250)] }),
    addressLine2: new FormControl<string>('', { nonNullable: true, validators: [Validators.maxLength(250)] }),
    zipCode: new FormControl<string>('', { nonNullable: true, validators: [Validators.maxLength(10)] }),
    city: new FormControl<string>('', { nonNullable: true, validators: [Validators.maxLength(250)] }),
    country: new FormControl<string>('Deutschland', { nonNullable: true, validators: [Validators.maxLength(250)] }),
    email: new FormControl<string>('', { nonNullable: true, validators: [Validators.email] }),
    phone: new FormControl<string>('', { nonNullable: true })
  });

  onSearchInput(event: Event) {
    const input = event.target as HTMLInputElement;
    const term = input.value;
    this.searchTerm.set(term);

    if (term.length < 2) {
      this.recipientService.getRecipients().subscribe({
        next: (recipients) => {
          this.searchResults.set(recipients);
          this.showDropdown.set(recipients.length > 0);
        }
      });
      return;
    }

    // Filter locally (da Backend keine Search-Funktion hat)
    this.recipientService.getRecipients().subscribe({
      next: (recipients) => {
        const filtered = recipients.filter(r =>
          r.name.toLowerCase().includes(term.toLowerCase()) ||
          r.customerNumber.toLowerCase().includes(term.toLowerCase()) ||
          r.email.toLowerCase().includes(term.toLowerCase())
        );
        this.searchResults.set(filtered);
        this.showDropdown.set(filtered.length > 0);
      },
      error: () => {
        this.searchResults.set([]);
      }
    });
  }

  selectRecipient(recipient: Recipient) {
    this.selectedRecipient.set(recipient);
    this.isEditing.set(true);
    this.lastSaved.set(null);
    this.recipientForm.patchValue(recipient, { emitEvent: false });
    this.showDropdown.set(false);
    this.searchTerm.set('');
  }

  addNewRecipient() {
    this.selectedRecipient.set(null);
    this.isEditing.set(false);
    this.lastSaved.set(null);
    this.recipientForm.reset({
      id: 0,
      customerNumber: '',
      salutation: '',
      name: '',
      contactPerson: '',
      street: '',
      addressLine2: '',
      zipCode: '',
      city: '',
      country: 'Deutschland',
      email: '',
      phone: ''
    }, { emitEvent: false });
  }

  saveRecipient() {
    if (this.recipientForm.invalid) {
      this.recipientForm.markAllAsTouched();

      const invalidFields: string[] = [];
      Object.keys(this.recipientForm.controls).forEach(key => {
        const control = this.recipientForm.get(key);
        if (control && control.invalid) {
          invalidFields.push(`${key}: ${JSON.stringify(control.errors)}`);
        }
      });

      console.error('Form is invalid. Invalid fields:', invalidFields);
      console.error('Form value:', this.recipientForm.getRawValue());

      const validationError = this.translationService.translate('recipients.toast.validationError');
      this.saveError.set(`${validationError}: ${invalidFields.map(f => f.split(':')[0]).join(', ')}`);
      setTimeout(() => {
        this.saveError.set(null);
      }, 5000);
      return;
    }

    const formValue = this.recipientForm.getRawValue() as Recipient;
    console.log('Saving recipient:', formValue);
    this.isSaving.set(true);

    if (this.isEditing() && formValue.id) {
      console.log('Updating recipient with ID:', formValue.id);
      this.recipientService.updateRecipient(formValue.id, formValue).subscribe({
        next: () => {
          this.isSaving.set(false);
          this.lastSaved.set(new Date());

          this.successMessage.set(this.translationService.translate('recipients.toast.updated'));
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
          console.error('Error updating recipient:', err);
          const errorMsg = this.translationService.translate('recipients.toast.errorSave');
          this.saveError.set(`${errorMsg}: ${err?.error?.message || err?.message || 'Unknown error'}`);

          setTimeout(() => {
            this.saveError.set(null);
          }, 5000);
        }
      });
    } else {
      console.log('Creating new recipient');
      const { id, ...newRecipient } = formValue;
      this.recipientService.createRecipient(newRecipient).subscribe({
        next: (created) => {
          this.isSaving.set(false);
          this.lastSaved.set(new Date());

          this.successMessage.set(this.translationService.translate('recipients.toast.created'));
          this.saveSuccess.set(true);
          this.saveError.set(null);

          setTimeout(() => {
            this.saveSuccess.set(false);
          }, 5000);

          this.selectRecipient(created);
        },
        error: (err) => {
          this.isSaving.set(false);
          console.error('Error creating recipient:', err);
          const errorMsg = this.translationService.translate('recipients.toast.errorCreate');
          this.saveError.set(`${errorMsg}: ${err?.error?.message || err?.message || 'Unknown error'}`);

          setTimeout(() => {
            this.saveError.set(null);
          }, 5000);
        }
      });
    }
  }

  deleteRecipient() {
    const recipient = this.selectedRecipient();
    if (!recipient) return;

    if (!confirm(`Möchten Sie den Kunden "${recipient.name}" wirklich löschen?`)) {
      return;
    }

    this.isDeleting.set(true);
    this.recipientService.deleteRecipient(recipient.id).subscribe({
      next: () => {
        this.isDeleting.set(false);
        this.successMessage.set(this.translationService.translate('recipients.toast.deleted'));
        this.saveSuccess.set(true);
        this.saveError.set(null);

        setTimeout(() => {
          this.saveSuccess.set(false);
        }, 5000);

        this.addNewRecipient();

        // Refresh search results
        const currentTerm = this.searchTerm();
        if (currentTerm) {
          this.onSearchInput({ target: { value: currentTerm } } as any);
        }
      },
      error: (err) => {
        this.isDeleting.set(false);
        console.error('Error deleting recipient:', err);
        const errorMsg = this.translationService.translate('recipients.toast.errorDelete');
        this.saveError.set(`${errorMsg}: ${err?.error?.message || err?.message || 'Unknown error'}`);

        setTimeout(() => {
          this.saveError.set(null);
        }, 5000);
      }
    });
  }
}
