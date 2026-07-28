import {Component, inject, signal, effect, ChangeDetectionStrategy} from '@angular/core';
import {RouterModule} from '@angular/router';
import { TranslatePipe } from '../../pipes/translate.pipe';
import {AuthService, RoleDashboardService} from '../../../core/services';

@Component({
  selector: 'app-sidenav',
  imports: [RouterModule, TranslatePipe],
  templateUrl: './sidenav.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './sidenav.css',
})
export class Sidenav {
  sidebarOpen = signal<boolean>(true);
  protected openSubmenu = signal<string | null>(null);
  authService = inject(AuthService);
  protected roleDashboard = inject(RoleDashboardService);

  constructor() {
    // Close all submenus when sidebar is collapsed
    effect(() => {
      if (!this.sidebarOpen()) {
        this.openSubmenu.set(null);
      }
    });
  }

  toggleSubmenu(menuId: string) {
    if (this.openSubmenu() === menuId) {
      this.openSubmenu.set(null);
    } else {
      this.openSubmenu.set(menuId);
    }
  }

  isSubmenuOpen(menuId: string): boolean {
    return this.openSubmenu() === menuId;
  }
}
