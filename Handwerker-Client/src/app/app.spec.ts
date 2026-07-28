import { TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { provideRouter } from '@angular/router';
import { App } from './app';
import { ThemeService, AuthService, I18nService, SettingsService, InstallationService } from './core/services';
import { of } from 'rxjs';

describe('App', () => {
  let component: App;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        App,
        provideRouter([]),
        {
          provide: ThemeService,
          useValue: { currentTheme: signal('light'), availableThemes: [], setTheme: () => {} }
        },
        {
          provide: AuthService,
          useValue: { isLoggedIn: signal(false) }
        },
        {
          provide: I18nService,
          useValue: { currentLanguage: signal({ code: 'de', locale: 'de-DE' }), setLanguage: () => {} }
        },
        {
          provide: SettingsService,
          useValue: { getSettings: () => of(null) }
        },
        {
          provide: InstallationService,
          useValue: { navigateBasedOnStatus: () => of({ isCompleted: true }) }
        }
      ]
    });

    component = TestBed.inject(App);
  });

  it('should create the app', () => {
    expect(component).toBeTruthy();
  });

  it('should have title property', () => {
    expect(component.title).toBeDefined();
  });
});
