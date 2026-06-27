import { inject } from '@angular/core';
import { Routes } from '@angular/router';
import { AuthRedirectGuard } from './core/guards/auth-redirect.guard';
import { adminGuard } from './core/guards/admin.guard';

export const routes: Routes = [
  {
    path: 'auth',
    loadComponent: () => import('./features/auth/auth-page.component')
      .then(m => m.AuthPageComponent),
    canActivate: [() => inject(AuthRedirectGuard).canActivate()]
  },
  {
    path: 'operations/admin',
    canMatch: [adminGuard],
    canActivate: [adminGuard],
    loadComponent: () => import('./features/admin/admin-shell.component')
      .then(m => m.AdminShellComponent),
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        loadComponent: () => import('./features/admin/pages/dashboard.component')
          .then(m => m.DashboardComponent)
      },
      {
        path: 'management',
        children: [
          { path: 'room', loadComponent: () => import('./features/admin/pages/management/room-management.component').then(m => m.RoomManagementComponent) },
          { path: 'room-type', loadComponent: () => import('./features/admin/pages/management/room-type-management.component').then(m => m.RoomTypeManagementComponent) },
          { path: 'staff', loadComponent: () => import('./features/admin/pages/management/staff-management.component').then(m => m.StaffManagementComponent) },
          { path: 'amenities', loadComponent: () => import('./features/admin/pages/management/amenities-management.component').then(m => m.AmenitiesManagementComponent) },
          { path: 'menu', loadComponent: () => import('./features/admin/pages/management/menu-management.component').then(m => m.MenuManagementComponent) },
        ]
      },
      {
        path: 'oversight',
        children: [
          { path: 'analytics', loadComponent: () => import('./features/admin/pages/oversight/analytics.component').then(m => m.AnalyticsComponent) },
          { path: 'auditlogs', loadComponent: () => import('./features/admin/pages/oversight/audit-logs.component').then(m => m.AuditLogsComponent) },
          { path: 'billings-receipts', loadComponent: () => import('./features/admin/pages/oversight/billing-receipts.component').then(m => m.BillingReceiptsComponent) },
          { path: 'feedback', loadComponent: () => import('./features/admin/pages/oversight/feedback.component').then(m => m.FeedbackComponent) },
        ]
      },
      {
        path: 'profile',
        loadComponent: () => import('./features/admin/pages/profile.component')
          .then(m => m.PlaceholderProfileComponent)
      },
      { path: '**', redirectTo: 'dashboard' }
    ]
  }
];


