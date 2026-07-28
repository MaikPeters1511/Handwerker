import { Injectable, signal, inject, computed, effect } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { I18nService } from './i18n.service';

@Injectable({
  providedIn: 'root'
})
export class TranslationService {
  private http = inject(HttpClient);
  private i18nService = inject(I18nService);

  private translations = signal<Record<string, any>>({});

  // Computed signal that changes when translations change
  private translationsVersion = computed(() => {
    const lang = this.i18nService.currentLanguage().code;
    const trans = this.translations();
    return { lang, trans };
  });

  constructor() {
    // React to language changes
    effect(() => {
      const lang = this.i18nService.currentLanguage().code;
      this.loadTranslations(lang);
    });
  }

  private async loadTranslations(lang: string): Promise<void> {
    try {
      const data = await this.http.get<Record<string, any>>(`/assets/i18n/${lang}.json`).toPromise();
      this.translations.set(data || {});
    } catch (error) {
      console.error('Failed to load translations:', error);
      this.translations.set({});
    }
  }

  translate(key: string): string {
    // Access the computed to trigger reactivity
    const { trans } = this.translationsVersion();

    const keys = key.split('.');
    let value: any = trans;

    for (const k of keys) {
      value = value?.[k];
      if (value === undefined) {
        return key;
      }
    }

    return value as string;
  }

  t(key: string): string {
    return this.translate(key);
  }
}

