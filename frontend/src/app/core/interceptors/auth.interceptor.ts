import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../features/authentication/services/auth.service';
import { tap } from 'rxjs/operators';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const token = authService.token();

  if (token) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  return next(req).pipe(
    tap({
      error: (error: HttpErrorResponse) => {
        if (error.status === 401) {
          // Token expirado ou inválido — forçar logout
          authService.logout();
          router.navigate(['/login']);
        }
        // 403 é tratado ao nível dos componentes para dar feedback contextual
      }
    })
  );
};
