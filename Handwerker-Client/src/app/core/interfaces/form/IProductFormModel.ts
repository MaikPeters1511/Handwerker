import { FormControl } from "@angular/forms";

export interface ProductFormModel {
  id: FormControl<number>;
  articleNumber: FormControl<string>;
  name: FormControl<string>;
  position: FormControl<number>;
  quantity: FormControl<number>;
  unit: FormControl<string>;
  description: FormControl<string>;
  taxRate: FormControl<number>;
  taxAmount: FormControl<number>;
  unitPrice: FormControl<number>;
  discountPercent: FormControl<number>;
  discountAmount: FormControl<number>;
  totalNet: FormControl<number>;
  totalGross: FormControl<number>;
}
