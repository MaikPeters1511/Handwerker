import { Component, signal, inject, output, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import {TranslatePipe} from '../../shared';


interface Provider {
  id: number;
  name: string;
  company: string;
  street: string;
  zipCode: string;
  city: string;
  email: string;
  phone: string;
  website: string;
  taxId: string;
  taxNumber: string;
  commercialRegister: string;
  registerCourt: string;
  bank: { name: string; iban: string; bic: string };
}

@Component({
  selector: 'app-installation-wizard',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslatePipe],
  templateUrl: './installation-wizard.html',
  styleUrl: './installation-wizard.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class InstallationWizard {
  private fb = inject(FormBuilder);
  private http = inject(HttpClient);
  private router = inject(Router);

  close = output();

  currentStep = signal(0);
  isCompleted = signal(false);

  userForm: FormGroup;
  companyForm: FormGroup;
  finalForm: FormGroup;

  providers = signal<Provider[]>([]);
  selectedProviders = signal<number[]>([]);

  constructor() {
    this.userForm = this.fb.group({
      salutation: ['', Validators.required],
      title: [''],
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      profileImage: [null],
    });

    this.companyForm = this.fb.group({
      name: ['', Validators.required],
      street: ['', Validators.required],
      zipCode: ['', Validators.required],
      city: ['', Validators.required],
      phone: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      commercialRegister: [''],
      registerCourt: [''],
      taxId: [''],
      taxNumber: [''],
      logo: [null],
      vatExemption: [false],
      bankName: [''],
      iban: [''],
      bic: [''],
    });

    this.finalForm = this.fb.group({
      industry: ['', Validators.required],
      referralSource: ['', Validators.required],
      avAgreementAccepted: [false, Validators.requiredTrue],
    });

    this.checkStatus();
  }

  checkStatus() {
    this.http.get<{ isCompleted: boolean }>('/api/installation/status').subscribe({
      next: (res) => {
        if (res.isCompleted) {
          this.router.navigate(['/dashboard']);
        }
      },
    });
  }

  nextStep() {
    if (this.currentStep() < 4) {
      this.currentStep.update(s => s + 1);
      if (this.currentStep() === 3) {
        this.loadProviders();
      }
    }
  }

  prevStep() {
    if (this.currentStep() > 0) {
      this.currentStep.update(s => s - 1);
    }
  }

  setStep(step: number) {
    if (step >= 0 && step <= 4) {
      this.currentStep.set(step);
      if (step === 3 && this.providers().length === 0) {
        this.loadProviders();
      }
    }
  }

  loadProviders() {
    this.http.get<Provider[]>('/api/installation/suppliers').subscribe({
      next: (providers) => this.providers.set(providers),
    });
  }

  toggleProvider(id: number) {
    this.selectedProviders.update(selected => {
      if (selected.includes(id)) {
        return selected.filter(s => s !== id);
      } else {
        return [...selected, id];
      }
    });
  }

  onFileChange(event: Event, formControlName: string, form: FormGroup) {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (file) {
      form.patchValue({ [formControlName]: file });
    }
  }

  submitUserData() {
    this.saveUserDataInternal();
    this.nextStep();
  }

  saveUserData() {
    this.saveUserDataInternal();
  }

  private saveUserDataInternal() {
    const formData = new FormData();
    Object.keys(this.userForm.value).forEach(key => {
      if (key === 'profileImage' && this.userForm.value[key]) {
        formData.append(key, this.userForm.value[key]);
      } else {
        formData.append(key, this.userForm.value[key]);
      }
    });

    this.http.post('/api/installation/user-data', formData).subscribe();
  }

  submitCompanyData() {
    this.saveCompanyDataInternal();
    this.nextStep();
  }

  saveCompanyData() {
    this.saveCompanyDataInternal();
  }

  private saveCompanyDataInternal() {
    const formData = new FormData();
    Object.keys(this.companyForm.value).forEach(key => {
      if ((key === 'logo' || key === 'profileImage') && this.companyForm.value[key]) {
        formData.append(key, this.companyForm.value[key]);
      } else {
        formData.append(key, this.companyForm.value[key]);
      }
    });

    this.http.post('/api/installation/company-data', formData).subscribe();
  }

  submitSuppliers() {
    this.saveSuppliersInternal();
    this.nextStep();
  }

  saveSuppliers() {
    this.saveSuppliersInternal();
  }

  private saveSuppliersInternal() {
    this.http.post('/api/installation/suppliers', { selectedSupplierIds: this.selectedProviders() }).subscribe();
  }

  submitFinal() {
    this.saveFinalInternal();
    this.isCompleted.set(true);
    this.close.emit();
  }

  saveFinal() {
    this.saveFinalInternal();
  }

  private saveFinalInternal() {
    this.http.post('/api/installation/final', this.finalForm.value).subscribe({
      next: () => {
        this.isCompleted.set(true);
        this.close.emit();
      },
    });
  }
}
