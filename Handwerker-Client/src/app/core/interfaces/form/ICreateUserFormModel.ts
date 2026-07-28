import {FormControl} from '@angular/forms';

export interface CreateUserFormModel {
  username : FormControl<string>;
  firstName : FormControl<string>;
  lastName :FormControl<string>;
  email : FormControl<string>;
  enabled : FormControl<boolean>;
}
