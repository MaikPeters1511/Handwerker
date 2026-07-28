import { FormControl } from '@angular/forms';

export interface WageTypeFormModel {
  id: FormControl<number>;
  wageNumber: FormControl<string>;
  name: FormControl<string>;
  description: FormControl<string>;
  hourlyRate: FormControl<number>;
  taxRate: FormControl<number>;
  isActive: FormControl<boolean>;
}
