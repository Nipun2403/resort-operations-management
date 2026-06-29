import { Injectable, inject } from '@angular/core';
import { Router, UrlTree } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class AuthRedirectGuard {
  private authService = inject(AuthService);
  private router = inject(Router);

  canActivate(
    route?: import('@angular/router').ActivatedRouteSnapshot,
    state?: import('@angular/router').RouterStateSnapshot
  ): boolean | UrlTree {
    if (this.authService.isAuthenticated()) {
      const url = state?.url ?? this.router.routerState.snapshot.url;
      const urlTree = this.router.parseUrl(url);
      const returnUrl = urlTree.queryParams['returnUrl'];
      if (returnUrl && typeof returnUrl === 'string' && returnUrl.startsWith('/')) {
        return this.router.parseUrl(returnUrl);
      }

      const role = this.authService.role();
      let targetRoute = '/user/dashboard';

      switch (role) {
        case 'RegisteredUser':
          targetRoute = '/user/dashboard';
          break;
        case 'Admin':
          targetRoute = '/operations/admin/dashboard';
          break;
        case 'FrontDesk':
          targetRoute = '/operations/front-desk/dashboard';
          break;
        case 'Kitchen':
          targetRoute = '/operations/kitchen/dashboard';
          break;
        case 'Housekeeping':
          targetRoute = '/operations/housekeeping/dashboard';
          break;
        case 'Maintenance':
          targetRoute = '/operations/maintenance/dashboard';
          break;
        default:
          targetRoute = '/user/dashboard';
          break;
      }

      return this.router.parseUrl(targetRoute);
    }

    return true;
  }
}
