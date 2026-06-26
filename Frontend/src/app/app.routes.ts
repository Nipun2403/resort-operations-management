import { inject } from '@angular/core';
import { Routes } from '@angular/router';
import { AuthRedirectGuard } from './core/guards/auth-redirect.guard';

export const routes: Routes = [
  {
    path: 'auth',
    loadComponent: () => import('./features/auth/auth-page.component')
      .then(m => m.AuthPageComponent),
    canActivate: [() => inject(AuthRedirectGuard).canActivate()]
  }
];

