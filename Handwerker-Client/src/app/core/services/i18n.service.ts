import { Injectable, signal } from '@angular/core';
import { registerLocaleData } from '@angular/common';
import localeDe from '@angular/common/locales/de';
import localeEn from '@angular/common/locales/en';
import localeFr from '@angular/common/locales/fr';

export interface Language {
  code: string;
  name: string;
  locale: string;
}

@Injectable({
  providedIn: 'root'
})
export class I18nService {
  private readonly STORAGE_KEY = 'app-language';

  readonly availableLanguages: Language[] = [
    { code: 'de', name: 'Deutsch', locale: 'de-DE' },
    { code: 'en', name: 'English', locale: 'en-US' },
    { code: 'fr', name: 'Français', locale: 'fr-FR' }
  ];

  currentLanguage = signal<Language>(this.availableLanguages[0]);

  constructor() {
    // Register all locales
    registerLocaleData(localeDe);
    registerLocaleData(localeEn);
    registerLocaleData(localeFr);

    // Load saved language or set default to German
    this.loadLanguage();
  }

  private loadLanguage(): void {
    const saved = localStorage.getItem(this.STORAGE_KEY);
    if (saved) {
      const lang = this.availableLanguages.find(l => l.code === saved);
      if (lang) {
        this.currentLanguage.set(lang);
      } else {
        // If saved language is not found, default to German
        this.currentLanguage.set(this.availableLanguages[0]);
      }
    } else {
      // No saved language, default to German
      this.currentLanguage.set(this.availableLanguages[0]);
    }
  }

  setLanguage(code: string): void {
    const lang = this.availableLanguages.find(l => l.code === code);
    if (lang) {
      this.currentLanguage.set(lang);
      localStorage.setItem(this.STORAGE_KEY, code);
    }
  }


  getLanguageCode(): string {
    return this.currentLanguage().code;
  }
}

