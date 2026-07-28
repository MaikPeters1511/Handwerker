import { Component, effect, inject, signal, viewChild, ChangeDetectionStrategy } from '@angular/core';
import { RouterOutlet, RouterModule, Router } from '@angular/router';
import { catchError, of, take } from 'rxjs';
import { ThemeService } from './core/services';
import { AuthService } from './core/services';
import { I18nService } from './core/services';
import { SettingsService } from './core/services';
import { InstallationService } from './core/services';
import {Welcom} from './features/welcom/welcom';
import {Sidenav} from './shared/components/sidenav/sidenav';
import { NotificationDropdown } from './shared/components/notification-dropdown/notification-dropdown';
import { RealtimeNotifications } from './shared/components/realtime-notifications/realtime-notifications';
import { DOCUMENT } from '@angular/common';
import { InstallationWizard } from './features/installation-wizard/installation-wizard';
import {TranslatePipe} from './shared';
import {ToastComponent} from './shared/components/toast-component/toast-component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterModule, Welcom, Sidenav, NotificationDropdown, RealtimeNotifications, TranslatePipe, InstallationWizard, ToastComponent],
  templateUrl: './app.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './app.scss'
})
export class App {
  readonly title = signal('Handwerker-Client');
  private themeService = inject(ThemeService);
  private i18nService = inject(I18nService);
  private settingsService = inject(SettingsService);
  private installationService = inject(InstallationService);
  private document = inject(DOCUMENT);
  authService = inject(AuthService);
  private router = inject(Router);

  protected currentTheme = this.themeService.currentTheme;
  protected availableThemes = this.themeService.availableThemes;

  protected sidebarOpen = signal(true);
  protected installationModalOpen = signal(false);
  protected isInitializing = signal(false);
  private settingsLoaded = false;

  private sidenavComponent = viewChild(Sidenav);

  constructor() {
    // Set language attribute
    effect(() => {
      const lang = this.i18nService.currentLanguage();
      this.document.documentElement.setAttribute('lang', lang.code);
    });

    // Load settings nur einmal wenn eingeloggt
    effect(() => {
      if (this.authService.isLoggedIn() && !this.settingsLoaded) {
        this.isInitializing.set(true);
        this.settingsLoaded = true;
        this.loadAppSettings();
      }
    });

    // Sync sidebar state with sidenav component
    effect(() => {
      const sidenav = this.sidenavComponent();
      if (sidenav) {
        sidenav.sidebarOpen.set(this.sidebarOpen());
      }
    });
  }

  toggleSidebar() {
    this.sidebarOpen.update(v => !v);
  }

  setTheme(theme: string) {
    this.themeService.setTheme(theme);
  }

  private loadAppSettings() {
    this.settingsService
      .getSettings()
      .pipe(
        take(1),
        catchError(() => of(null))
      )
      .subscribe(settings => {
        if (!settings) {
          return;
        }

        this.themeService.setTheme(settings.theme);
        this.i18nService.setLanguage(settings.languageCode);

        // Nach Laden der Settings, prüfe Installation-Status
        this.installationService.navigateBasedOnStatus().subscribe({
          next: (res) => {
            if (res.isCompleted) {
              // Navigiere zu Dashboard
              this.router.navigate(['/dashboard']);
            } else {
              // Öffne Modal
              this.installationModalOpen.set(true);
            }
            this.isInitializing.set(false);
          },
          error: () => {
            // Bei Fehler, öffne Modal
            this.installationModalOpen.set(true);
            this.isInitializing.set(false);
          }
        });
      });
  }
}
