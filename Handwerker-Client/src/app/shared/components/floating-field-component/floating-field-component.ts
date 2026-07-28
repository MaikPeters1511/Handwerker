import {Component, Input, ChangeDetectionStrategy} from '@angular/core';
import {FormControl, ReactiveFormsModule} from '@angular/forms';
import {TranslatePipe} from '../../pipes/translate.pipe';

@Component({
  selector: 'app-floating-field-component',
  imports: [
    TranslatePipe,
    ReactiveFormsModule
  ],
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './floating-field-component.html'
})
export class FloatingFieldComponent {
  @Input({ required: true }) control!: FormControl<any>;
  @Input({ required: true }) label!: string;
  @Input() type: string = 'text';
  @Input() readonly: boolean = false;
  @Input() disabled: boolean = false;

  get showError(): boolean {
    return this.control.invalid && (this.control.dirty || this.control.touched);
  }
  get error(): string | null {
    if (!this.showError || !this.control.errors) return null;

    if (this.control.errors['required']) return 'validation.required';
    if (this.control.errors['minlength']) return 'validation.minlength';
    if (this.control.errors['email']) return 'validation.email';

    return 'validation.invalid';
  }
  get describedBy(): string | null {
    return this.error ? `${this.label}-error` : null;
  }
}
