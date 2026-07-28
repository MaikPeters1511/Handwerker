import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { toObservable } from '@angular/core/rxjs-interop';
import { filter, map, take } from 'rxjs';
import { AuthService } from '../services';

/**
 * Schützt Routen, die die Rolle "admin" erfordern.
 * Wartet auf die Keycloak-Initialisierung, prüft Login-Status und Admin-Rolle.
 */
export const adminGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const checkAccess = (): boolean | ReturnType<typeof router.createUrlTree> => {
    if (!authService.isLoggedIn()) {
      void authService.login();
      return false;
    }
    if (!authService.isAdmin()) {
      return router.createUrlTree(['/dashboard']);
    }
    return true;
  };

  // Falls bereits initialisiert, direkt entscheiden
  if (authService.initialized()) {
    return checkAccess();
  }

  // Warten bis Keycloak-Init abgeschlossen ist
  return toObservable(authService.initialized).pipe(
    filter(initialized => initialized),
    take(1),
    map(() => checkAccess())
  );
};

