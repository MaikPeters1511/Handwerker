import {FormControl} from '@angular/forms';

export interface ProviderFormModel {
  id: FormControl<number>;
  name: FormControl<string>;
  company: FormControl<string>;
  street: FormControl<string>;
  zipCode: FormControl<string>;
  city: FormControl<string>;
  email: FormControl<string>;
  phone: FormControl<string>;
  taxId: FormControl<string>;
  taxNumber: FormControl<string>;
  commercialRegister: FormControl<string>;
  registerCourt: FormControl<string>;
  bankId: FormControl<number | null>;
  bankName: FormControl<string>;
  bankIban: FormControl<string>;
  bankBic: FormControl<string>;
  bankPlz: FormControl<string>;
  bankOrt: FormControl<string>;
}
