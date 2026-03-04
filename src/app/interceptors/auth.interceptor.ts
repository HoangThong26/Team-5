import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';

// Không intercept các auth endpoint để tránh vòng lặp vô hạn
const AUTH_URLS = ['/api/auth/login', '/api/auth/register', '/api/auth/refresh-token', '/api/auth/logout', '/api/auth/forgot-password', '/api/auth/reset-password', '/api/auth/verify'];

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  // Bỏ qua interceptor cho các auth endpoints
  const isAuthUrl = AUTH_URLS.some(url => req.url.includes(url));
  if (isAuthUrl) {
    return next(req);
  }

  const authService = inject(AuthService);
  const router = inject(Router);

  const token = authService.getAccessToken();

  let authReq = req;
  if (token) {
    authReq = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && authService.getRefreshToken()) {
        return authService.refreshToken().pipe(
          switchMap((res) => {
            const retryReq = req.clone({
              setHeaders: {
                Authorization: `Bearer ${res.accessToken}`
              }
            });
            return next(retryReq);
          }),
          catchError((refreshError) => {
            authService.clearToken();
            router.navigate(['/login']);
            return throwError(() => refreshError);
          })
        );
      }
      return throwError(() => error);
    })
  );
};
