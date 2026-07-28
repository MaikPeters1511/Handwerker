import {FormControl} from '@angular/forms';

export interface RecipientFormModel {
  id: FormControl<number>;
  customerNumber: FormControl<string>;
  salutation: FormControl<string>;
  name: FormControl<string>;
  contactPerson: FormControl<string>;
  street: FormControl<string>;
  addressLine2: FormControl<string>;
  zipCode: FormControl<string>;
  city: FormControl<string>;
  country: FormControl<string>;
  email: FormControl<string>;
  phone: FormControl<string>;
}
