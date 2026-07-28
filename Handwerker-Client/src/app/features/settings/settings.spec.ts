import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { signal } from '@angular/core';

import { Settings } from './settings';
import { AuthService } from "../../core/services/auth.service";
import {I18nService, SettingsService, ThemeService, TranslationService} from '../../core/services';


describe('Settings', () => {
  let component: Settings;

  let settingsServiceMock: { getSettings: ReturnType<typeof vi.fn>; saveSettings: ReturnType<typeof vi.fn>; sendTestEmail: ReturnType<typeof vi.fn> };
  let themeServiceMock: { setTheme: ReturnType<typeof vi.fn> };
  let i18nServiceMock: { setLanguage: ReturnType<typeof vi.fn>; currentLanguage: ReturnType<typeof signal>; availableLanguages: Array<{ code: string; name: string }> };
  let authServiceMock: Partial<AuthService>;
  let translationServiceMock: { translate: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    settingsServiceMock = {
      getSettings: vi.fn(),
      saveSettings: vi.fn(),
      sendTestEmail: vi.fn()
    };

    themeServiceMock = {
      setTheme: vi.fn()
    };

    i18nServiceMock = {
      setLanguage: vi.fn(),
      currentLanguage: signal({ code: 'de' }),
      availableLanguages: [
        { code: 'de', name: 'Deutsch' },
        { code: 'en', name: 'English' }
      ]
    };

    authServiceMock = {
      userProfile: signal({ info: { given_name: 'John', family_name: 'Doe', email: 'john.doe@example.com' } }),
      isLoggedIn: signal(true),
      userName: signal('John Doe'),
      roles: signal([])
    } as Partial<AuthService>;

    translationServiceMock = {
      translate: vi.fn((key: string) =>
        key === 'settings.error' ? 'Speichern fehlgeschlagen' : 'Translated Text'
      )
    };

    TestBed.configureTestingModule({
      providers: [
        { provide: SettingsService, useValue: settingsServiceMock },
        { provide: ThemeService, useValue: themeServiceMock },
        { provide: I18nService, useValue: i18nServiceMock },
        { provide: AuthService, useValue: authServiceMock },
        { provide: TranslationService, useValue: translationServiceMock }
      ]
    });

    component = TestBed.runInInjectionContext(() => new Settings());
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load settings and initialize snapshot', () => {
    settingsServiceMock.getSettings.mockReturnValue(
      of({
        id: 1,
        theme: 'dark',
        languageCode: 'en',
        emailNotifications: false,
        pushNotifications: true,
        smsNotifications: false,
        testEmail: null,
        testEmailSubject: null,
        testEmailBody: null,
        invoicePrefix: 'INV-',
        nextInvoiceNumber: 2000,
        taxRate: 21,
        currency: 'EUR'
      })
    );

    component.ngOnInit();

    expect(component.selectedTheme()).toBe('dark');
    expect(component.selectedLanguage()).toBe('en');
    expect(component.emailNotifications()).toBe(false);
    expect(themeServiceMock.setTheme).toHaveBeenCalledWith('dark');
    expect(i18nServiceMock.setLanguage).toHaveBeenCalledWith('en');

    // Nach initialem Laden: nicht dirty
    expect(component.isDirty()).toBe(false);
  });

  it('should become dirty after change', () => {
    settingsServiceMock.getSettings.mockReturnValue(
      of({
        id: 1,
        theme: 'light',
        languageCode: 'de',
        emailNotifications: true,
        pushNotifications: false,
        smsNotifications: false,
        testEmail: null,
        testEmailSubject: null,
        testEmailBody: null,
        invoicePrefix: 'RE-',
        nextInvoiceNumber: 1001,
        taxRate: 19,
        currency: 'EUR'
      })
    );

    component.ngOnInit();
    expect(component.isDirty()).toBe(false);

    component.changeTheme('dark');
    expect(component.isDirty()).toBe(true);
  });

  it('should save settings and clear dirty state (success)', () => {
    settingsServiceMock.getSettings.mockReturnValue(
      of({
        id: 1,
        theme: 'light',
        languageCode: 'de',
        emailNotifications: true,
        pushNotifications: false,
        smsNotifications: false,
        testEmail: null,
        testEmailSubject: null,
        testEmailBody: null,
        invoicePrefix: 'RE-',
        nextInvoiceNumber: 1001,
        taxRate: 19,
        currency: 'EUR'
      })
    );

    settingsServiceMock.saveSettings.mockReturnValue(
      of({
        id: 1,
        theme: 'dark',
        languageCode: 'de',
        emailNotifications: true,
        pushNotifications: false,
        smsNotifications: false,
        testEmail: null,
        testEmailSubject: null,
        testEmailBody: null,
        invoicePrefix: 'RE-',
        nextInvoiceNumber: 1001,
        taxRate: 19,
        currency: 'EUR'
      })
    );

    component.ngOnInit();

    component.changeTheme('dark');
    expect(component.isDirty()).toBe(true);

    component.saveSettings();

    expect(settingsServiceMock.saveSettings).toHaveBeenCalledWith({
      theme: 'dark',
      languageCode: 'de',
      emailNotifications: true,
      pushNotifications: false,
      smsNotifications: false,
      testEmail: null,
      testEmailSubject: null,
      testEmailBody: null,
      invoicePrefix: 'RE-',
      nextInvoiceNumber: 1001,
      taxRate: 19,
      currency: 'EUR'
    });

    expect(component.isDirty()).toBe(false);
    expect(component.saveSuccess()).toBe(true);
  });

  it('should expose an error message on save failure', () => {
    settingsServiceMock.getSettings.mockReturnValue(
      of({
        id: 1,
        theme: 'light',
        languageCode: 'de',
        emailNotifications: true,
        pushNotifications: false,
        smsNotifications: false,
        testEmail: null,
        testEmailSubject: null,
        testEmailBody: null,
        invoicePrefix: 'RE-',
        nextInvoiceNumber: 1001,
        taxRate: 19,
        currency: 'EUR'
      })
    );

    settingsServiceMock.saveSettings.mockReturnValue(throwError(() => new Error('boom')));

    component.ngOnInit();
    component.changeTheme('dark');

    component.saveSettings();

    expect(component.saveError()).toContain('Speichern fehlgeschlagen');
    expect(component.saveSuccess()).toBe(false);
  });

  it('should reset settings to defaults', () => {
    component.selectedTheme.set('dark');
    component.selectedLanguage.set('en');
    component.emailNotifications.set(false);
    component.pushNotifications.set(true);
    component.smsNotifications.set(true);
    component.invoicePrefix.set('INV-');
    component.nextInvoiceNumber.set(2000);
    component.taxRate.set(25);
    component.currency.set('USD');

    component.resetSettings();

    expect(component.selectedTheme()).toBe('light');
    expect(component.selectedLanguage()).toBe('de');
    expect(component.emailNotifications()).toBe(true);
    expect(component.pushNotifications()).toBe(false);
    expect(component.smsNotifications()).toBe(false);
    expect(component.invoicePrefix()).toBe('RE-');
    expect(component.nextInvoiceNumber()).toBe(1001);
    expect(component.taxRate()).toBe(19);
    expect(component.currency()).toBe('EUR');
  });
});
