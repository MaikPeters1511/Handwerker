import { Component, ChangeDetectionStrategy, effect, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl, FormGroup, Validators } from '@angular/forms';
import { toSignal } from '@angular/core/rxjs-interop';
import {TranslatePipe} from '../../shared';
import {ProductService} from './services/product.service';
import {TranslationService} from '../../core/services';
import {Product} from '../../core/entities';
import {ProductFormModel} from '../../core/interfaces/form/IProductFormModel';


@Component({
  selector: 'app-products',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslatePipe],
  templateUrl: './products.html',
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
export class Products {
  private productService = inject(ProductService);
  private translationService = inject(TranslationService);

  // Search State
  // Search State
  searchTerm = signal<string>('');
  searchResults = signal<Product[]>([]);
  showDropdown = signal<boolean>(false);

  // Selected Product State
  selectedProduct = signal<Product | null>(null);
  isEditing = signal<boolean>(false);
  isSaving = signal<boolean>(false);
  lastSaved = signal<Date | null>(null);

  // Toast State
  readonly saveSuccess = signal(false);
  readonly saveError = signal<string | null>(null);
  readonly successMessage = signal<string>('');

  // Signal-based Form
  productForm = new FormGroup<ProductFormModel>({
    id: new FormControl<number>(0, { nonNullable: true }),
    articleNumber: new FormControl<string>('', { nonNullable: true, validators: [Validators.maxLength(100)] }),
    name: new FormControl<string>('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(255)] }),
    position: new FormControl<number>(1, { nonNullable: true, validators: [Validators.required, Validators.min(1)] }),
    quantity: new FormControl<number>(1, { nonNullable: true, validators: [Validators.required, Validators.min(0.01)] }),
    unit: new FormControl<string>('', { nonNullable: true, validators: [Validators.maxLength(50)] }),
    description: new FormControl<string>('', { nonNullable: true, validators: [Validators.maxLength(2000)] }),
    taxRate: new FormControl<number>(19, { nonNullable: true, validators: [Validators.required, Validators.min(0), Validators.max(100)] }),
    taxAmount: new FormControl<number>(0, { nonNullable: true }),
    unitPrice: new FormControl<number>(0.00, { nonNullable: true, validators: [Validators.required, Validators.min(0.00)] }),
    discountPercent: new FormControl<number>(0, { nonNullable: true, validators: [Validators.min(0), Validators.max(100)] }),
    discountAmount: new FormControl<number>(0, { nonNullable: true }),
    totalNet: new FormControl<number>(0, { nonNullable: true }),
    totalGross: new FormControl<number>(0, { nonNullable: true })
  });

  private formValue = toSignal(this.productForm.valueChanges, {
    initialValue: this.productForm.getRawValue()
  });

  constructor() {

    effect(() => {
      const value = this.formValue();
      const quantity = value.quantity ?? 0;
      const unitPrice = value.unitPrice ?? 0;
      const taxRate = value.taxRate ?? 0;
      const discountPercent = value.discountPercent ?? 0;

      const grossPriceInitial = quantity * unitPrice;
      const discountVal = (grossPriceInitial * discountPercent) / 100;
      const totalNet = grossPriceInitial - discountVal;
      const taxVal = (totalNet * taxRate) / 100;
      const totalGross = totalNet + taxVal;

      this.productForm.patchValue(
        {
          taxAmount: parseFloat(taxVal.toFixed(2)),
          discountAmount: parseFloat(discountVal.toFixed(2)),
          totalNet: parseFloat(totalNet.toFixed(2)),
          totalGross: parseFloat(totalGross.toFixed(2))
        },
        { emitEvent: false }
      );
    });

  }

  onSearchInput(event: Event) {
    const input = event.target as HTMLInputElement;
    const term = input.value;
    this.searchTerm.set(term);

    if (term.length < 2) {
      this.searchResults.set([]);
      this.showDropdown.set(false);
      return;
    }

    // Debounce für bessere Performance (300ms)
    setTimeout(() => {
      if (this.searchTerm() !== term) return; // Nur suchen, wenn Wert noch aktuell ist

      this.productService.searchProducts(term).subscribe({
        next: (products) => {
          this.searchResults.set(products);
          this.showDropdown.set(products.length > 0);
        },
        error: () => {
          this.searchResults.set([]);
        }
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

    this.productService.searchProducts(term).subscribe({
      next: (products) => {
        this.searchResults.set(products);
        this.showDropdown.set(products.length > 0);
      },
      error: () => {
        this.searchResults.set([]);
      }
    });
  }

  selectProduct(product: Product) {
    this.selectedProduct.set(product);
    this.isEditing.set(true);
    this.lastSaved.set(null);
    this.productForm.patchValue(product, { emitEvent: false });
    this.showDropdown.set(false);
    this.searchTerm.set('');
  }

  addNewProduct() {
     this.selectedProduct.set(null);
     this.isEditing.set(false);
     this.lastSaved.set(null);
     this.productForm.reset({
        id: 0,
        articleNumber: '',
        name: '',
        position: 1,
        quantity: 1,
        unit: '',
        description: '',
        taxRate: 19,
        taxAmount: 0,
        unitPrice: 0.00,
        discountPercent: 0,
        discountAmount: 0,
        totalNet: 0,
        totalGross: 0
      }, { emitEvent: false });
  }

  saveProduct() {
      if (this.productForm.invalid) {
          this.productForm.markAllAsTouched();

          // Finde heraus, welche Felder ungültig sind
          const invalidFields: string[] = [];
          Object.keys(this.productForm.controls).forEach(key => {
            const control = this.productForm.get(key);
            if (control && control.invalid) {
              invalidFields.push(`${key}: ${JSON.stringify(control.errors)}`);
            }
          });

          console.error('Form is invalid. Invalid fields:', invalidFields);
          console.error('Form value:', this.productForm.getRawValue());

          // Zeige Validierungsfehler im Toast
          const validationError = this.translationService.translate('products.toast.validationError');
          this.saveError.set(`${validationError}: ${invalidFields.map(f => f.split(':')[0]).join(', ')}`);
          setTimeout(() => {
            this.saveError.set(null);
          }, 5000);
          return;
      }

      const formValue = this.productForm.getRawValue() as Product;
      console.log('Saving product:', formValue);
      this.isSaving.set(true);

      if (this.isEditing() && formValue.id) {
          console.log('Updating product with ID:', formValue.id);
          this.productService.updateProduct(formValue.id, formValue).subscribe({
              next: () => {
            this.isSaving.set(false);
            this.lastSaved.set(new Date());

                  // Toast anzeigen
                  this.successMessage.set(this.translationService.translate('products.toast.updated'));
                  this.saveSuccess.set(true);
                  this.saveError.set(null);

                  // Toast nach 5 Sekunden automatisch ausblenden
                  setTimeout(() => {
                    this.saveSuccess.set(false);
                  }, 5000);

                  // Trigger search update if search term exists
                  const currentTerm = this.searchTerm();
                  if (currentTerm && currentTerm.length >= 2) {
                    this.onSearchInput({ target: { value: currentTerm } } as any);
                  }
              },
          error: (err) => {
          this.isSaving.set(false);
          console.error('Error updating product:', err);
          const errorMsg = this.translationService.translate('products.toast.errorSave');
          this.saveError.set(`${errorMsg}: ${err?.error?.message || err?.message || 'Unknown error'}`);

          // Fehler-Toast nach 5 Sekunden automatisch ausblenden
          setTimeout(() => {
            this.saveError.set(null);
          }, 5000);
          }
          });
      } else {
          console.log('Creating new product');
          const { id, ...newProduct } = formValue;
          this.productService.createProduct(newProduct).subscribe({
              next: (created) => {
            this.isSaving.set(false);
            this.lastSaved.set(new Date());

                  // Toast anzeigen
                  this.successMessage.set(this.translationService.translate('products.toast.created'));
                  this.saveSuccess.set(true);
                  this.saveError.set(null);

                  // Toast nach 5 Sekunden automatisch ausblenden
                  setTimeout(() => {
                    this.saveSuccess.set(false);
                  }, 5000);

                  this.selectProduct(created);
              },
          error: (err) => {
          this.isSaving.set(false);
          console.error('Error creating product:', err);
          const errorMsg = this.translationService.translate('products.toast.errorCreate');
          this.saveError.set(`${errorMsg}: ${err?.error?.message || err?.message || 'Unknown error'}`);

          // Fehler-Toast nach 5 Sekunden automatisch ausblenden
          setTimeout(() => {
            this.saveError.set(null);
          }, 5000);
          }
          });
      }
  }
}
