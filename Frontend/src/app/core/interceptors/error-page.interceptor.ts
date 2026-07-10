import {
  HttpErrorResponse,
  HttpEvent,
  HttpHandlerFn,
  HttpInterceptorFn,
  HttpRequest,
} from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { EMPTY, Observable, catchError, throwError } from 'rxjs';

const SKIP_ERROR_PAGE_ENDPOINTS = ['/auth/login', '/auth/register'];

function shouldRedirect(req: HttpRequest<unknown>, err: HttpErrorResponse): boolean {
  if (req.method !== 'GET' && req.method !== 'HEAD') {
    return false;
  }

  if (SKIP_ERROR_PAGE_ENDPOINTS.some((endpoint) => req.url.includes(endpoint))) {
    return false;
  }

  return err.status === 403 || err.status === 404 || err.status >= 500;
}

export const errorPageInterceptor: HttpInterceptorFn = (
  req,
  next,
): Observable<HttpEvent<unknown>> => {
  const router = inject(Router);

  return next(req).pipe(
    catchError((err: unknown) => {
      if (err instanceof HttpErrorResponse && shouldRedirect(req, err)) {
        const status = err.status >= 500 ? 500 : err.status;
        queueMicrotask(() => {
          router.navigate(['/error', status], { replaceUrl: true });
        });
        return EMPTY;
      }

      return throwError(() => err);
    }),
  );
};
