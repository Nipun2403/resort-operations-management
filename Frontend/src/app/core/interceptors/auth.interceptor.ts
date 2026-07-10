import { HttpEvent, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Observable } from 'rxjs';
import { AuthService } from '../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next): Observable<HttpEvent<unknown>> => {
  const authService = inject(AuthService);
  const token = authService.token();

  // Skip token header for auth endpoints (stale tokens cause 401) and Azure blob storage (SAS auth only)
  const isAuthEndpoint = req.url.includes('/auth/login') || req.url.includes('/auth/register');
  const isAzureBlob = req.url.includes('.blob.core.windows.net');

  if (token && !isAuthEndpoint && !isAzureBlob) {
    const authReq = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`,
      },
    });
    return next(authReq);
  }

  return next(req);
};