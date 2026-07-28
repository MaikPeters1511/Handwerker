import { inject } from '@angular/core';
import { CanActivateFn } from '@angular/router';
import { toObservable } from '@angular/core/rxjs-interop';
import { filter, map, take } from 'rxjs';
import { AuthService } from '../services';

/**
 * Wartet auf die Keycloak-Initialisierung und leitet nicht-authentifizierte
 * Benutzer zur Login-Seite weiter.
 */
export const authGuard: CanActivateFn = () => {
  const authService = inject(AuthService);

  // Falls bereits initialisiert, direkt entscheiden
  if (authService.initialized()) {
    if (authService.isLoggedIn()) {
      return true;
    }
    void authService.login();
    return false;
  }

  // Warten bis Keycloak-Init abgeschlossen ist
  return toObservable(authService.initialized).pipe(
    filter(initialized => initialized),
    take(1),
    map(() => {
      if (authService.isLoggedIn()) {
        return true;
      }
      void authService.login();
      return false;
    })
  );
};


