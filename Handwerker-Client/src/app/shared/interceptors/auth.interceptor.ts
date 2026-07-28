import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { from, switchMap } from 'rxjs';
import {AuthService} from '../../core/services';


export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);

  // Nur Token anhängen, wenn User authentifiziert ist
  if (!authService.isLoggedIn()) {
    return next(req);
  }

  return from(authService.getToken()).pipe(
    switchMap(token => {
      if (token) {
        const cloned = req.clone({
          setHeaders: {
            Authorization: `Bearer ${token}`
          }
        });
        return next(cloned);
      }
      return next(req);
    })
  );
};



