import { Component, ChangeDetectionStrategy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl, FormGroup, Validators } from '@angular/forms';
import {TranslatePipe} from '../../shared';
import {Company, CreateCompanyRequest} from '../../core/entities';
import {CompanyService} from './services/company.service';

@Component({
  selector: 'app-company',
  imports: [CommonModule, ReactiveFormsModule, TranslatePipe],
  templateUrl: './company.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CompanyPage {
  companyService = inject(CompanyService);

  companies = signal<Company[]>([]);
  selected = signal<Company | null>(null);
  isLoading = signal(false);

  // File upload state
  selectedFile = signal<File | null>(null);
  previewUrl = signal<string | null>(null);
  uploadError = signal<string | null>(null);
  isUploading = signal(false);

  companyForm = new FormGroup({
    id: new FormControl<number | null>(null),
    name: new FormControl<string>('', { nonNullable: true, validators: [Validators.required] }),
    taxId: new FormControl<string>(''),
    taxNumber: new FormControl<string>(''),
    street: new FormControl<string>(''),
    zipCode: new FormControl<string>(''),
    city: new FormControl<string>(''),
    country: new FormControl<string>(''),
    email: new FormControl<string>(''),
    phone: new FormControl<string>(''),
    commercialRegister: new FormControl<string>(''),
    registerCourt: new FormControl<string>(''),
    vatExemption: new FormControl<boolean>(false),
    bankName: new FormControl<string>(''),
    iban: new FormControl<string>(''),
    bic: new FormControl<string>('')
  });

  loadAll() {
    this.isLoading.set(true);
    this.companyService.getCompanies().subscribe({
      next: (list) => {
        this.companies.set(list);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  // Lade Liste neu und wähle eine bestimmte Firma wieder aus
  private loadAllAndSelect(companyId: number) {
    this.isLoading.set(true);
    this.companyService.getCompanies().subscribe({
      next: (list) => {
        this.companies.set(list);
        this.isLoading.set(false);

        // Finde und wähle die Firma wieder aus
        const company = list.find(c => c.id === companyId);
        if (company) {
          this.select(company);
        }
      },
      error: () => this.isLoading.set(false)
    });
  }

  select(company: Company) {
    this.selected.set(company);
    this.companyForm.patchValue(company as any);
    const fullUrl = this.companyService.getFullLogoUrl(company.logoUrl);
    this.previewUrl.set(fullUrl);
    this.selectedFile.set(null);
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) {
      this.selectedFile.set(null);
      this.previewUrl.set(null);
      return;
    }

    const file = input.files[0];
    // client-side validation
    if (!['image/png', 'image/jpeg', 'image/jpg', 'image/svg+xml'].includes(file.type)) {
      this.uploadError.set('Invalid file type');
      return;
    }
    if (file.size > 2 * 1024 * 1024) {
      this.uploadError.set('File too large (max 2MB)');
      return;
    }

    this.selectedFile.set(file);
    this.uploadError.set(null);

    const reader = new FileReader();
    reader.onload = () => this.previewUrl.set(reader.result as string);
    reader.readAsDataURL(file);
  }

  removeSelectedFile() {
    this.selectedFile.set(null);
    this.previewUrl.set(null);
  }

  save() {
    if (this.companyForm.invalid) return;
    const raw = this.companyForm.getRawValue() as Company;
    if (raw.id) {
      this.companyService.updateCompany(raw.id, raw).subscribe({
        next: () => {
          // after update, if a file selected upload it
          if (this.selectedFile()) {
            this.uploadLogoAfterSave(raw.id);
          } else {
            this.loadAll();
          }
        },
        error: (err) => {
          console.error('Fehler beim Aktualisieren der Firma:', err);
          this.uploadError.set(err?.error?.message || 'Fehler beim Speichern');
        }
      });
    } else {
      // Erstelle neues Firma-Objekt nur mit relevanten Feldern (ohne id, createdAt, updatedAt)
      const toCreate: CreateCompanyRequest = {
        name: raw.name,
        taxId: raw.taxId || undefined,
        taxNumber: raw.taxNumber || undefined,
        street: raw.street || undefined,
        zipCode: raw.zipCode || undefined,
        city: raw.city || undefined,
        country: raw.country || undefined,
        email: raw.email || undefined,
        phone: raw.phone || undefined,
        commercialRegister: raw.commercialRegister || undefined,
        registerCourt: raw.registerCourt || undefined,
        vatExemption: raw.vatExemption || undefined,
        bankName: raw.bankName || undefined,
        iban: raw.iban || undefined,
        bic: raw.bic || undefined
      };
      this.companyService.createCompany(toCreate).subscribe({
        next: (created) => {
          // Setze die neu erstellte Firma als selected
          this.selected.set(created);
          this.companyForm.patchValue(created as any);

          if (this.selectedFile()) {
            this.uploadLogoAfterSave(created.id);
          } else {
            this.loadAll();
          }
        },
        error: (err) => {
          console.error('Fehler beim Erstellen der Firma:', err);
          this.uploadError.set(err?.error?.message || 'Fehler beim Erstellen der Firma');
        }
      });
    }
  }

  private uploadLogoAfterSave(companyId: number) {
    const file = this.selectedFile();
    if (!file) return;
    this.isUploading.set(true);
    this.companyService.uploadLogo(companyId, file).subscribe({
      next: (res) => {
        this.isUploading.set(false);
        this.selectedFile.set(null);

        // Lade Liste neu und wähle Firma wieder aus (mit neuem Logo)
        this.loadAllAndSelect(companyId);
      },
      error: (err) => {
        this.isUploading.set(false);
        this.uploadError.set(err?.error?.message || 'Upload fehlgeschlagen');
      }
    });
  }

  deleteSelected() {
    const sel = this.selected();
    if (!sel) return;
    this.companyService.deleteCompany(sel.id).subscribe(() => this.loadAll());
  }

  // Create/reset the form for new company
  newCompany() {
    this.selected.set(null);
    this.companyForm.reset({ id: null, name: '' });
    this.previewUrl.set(null);
    this.selectedFile.set(null);
    this.uploadError.set(null);
  }

  // Generic helper to set form control values from template input events (keine komplexen Ausdrücke in Template)
  setControlValue(controlName: string, event: Event) {
    const v = (event.target as HTMLInputElement).value;
    this.companyForm.get(controlName)?.setValue(v);
  }

  constructor() {
    this.loadAll();
  }
}
