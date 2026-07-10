import { Routes } from '@angular/router';
import { AuthRedirectGuard } from './core/guards/auth-redirect.guard';
import { adminGuard } from './core/guards/admin.guard';
import { customerGuard } from './core/guards/customer.guard';
import { frontDeskGuard } from './core/guards/front-desk.guard';
import { kitchenGuard } from './core/guards/kitchen.guard';
import { housekeepingGuard } from './core/guards/housekeeping.guard';
import { maintenanceGuard } from './core/guards/maintenance.guard';

export const routes: Routes = [
  {
    path: 'error/:status',
    loadComponent: () => import('./features/error-page/error-page.component').then(m => m.ErrorPageComponent),
  },
  {
    path: 'error',
    redirectTo: 'error/500',
    pathMatch: 'full',
  },
  {
    path: 'operations/admin',
    canMatch: [adminGuard],
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
        loadComponent: () => import('./shared/components/profile/profile.component')
          .then(m => m.ProfileComponent),
        data: { title: 'Profile' }
      },
      { path: '**', redirectTo: '/error/404' }
    ]
  },
  {
    path: 'user',
    canMatch: [customerGuard],
    loadComponent: () => import('./features/user/user-shell.component')
      .then(m => m.UserShellComponent),
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        loadComponent: () => import('./features/user/pages/dashboard.component')
          .then(m => m.CustomerDashboardComponent)
      },
      {
        path: 'bookings',
        loadComponent: () => import('./features/user/pages/bookings.component')
          .then(m => m.BookingsComponent)
      },
      {
        path: 'room-service',
        loadComponent: () => import('./features/user/pages/room-service.component')
          .then(m => m.RoomServiceComponent)
      },
      {
        path: 'profile',
        loadComponent: () => import('./shared/components/profile/profile.component')
          .then(m => m.ProfileComponent)
      },
      { path: '**', redirectTo: '/error/404' }
    ]
  },
  {
    path: 'operations/front-desk',
    canMatch: [frontDeskGuard],
    loadComponent: () => import('./features/front-desk/front-desk-shell.component')
      .then(m => m.FrontDeskShellComponent),
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        loadComponent: () => import('./features/front-desk/pages/dashboard.component')
          .then(m => m.DashboardComponent)
      },
      {
        path: 'new-booking',
        loadComponent: () => import('./features/front-desk/pages/new-booking.component')
          .then(m => m.FrontDeskBookingWizardComponent)
      },
      {
        path: 'profile',
        loadComponent: () => import('./shared/components/profile/profile.component')
          .then(m => m.ProfileComponent)
      },
      {
        path: 'guest/:email',
        loadComponent: () => import('./features/front-desk/pages/guest-details.component')
          .then(m => m.GuestDetailsComponent)
      },
      { path: '**', redirectTo: '/error/404' }
    ]
  },
  {
    path: 'operations/kitchen',
    canMatch: [kitchenGuard],
    loadComponent: () => import('./features/kitchen/kitchen-shell.component')
      .then(m => m.KitchenShellComponent),
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        loadComponent: () => import('./features/kitchen/pages/dashboard.component')
          .then(m => m.KitchenDashboardComponent)
      },
      {
        path: 'menu-items',
        loadComponent: () => import('./features/kitchen/pages/menu-items.component')
          .then(m => m.KitchenMenuItemsComponent)
      },
      {
        path: 'profile',
        loadComponent: () => import('./shared/components/profile/profile.component')
          .then(m => m.ProfileComponent)
      },
      { path: '**', redirectTo: '/error/404' }
    ]
  },
  {
    path: 'operations/housekeeping',
    canMatch: [housekeepingGuard],
    loadComponent: () => import('./features/housekeeping/housekeeping-shell.component')
      .then(m => m.HousekeepingShellComponent),
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        loadComponent: () => import('./features/housekeeping/pages/dashboard.component')
          .then(m => m.HousekeepingDashboardComponent)
      },
      {
        path: 'profile',
        loadComponent: () => import('./shared/components/profile/profile.component')
          .then(m => m.ProfileComponent)
      },
      { path: '**', redirectTo: '/error/404' }
    ]
  },
  {
    path: 'operations/maintenance',
    canMatch: [maintenanceGuard],
    loadComponent: () => import('./features/maintenance/maintenance-shell.component')
      .then(m => m.MaintenanceShellComponent),
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        loadComponent: () => import('./features/maintenance/pages/dashboard.component')
          .then(m => m.MaintenanceDashboardComponent)
      },
      {
        path: 'profile',
        loadComponent: () => import('./shared/components/profile/profile.component')
          .then(m => m.ProfileComponent)
      },
      { path: '**', redirectTo: 'dashboard' }
    ]
  },
  {
    path: '',
    loadComponent: () => import('./features/public/public-shell.component')
      .then(m => m.PublicShellComponent),
    children: [
      { path: '', redirectTo: 'home', pathMatch: 'full' },
      {
        path: 'auth',
        loadComponent: () => import('./features/auth/auth-page.component')
          .then(m => m.AuthPageComponent),
        canActivate: [AuthRedirectGuard]
      },
      {
        path: 'home',
        loadComponent: () => import('./features/public/pages/home.component')
          .then(m => m.HomeComponent)
      },
      {
        path: 'rooms',
        loadComponent: () => import('./features/public/pages/room-catalogue.component')
          .then(m => m.RoomCatalogueComponent)
      },
      {
        path: 'rooms/:id',
        loadComponent: () => import('./features/public/pages/room-detail.component')
          .then(m => m.RoomDetailComponent)
      },
      {
        path: 'experiences',
        loadComponent: () => import('./features/public/pages/experiences.component')
          .then(m => m.ExperiencesComponent)
      },
      { path: 'menu', redirectTo: 'experiences', pathMatch: 'full' },
      { path: 'amenities', redirectTo: 'experiences', pathMatch: 'full' },
      {
        path: 'availability',
        loadComponent: () => import('./features/public/pages/availability.component')
          .then(m => m.AvailabilityComponent)
      },
      {
        path: 'privacy',
        loadComponent: () => import('./features/public/pages/privacy.component')
          .then(m => m.PrivacyComponent)
      },
      {
        path: 'terms',
        loadComponent: () => import('./features/public/pages/terms.component')
          .then(m => m.TermsComponent)
      },
      {
        path: 'contact',
        loadComponent: () => import('./features/public/pages/contact.component')
          .then(m => m.ContactComponent)
      },
      { path: '**', redirectTo: '/error/404' }
    ]
  },
];
