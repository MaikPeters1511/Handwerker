import {FormControl} from '@angular/forms';

export interface UpdateUserFormModel {
  id : FormControl<string>;
  username : FormControl<string>;
  firstName : FormControl<string>;
  lastName :FormControl<string>;
  email : FormControl<string>;
  // emailVerified : FormControl<boolean>;
  enabled : FormControl<boolean>;
}
