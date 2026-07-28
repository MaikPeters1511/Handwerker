import {Component, inject, OnInit, signal, ViewChild, ChangeDetectionStrategy} from '@angular/core';
import {NgClass} from '@angular/common';
import {User} from '../../core/interfaces/user/IUser';
import {TranslatePipe} from '../../shared';
import {UserService} from '../../core/services';
import {CreateoreditUser} from './createoredit-user/createoredit-user';
import {DeleteComponent} from '../../shared/components/delete-component/delete-component';
import {ToastService} from '../../shared/services/toast.service';

@Component({
  selector: 'app-users',
  imports: [
    TranslatePipe,
    NgClass,
    CreateoreditUser,
    DeleteComponent
  ],
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './users.html'
})

export class Users implements OnInit {
  @ViewChild('modal') modal!: CreateoreditUser;
  @ViewChild('deleteModal') deleteModal! : DeleteComponent;
  userService = inject(UserService);
  users = signal<User[]>([]);
  loading = signal(true);
  toast = inject(ToastService);

  ngOnInit(): void {
      this.loadUsers();
  }
  loadUsers() {
    this.loading.set(true);
    this.userService.getUsers().subscribe({
      next: (users) => {
        this.users.set(users);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
      }
    });
  }

  createUser() {
    this.modal.open();
  }

  editUser(user: User) {
    this.modal.open(user);
  }
  onModalUpdated() {
    this.loadUsers();
  }

  deleteUser(user: User) {
    this.deleteModal.open(user.id);
  }
  onDeleteConfirmed(id: string | null) {
    if (!id) return;

    this.userService.deleteUser(id).subscribe({
      next: () => {
        this.loading.set(false);
        this.loadUsers();
        this.toast.success('User erfolgreich gelöscht');
      },
      error: (err) => {
        this.loading.set(false);
        this.toast.error('User konnte nicht gelöscht werden');
      }
    });
  }
}
