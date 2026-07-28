import {
  ApplicationConfig,
  LOCALE_ID,
  provideBrowserGlobalErrorListeners,
  provideZonelessChangeDetection
} from '@angular/core';
import { provideHttpClient, withInterceptors, withXhr } from '@angular/common/http';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import {I18nService} from './core/services';
import {authInterceptor} from './shared/interceptors/auth.interceptor';


export function localeIdFactory(i18nService: I18nService): string {
  return i18nService.currentLanguage().locale;
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZonelessChangeDetection(),
    provideRouter(routes),
    provideHttpClient(withXhr(), 
      withInterceptors([authInterceptor])
    ),
    {
      provide: LOCALE_ID,
      useFactory: localeIdFactory,
      deps: [I18nService]
    }
  ]
};
