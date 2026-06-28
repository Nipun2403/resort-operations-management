import { inject } from '@angular/core';
import { Routes } from '@angular/router';
import { AuthRedirectGuard } from './core/guards/auth-redirect.guard';
import { adminGuard } from './core/guards/admin.guard';
import { customerGuard } from './core/guards/customer.guard';

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
          .then(m => m.DashboardComponent),
        data: { title: 'Dashboard' }
      },
      {
        path: 'management',
        children: [
          { path: 'room', loadComponent: () => import('./features/admin/pages/management/room-management.component').then(m => m.RoomManagementComponent), data: { title: 'Rooms' } },
          { path: 'room-type', loadComponent: () => import('./features/admin/pages/management/room-type-management.component').then(m => m.RoomTypeManagementComponent), data: { title: 'Room Types' } },
          { path: 'staff', loadComponent: () => import('./features/admin/pages/management/staff-management.component').then(m => m.StaffManagementComponent), data: { title: 'Staff' } },
          { path: 'amenities', loadComponent: () => import('./features/admin/pages/management/amenities-management.component').then(m => m.AmenitiesManagementComponent), data: { title: 'Amenities' } },
          { path: 'menu', loadComponent: () => import('./features/admin/pages/management/menu-management.component').then(m => m.MenuManagementComponent), data: { title: 'Menu Items' } },
        ]
      },
      {
        path: 'oversight',
        children: [
          { path: 'analytics', loadComponent: () => import('./features/admin/pages/oversight/analytics.component').then(m => m.AnalyticsComponent), data: { title: 'Analytics' } },
          { path: 'auditlogs', loadComponent: () => import('./features/admin/pages/oversight/audit-logs.component').then(m => m.AuditLogsComponent), data: { title: 'Audit Logs' } },
          { path: 'billings-receipts', loadComponent: () => import('./features/admin/pages/oversight/billing-receipts.component').then(m => m.BillingReceiptsComponent), data: { title: 'Billing & Receipts' } },
          { path: 'feedback', loadComponent: () => import('./features/admin/pages/oversight/feedback.component').then(m => m.FeedbackComponent), data: { title: 'Feedback' } },
        ]
      },
      {
        path: 'profile',
        loadComponent: () => import('./features/admin/pages/profile.component')
          .then(m => m.PlaceholderProfileComponent),
        data: { title: 'Profile' }
      },
      { path: '**', redirectTo: 'dashboard' }
    ]
  },
  {
    path: 'user',
    canMatch: [customerGuard],
    canActivate: [customerGuard],
    loadComponent: () => import('./features/user/user-shell.component')
      .then(m => m.UserShellComponent),
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        loadComponent: () => import('./features/user/pages/dashboard.component')
          .then(m => m.PlaceholderCustomerDashboardComponent)
      },
      {
        path: 'bookings',
        loadComponent: () => import('./features/user/pages/bookings.component')
          .then(m => m.BookingsComponent)
      },
      {
        path: 'room-service',
        loadComponent: () => import('./features/user/pages/room-service.component')
          .then(m => m.PlaceholderCustomerRoomServiceComponent)
      },
      {
        path: 'profile',
        loadComponent: () => import('./features/user/pages/profile.component')
          .then(m => m.PlaceholderCustomerProfileComponent)
      },
      { path: '**', redirectTo: 'dashboard' }
    ]
  }
];


