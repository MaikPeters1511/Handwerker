import {Component, ElementRef, EventEmitter, inject, Output, ViewChild, signal, computed, ChangeDetectionStrategy} from '@angular/core';
import {User} from '../../../core/interfaces/user/IUser';
import {IKcRole} from '../../../core/interfaces/user/IKcRole';
import {TranslatePipe} from '../../../shared';
import {FormControl, FormGroup, ReactiveFormsModule, Validators} from '@angular/forms';
import {UpdateUserFormModel} from '../../../core/interfaces/form/IEditUserFormModel';
import {UserService} from '../../../core/services';
import {FloatingFieldComponent} from '../../../shared/components/floating-field-component/floating-field-component';
import {ToastService} from '../../../shared/services/toast.service';
import {CommonModule} from '@angular/common';

@Component({
  selector: 'app-createoredit-user',
  imports: [
    TranslatePipe,
    ReactiveFormsModule,
    FloatingFieldComponent,
    CommonModule
  ],
  standalone: true,
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './createoredit-user.html'
})
export class CreateoreditUser {
  @ViewChild('dialog') dialog!: ElementRef<HTMLDialogElement>;
  @Output() updated = new EventEmitter<void>();
  data?: User;
  private userService = inject(UserService);
  toast = inject(ToastService);

  // Role management signals
  availableRoles = signal<IKcRole[]>([]);
  selectedRoles = signal<IKcRole[]>([]);
  loadingRoles = signal(false);

  userForm = new FormGroup<UpdateUserFormModel>({
    id: new FormControl<string>("", { nonNullable: true }),
    username: new FormControl<string>("", { nonNullable: true , validators: [Validators.required, Validators.minLength(3)]}),
    firstName: new FormControl<string>("", { nonNullable: true, validators: [Validators.required]}),
    lastName: new FormControl<string>("", { nonNullable: true, validators: [Validators.required]}),
    email: new FormControl<string>("", { nonNullable: true, validators: [Validators.required, Validators.email]}),
    enabled: new FormControl<boolean>(false, { nonNullable: true})
  });

  private loadAvailableRoles(userId: string) {
    this.loadingRoles.set(true);
    this.userService.getRoles().subscribe({
      next: (roles) => {
        this.availableRoles.set(roles);
        // Lade auch die bereits zugewiesenen Rollen des Users
        this.loadUserRoles(userId);
      },
      error: (err) => {
        console.error('Error loading available roles:', err);
        this.loadingRoles.set(false);
        this.toast.error('Error loading roles');
      }
    });
  }

  private loadUserRoles(userId: string) {
    this.userService.getUserRoles(userId).subscribe({
      next: (userRoles) => {
        // Setze die Rollen des Users als bereits ausgewählt
        this.selectedRoles.set(userRoles);
        this.loadingRoles.set(false);
      },
      error: (err) => {
        console.error('Error loading user roles:', err);
        this.loadingRoles.set(false);
        this.toast.error('Error loading user roles');
      }
    });
  }

  open(user?: User) {
    if (user) {
      this.data = user;
      this.userForm.patchValue({
        id: user.id,
        username: user.username,
        firstName: user.firstName,
        lastName: user.lastName,
        email: user.email,
        enabled: user.enabled
      });
      // Load available roles for edit mode
      this.loadAvailableRoles(user.id);
    }
    else {
      this.data = undefined;
      this.userForm.reset();
      this.availableRoles.set([]);
      this.selectedRoles.set([]);
    }

    this.dialog.nativeElement.showModal();
  }

  close() {
    this.dialog.nativeElement.close();
  }

  toggleRole(role: IKcRole) {
    const current = this.selectedRoles();
    const exists = current.some(r => r.id === role.id);

    if (exists) {
      this.selectedRoles.update(roles => roles.filter(r => r.id !== role.id));
    } else {
      this.selectedRoles.update(roles => [...roles, role]);
    }
  }

  isRoleSelected(role: IKcRole): boolean {
    return this.selectedRoles().some(r => r.id === role.id);
  }

  save() {
    if (this.userForm.invalid) {
      this.userForm.markAllAsTouched();
      return;
    }
    const updateUser = this.userForm.getRawValue();
    const request$ =
      this.isEditing()
      ? this.userService.updateUser(this.data?.id ?? "test", updateUser)
      : this.userService.createUser(updateUser);

    request$.subscribe({
      next: () => {
        // Handle role changes if in edit mode
        if (this.isEditing() && this.data?.id) {
          this.handleRoleChanges(this.data.id);
        } else {
          this.finalizeAndClose();
        }
      },
      error: err => {
        this.toast.error(this.isEditing() ? 'Error while saving user' : 'Error while saving user');
      }
    });
  }

  private handleRoleChanges(userId: string) {
    // Get current user roles
    this.userService.getUserRoles(userId).subscribe({
      next: (currentRoles) => {
        const selected = this.selectedRoles();
        const toAdd = selected.filter(sr => !currentRoles.some(cr => cr.id === sr.id));
        const toRemove = currentRoles.filter(cr => !selected.some(sr => sr.id === cr.id));

        let pending = 0;
        if (toAdd.length > 0) pending++;
        if (toRemove.length > 0) pending++;

        if (pending === 0) {
          this.finalizeAndClose();
          return;
        }

        if (toAdd.length > 0) {
          this.userService.createUserRoleMappings(userId, toAdd).subscribe({
            next: () => {
              pending--;
              if (pending === 0) this.finalizeAndClose();
            },
            error: (err) => {
              console.error('Error adding roles:', err);
              pending--;
              if (pending === 0) this.finalizeAndClose();
            }
          });
        }

        if (toRemove.length > 0) {
          this.userService.deleteUserRoleMappings(userId, toRemove).subscribe({
            next: () => {
              pending--;
              if (pending === 0) this.finalizeAndClose();
            },
            error: (err) => {
              console.error('Error removing roles:', err);
              pending--;
              if (pending === 0) this.finalizeAndClose();
            }
          });
        }
      },
      error: (err) => {
        console.error('Error loading current roles:', err);
        this.finalizeAndClose();
      }
    });
  }

  private finalizeAndClose() {
    this.dialog.nativeElement.close();
    this.updated.emit();
    this.toast.success(this.isEditing() ? 'Successfully updated user' : 'Successfully created user');
  }

  isEditing() {
    return this.data?.id !== undefined;
  }
}
