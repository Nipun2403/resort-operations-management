import { inject } from '@angular/core';
import { CanActivateFn, CanMatchFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const kitchenGuard: CanActivateFn & CanMatchFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  if (auth.isAuthenticated() && auth.role() === 'Kitchen') {
    return true;
  }
  if (auth.isAuthenticated()) {
    return router.createUrlTree(['/error/403']);
  }
  return router.createUrlTree(['/auth']);
};
