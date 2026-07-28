import { ChangeDetectionStrategy, Component, computed, inject, OnInit, signal } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { take } from 'rxjs';
import {
  AuthService,
  I18nService,
  SettingsService,
  ThemeService,
  TranslationService,
  UserSettingsSave
} from '../../core/services';
import {TranslatePipe} from '../../shared';

@Component({
  selector: 'app-settings',
  imports: [ReactiveFormsModule, TranslatePipe],
  templateUrl: './settings.html',
  styleUrl: './settings.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class Settings implements OnInit {
  authService = inject(AuthService);
  i18nService = inject(I18nService);
  settingsService = inject(SettingsService);
  themeService = inject(ThemeService);
  translationService = inject(TranslationService);
  userProfile = this.authService.userProfile;

  // Theme settings
  selectedTheme = signal<string>('light');
  themes = ['light', 'dark', 'cupcake', 'bumblebee', 'emerald', 'corporate', 'synthwave', 'retro', 'cyberpunk', 'valentine', 'halloween', 'garden', 'forest', 'aqua', 'lofi', 'pastel', 'fantasy', 'wireframe', 'black', 'luxury', 'dracula'];

  // Language settings (UI state)
  selectedLanguage = signal<string>('de');

  languages = this.i18nService.availableLanguages;

  // Notification settings
  emailNotifications = signal<boolean>(true);
  pushNotifications = signal<boolean>(false);
  smsNotifications = signal<boolean>(false);

  // Email settings
  testEmail = signal<string>('');
  testEmailSubject = signal<string>('');
  testEmailBody = signal<string>('');
  isSendingEmail = signal<boolean>(false);
  emailSendSuccess = signal<boolean>(false);
  emailSendError = signal<string | null>(null);

  // Invoice settings
  invoicePrefix = signal<string>('RE-');
  nextInvoiceNumber = signal<number>(1001);
  taxRate = signal<number>(19);
  currency = signal<string>('EUR');



  // UI state
  readonly isSaving = signal(false);
  readonly saveError = signal<string | null>(null);
  readonly saveSuccess = signal(false);

  private readonly loadedSnapshot = signal<UserSettingsSave | null>(null);
  private hasLoadedInitially = false; // Verhindert mehrfaches Laden

  readonly currentPayload = computed<UserSettingsSave>(() => ({
    theme: this.selectedTheme(),
    languageCode: this.selectedLanguage(),
    emailNotifications: this.emailNotifications(),
    pushNotifications: this.pushNotifications(),
    smsNotifications: this.smsNotifications(),
    testEmail: this.testEmail() || null,
    testEmailSubject: this.testEmailSubject() || null,
    testEmailBody: this.testEmailBody() || null,
    invoicePrefix: this.invoicePrefix(),
    nextInvoiceNumber: this.nextInvoiceNumber(),
    taxRate: this.taxRate(),
    currency: this.currency()
  }));

  readonly isDirty = computed(() => {
    const baseline = this.loadedSnapshot();
    if (!baseline) {
      // Solange noch nicht geladen: Button deaktiviert lassen
      return false;
    }

    const current = this.currentPayload();
    return (
      baseline.theme !== current.theme ||
      baseline.languageCode !== current.languageCode ||
      baseline.emailNotifications !== current.emailNotifications ||
      baseline.pushNotifications !== current.pushNotifications ||
      baseline.smsNotifications !== current.smsNotifications ||
      baseline.testEmail !== current.testEmail ||
      baseline.testEmailSubject !== current.testEmailSubject ||
      baseline.testEmailBody !== current.testEmailBody ||
      baseline.invoicePrefix !== current.invoicePrefix ||
      baseline.nextInvoiceNumber !== current.nextInvoiceNumber ||
      baseline.taxRate !== current.taxRate ||
      baseline.currency !== current.currency
    );
  });

  ngOnInit() {
    // Settings nur beim ersten Besuch laden, nicht bei jeder Navigation
    // Dies verhindert, dass Backend-Settings die User-Änderungen überschreiben
    if (!this.hasLoadedInitially) {
      this.loadSettings();
      this.hasLoadedInitially = true;
    }
  }

  changeTheme(theme: string) {
    this.selectedTheme.set(theme);
    this.themeService.setTheme(theme);
    this.saveSuccess.set(false);
    this.saveError.set(null);
  }

  changeLanguage(code: string) {
    this.selectedLanguage.set(code);
    this.i18nService.setLanguage(code);
    this.saveSuccess.set(false);
    this.saveError.set(null);
  }

  toggleEmailNotifications() {
    this.emailNotifications.update(v => !v);
    this.saveSuccess.set(false);
    this.saveError.set(null);
  }

  togglePushNotifications() {
    this.pushNotifications.update(v => !v);
    this.saveSuccess.set(false);
    this.saveError.set(null);
  }

  toggleSmsNotifications() {
    this.smsNotifications.update(v => !v);
    this.saveSuccess.set(false);
    this.saveError.set(null);
  }

  sendTestEmail() {
    const email = this.testEmail();
    const subject = this.testEmailSubject();
    const body = this.testEmailBody();

    if (!email || !subject || !body) {
      this.emailSendError.set(this.translationService.translate('settings.email.requiredFields'));
      return;
    }

    this.isSendingEmail.set(true);
    this.emailSendSuccess.set(false);
    this.emailSendError.set(null);

    this.settingsService
      .sendTestEmail(email, subject, body)
      .pipe(take(1))
      .subscribe({
        next: () => {
          this.emailSendSuccess.set(true);

          // Toast nach 5 Sekunden ausblenden
          setTimeout(() => {
            this.emailSendSuccess.set(false);
          }, 5000);
        },
        error: (err) => {
          this.emailSendError.set(
            err.error?.message || this.translationService.translate('settings.email.sendError')
          );

          // Fehler nach 5 Sekunden ausblenden
          setTimeout(() => {
            this.emailSendError.set(null);
          }, 5000);
        }
      })
      .add(() => this.isSendingEmail.set(false));
  }

  saveSettings() {
    if (this.isSaving() || !this.isDirty()) {
      return;
    }

    this.isSaving.set(true);
    this.saveError.set(null);
    this.saveSuccess.set(false);

    const payload = this.currentPayload();

    this.settingsService
      .saveSettings(payload)
      .pipe(take(1))
      .subscribe({
        next: saved => {
          this.selectedTheme.set(saved.theme);
          this.selectedLanguage.set(saved.languageCode);
          this.emailNotifications.set(saved.emailNotifications);
          this.pushNotifications.set(saved.pushNotifications);
          this.smsNotifications.set(saved.smsNotifications);
          this.testEmail.set(saved.testEmail || '');
          this.testEmailSubject.set(saved.testEmailSubject || '');
          this.testEmailBody.set(saved.testEmailBody || '');
          this.invoicePrefix.set(saved.invoicePrefix);
          this.nextInvoiceNumber.set(saved.nextInvoiceNumber);
          this.taxRate.set(saved.taxRate);
          this.currency.set(saved.currency);

          this.themeService.setTheme(saved.theme);
          this.i18nService.setLanguage(saved.languageCode);

          this.loadedSnapshot.set({
            theme: saved.theme,
            languageCode: saved.languageCode,
            emailNotifications: saved.emailNotifications,
            pushNotifications: saved.pushNotifications,
            smsNotifications: saved.smsNotifications,
            testEmail: saved.testEmail,
            testEmailSubject: saved.testEmailSubject,
            testEmailBody: saved.testEmailBody,
            invoicePrefix: saved.invoicePrefix,
            nextInvoiceNumber: saved.nextInvoiceNumber,
            taxRate: saved.taxRate,
            currency: saved.currency
          });

          this.saveSuccess.set(true);

          // Toast nach 5 Sekunden automatisch ausblenden
          setTimeout(() => {
            this.saveSuccess.set(false);
          }, 5000);
        },
        error: () => {
          this.saveError.set(this.translationService.translate('settings.error'));

          // Fehler-Toast nach 5 Sekunden automatisch ausblenden
          setTimeout(() => {
            this.saveError.set(null);
          }, 5000);
        }
      })
      .add(() => this.isSaving.set(false));
  }

  resetSettings() {
    this.selectedTheme.set('light');
    this.selectedLanguage.set('de');
    this.emailNotifications.set(true);
    this.pushNotifications.set(false);
    this.smsNotifications.set(false);
    this.testEmail.set('');
    this.testEmailSubject.set('');
    this.testEmailBody.set('');
    this.invoicePrefix.set('RE-');
    this.nextInvoiceNumber.set(1001);
    this.taxRate.set(19);
    this.currency.set('EUR');

    this.themeService.setTheme('light');
    this.i18nService.setLanguage('de');

    this.saveSuccess.set(false);
    this.saveError.set(null);
  }

  private loadSettings() {
    this.settingsService
      .getSettings()
      .pipe(take(1))
      .subscribe(settings => {
        this.selectedTheme.set(settings.theme);
        this.selectedLanguage.set(settings.languageCode);
        this.emailNotifications.set(settings.emailNotifications);
        this.pushNotifications.set(settings.pushNotifications);
        this.smsNotifications.set(settings.smsNotifications);
        this.testEmail.set(settings.testEmail || '');
        this.testEmailSubject.set(settings.testEmailSubject || '');
        this.testEmailBody.set(settings.testEmailBody || '');
        this.invoicePrefix.set(settings.invoicePrefix);
        this.nextInvoiceNumber.set(settings.nextInvoiceNumber);
        this.taxRate.set(settings.taxRate);
        this.currency.set(settings.currency);

        this.themeService.setTheme(settings.theme);
        this.i18nService.setLanguage(settings.languageCode);

        this.loadedSnapshot.set({
          theme: settings.theme,
          languageCode: settings.languageCode,
          emailNotifications: settings.emailNotifications,
          pushNotifications: settings.pushNotifications,
          smsNotifications: settings.smsNotifications,
          testEmail: settings.testEmail,
          testEmailSubject: settings.testEmailSubject,
          testEmailBody: settings.testEmailBody,
          invoicePrefix: settings.invoicePrefix,
          nextInvoiceNumber: settings.nextInvoiceNumber,
          taxRate: settings.taxRate,
          currency: settings.currency
        });
      });
  }
}
