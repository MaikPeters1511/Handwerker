import { FormControl } from '@angular/forms';

export interface ServiceItemFormModel {
  id: FormControl<number>;
  serviceNumber: FormControl<string>;
  name: FormControl<string>;
  description: FormControl<string>;
  unit: FormControl<string>;
  unitPrice: FormControl<number>;
  taxRate: FormControl<number>;
  isActive: FormControl<boolean>;
}
