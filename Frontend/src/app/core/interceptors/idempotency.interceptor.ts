import { HttpInterceptorFn } from '@angular/common/http';

export const idempotencyInterceptor: HttpInterceptorFn = (req, next) => {
  if (['POST', 'PUT', 'PATCH'].includes(req.method)) {
    return next(req.clone({
      headers: req.headers.set('X-Idempotency-Key', crypto.randomUUID()),
    }));
  }
  return next(req);
};
