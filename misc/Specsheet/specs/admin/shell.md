# Specsheet: Admin Shell & Route Configuration

### 1. Purpose

- Provide the **layout container** for all `/operations/admin` pages.
- Includes:
  - Persistent sidebar navigation.
  - Top toolbar with user info and logout.
  - `<router-outlet>` for lazy‑loaded child pages.
  - Authentication & role guard (Admin only).
- Ensure that navigating to any `/operations/admin/*` path without Admin role redirects to `/auth`.

### 2. Route & Navigation

**Route config** added to `app.routes.ts` (after existing `auth` route).  
Import adminGuard:

```ts
import { adminGuard } from "./core/guards/admin.guard";
```

Full admin route block:

```ts
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
        .then(m => m.PlaceholderDashboardComponent)
    },
    {
      path: 'management',
      children: [
        { path: 'room', loadComponent: () => import('./features/admin/pages/management/room-management.component').then(m => m.PlaceholderRoomManagementComponent) },
        { path: 'room-type', loadComponent: () => import('./features/admin/pages/management/room-type-management.component').then(m => m.PlaceholderRoomTypeManagementComponent) },
        { path: 'staff', loadComponent: () => import('./features/admin/pages/management/staff-management.component').then(m => m.PlaceholderStaffManagementComponent) },
        { path: 'amenities', loadComponent: () => import('./features/admin/pages/management/amenities-management.component').then(m => m.PlaceholderAmenitiesManagementComponent) },
        { path: 'menu', loadComponent: () => import('./features/admin/pages/management/menu-management.component').then(m => m.PlaceholderMenuManagementComponent) },
      ]
    },
    {
      path: 'oversight',
      children: [
        { path: 'analytics', loadComponent: () => import('./features/admin/pages/oversight/analytics.component').then(m => m.PlaceholderAnalyticsComponent) },
        { path: 'auditlogs', loadComponent: () => import('./features/admin/pages/oversight/audit-logs.component').then(m => m.PlaceholderAuditLogsComponent) },
        { path: 'billings-receipts', loadComponent: () => import('./features/admin/pages/oversight/billing-receipts.component').then(m => m.PlaceholderBillingReceiptsComponent) },
        { path: 'feedback', loadComponent: () => import('./features/admin/pages/oversight/feedback.component').then(m => m.PlaceholderFeedbackComponent) },
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
```

- All leaf pages are **placeholders** for now.
- Sidebar links correspond exactly to these paths.

### 3. Authorization

**adminGuard** (functional guard, file `src/app/core/guards/admin.guard.ts`):

```ts
import { inject } from "@angular/core";
import { CanActivateFn, CanMatchFn, Router } from "@angular/router";
import { AuthService } from "../services/auth.service";

export const adminGuard: CanActivateFn & CanMatchFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  if (auth.isAuthenticated() && auth.role() === "Admin") {
    return true;
  }
  return router.createUrlTree(["/auth"]);
};
```

- Both `canActivate` and `canMatch` use the same function.
- Any unauthorized access returns a redirect to `/auth`.

### 4. Required Supporting Infrastructure (new)

- `adminGuard` (file above).
- `AdminShellComponent` (layout).
- 11 placeholder components (listed in File Structure).
- No modifications to `AuthService` from previous spec.

### 5. Component API (AdminShellComponent)

- **Selector**: `app-admin-shell`
- **Standalone**: `true`
- **Imports**: `CommonModule`, `RouterModule`, Angular Material 18 modules (`MatSidenavModule`, `MatToolbarModule`, `MatListModule`, `MatIconModule`, `MatButtonModule`, `MatMenuModule`, `MatDividerModule`), `LayoutModule` (for `BreakpointObserver`).
- **No inputs/outputs**.

### 6. Template Structure

```html
<mat-sidenav-container>
  <mat-sidenav
    #sidenav
    [mode]="isMobile() ? 'over' : 'side'"
    [opened]="isMobile() ? sidebarOpen() : true"
    aria-label="Main navigation"
  >
    <mat-toolbar color="primary">Admin Panel</mat-toolbar>

    <mat-nav-list>
      <!-- Dashboard -->
      <a
        mat-list-item
        routerLink="/operations/admin/dashboard"
        routerLinkActive="active"
        (click)="onNavClick()"
      >
        <mat-icon matListItemIcon>dashboard</mat-icon>
        <span matListItemTitle>Dashboard</span>
      </a>
      <mat-divider></mat-divider>

      <h3 matSubheader>Management</h3>
      <a
        mat-list-item
        routerLink="/operations/admin/management/room"
        routerLinkActive="active"
        (click)="onNavClick()"
      >
        <mat-icon matListItemIcon>meeting_room</mat-icon>
        <span matListItemTitle>Rooms</span>
      </a>
      <a
        mat-list-item
        routerLink="/operations/admin/management/room-type"
        routerLinkActive="active"
        (click)="onNavClick()"
      >
        <mat-icon matListItemIcon>bed</mat-icon>
        <span matListItemTitle>Room Types</span>
      </a>
      <a
        mat-list-item
        routerLink="/operations/admin/management/staff"
        routerLinkActive="active"
        (click)="onNavClick()"
      >
        <mat-icon matListItemIcon>people</mat-icon>
        <span matListItemTitle>Staff</span>
      </a>
      <a
        mat-list-item
        routerLink="/operations/admin/management/amenities"
        routerLinkActive="active"
        (click)="onNavClick()"
      >
        <mat-icon matListItemIcon>spa</mat-icon>
        <span matListItemTitle>Amenities</span>
      </a>
      <a
        mat-list-item
        routerLink="/operations/admin/management/menu"
        routerLinkActive="active"
        (click)="onNavClick()"
      >
        <mat-icon matListItemIcon>restaurant_menu</mat-icon>
        <span matListItemTitle>Menu</span>
      </a>
      <mat-divider></mat-divider>

      <h3 matSubheader>Oversight</h3>
      <a
        mat-list-item
        routerLink="/operations/admin/oversight/analytics"
        routerLinkActive="active"
        (click)="onNavClick()"
      >
        <mat-icon matListItemIcon>insights</mat-icon>
        <span matListItemTitle>Analytics</span>
      </a>
      <a
        mat-list-item
        routerLink="/operations/admin/oversight/auditlogs"
        routerLinkActive="active"
        (click)="onNavClick()"
      >
        <mat-icon matListItemIcon>history</mat-icon>
        <span matListItemTitle>Audit Logs</span>
      </a>
      <a
        mat-list-item
        routerLink="/operations/admin/oversight/billings-receipts"
        routerLinkActive="active"
        (click)="onNavClick()"
      >
        <mat-icon matListItemIcon>receipt</mat-icon>
        <span matListItemTitle>Billing & Receipts</span>
      </a>
      <a
        mat-list-item
        routerLink="/operations/admin/oversight/feedback"
        routerLinkActive="active"
        (click)="onNavClick()"
      >
        <mat-icon matListItemIcon>feedback</mat-icon>
        <span matListItemTitle>Feedback</span>
      </a>
      <mat-divider></mat-divider>

      <a
        mat-list-item
        routerLink="/operations/admin/profile"
        routerLinkActive="active"
        (click)="onNavClick()"
      >
        <mat-icon matListItemIcon>account_circle</mat-icon>
        <span matListItemTitle>Profile</span>
      </a>
    </mat-nav-list>
  </mat-sidenav>

  <mat-sidenav-content>
    <mat-toolbar color="primary">
      @if (isMobile()) {
      <button
        mat-icon-button
        (click)="sidebarOpen.set(!sidebarOpen())"
      >
        <mat-icon>menu</mat-icon>
      </button>
      }
      <span>Hotel Management</span>
      <span class="spacer"></span>
      <span>{{ userDisplayName() }}</span>
      <button
        mat-icon-button
        [matMenuTriggerFor]="userMenu"
        aria-label="Open user menu"
      >
        <mat-icon>account_circle</mat-icon>
      </button>
      <mat-menu #userMenu="matMenu">
        <button
          mat-menu-item
          routerLink="/operations/admin/profile"
        >
          <mat-icon>manage_accounts</mat-icon> Profile
        </button>
        <button
          mat-menu-item
          (click)="logout()"
        >
          <mat-icon>logout</mat-icon> Logout
        </button>
      </mat-menu>
    </mat-toolbar>

    <div class="content">
      <router-outlet></router-outlet>
    </div>
  </mat-sidenav-content>
</mat-sidenav-container>
```

### 7. State Management (All Signals via toSignal)

In `AdminShellComponent` class:

```ts
private breakpointObserver = inject(BreakpointObserver);

isMobile = toSignal(
  this.breakpointObserver.observe('(max-width: 768px)').pipe(
    map(result => result.matches)
  ),
  { initialValue: false }
);

sidebarOpen = signal(false);
userDisplayName = signal('Admin User');  // TODO: later replace with user profile from AuthService after /auth/me is implemented
```

### 8. Data Flow & API Calls

- **AuthService** (pre‑existing) provides `logout()` that clears token and navigates to `/auth`.
- **No new API calls** in the shell.

### 9. UI States

- Normal: Sidebar persistent (desktop) or overlay (mobile), content shows active child.
- Mobile overlay: Sidebar hidden by default; tap hamburger → open; tap link → close via `onNavClick()`.
- Active navigation item highlighted via `routerLinkActive` class.

### 10. Responsive Behaviour

- Desktop (>768px): Sidebar 250px wide, always visible.
- Mobile (≤768px): Sidebar overlay; hamburger icon visible in toolbar; `isMobile` signal drives mode.
- Toolbar always visible; user menu accessible.

### 11. Accessibility

- Sidebar has `aria-label="Main navigation"`.
- Toolbar menu button: `aria-label="Open user menu"`.
- All mat-icons are decorative (`aria-hidden="true"`).
- Keyboard: full tab order, Enter/Space activate links.
- On mobile sidebar open: focus moves to first nav item.

### 12. Integration Notes

- This shell is the **parent component** for all admin pages.
- The `adminGuard` is imported and used in the main route config.
- Placeholder components exist so every navigation resolves; they will be replaced later.

### 13. Dependencies (Imports for AdminShellComponent)

- `CommonModule`, `RouterModule`
- `MatSidenavModule`, `MatToolbarModule`, `MatListModule`, `MatIconModule`, `MatButtonModule`, `MatMenuModule`, `MatDividerModule`
- `LayoutModule` (from `@angular/cdk/layout` → `BreakpointObserver`)
- `toSignal` from `@angular/core/rxjs-interop`
- `map` from `rxjs/operators`
- `AuthService` (injected)
- `Router` (injected)

### 14. Router Requirements (Mandatory)

- Every sidebar item must resolve successfully.
- No navigation produces a 404.
- All placeholder routes render inside the parent `<router-outlet>`.
- Unknown child routes redirect to `dashboard`.

### 15. Shell Self‑Review Checklist (Agent must verify)

- All 11 routes exist in config.
- All 11 placeholder components exist with unique selectors.
- Every `routerLink` in template matches route path.
- All components are standalone.
- `<router-outlet>` present in shell template.
- `adminGuard` attached via `canMatch` and `canActivate`.
- Responsive: mobile overlay works, sidebar opens/closes.
- Logout calls `AuthService.logout()` and redirects to `/auth`.
- Sidebar links close on mobile via `onNavClick()`.

### 16. File Structure (all created in this spec)

```
src/
  app/
    core/
      guards/
        admin.guard.ts
    features/
      admin/
        admin-shell.component.ts
        admin-shell.component.html
        admin-shell.component.scss
        pages/
          dashboard.component.ts
          profile.component.ts
          management/
            room-management.component.ts
            room-type-management.component.ts
            staff-management.component.ts
            amenities-management.component.ts
            menu-management.component.ts
          oversight/
            analytics.component.ts
            audit-logs.component.ts
            billing-receipts.component.ts
            feedback.component.ts
```

**Placeholder selectors and class names (exact list):**

- `app-placeholder-dashboard` → `PlaceholderDashboardComponent`
- `app-placeholder-profile` → `PlaceholderProfileComponent`
- `app-placeholder-room-management` → `PlaceholderRoomManagementComponent`
- `app-placeholder-room-type-management` → `PlaceholderRoomTypeManagementComponent`
- `app-placeholder-staff-management` → `PlaceholderStaffManagementComponent`
- `app-placeholder-amenities-management` → `PlaceholderAmenitiesManagementComponent`
- `app-placeholder-menu-management` → `PlaceholderMenuManagementComponent`
- `app-placeholder-analytics` → `PlaceholderAnalyticsComponent`
- `app-placeholder-audit-logs` → `PlaceholderAuditLogsComponent`
- `app-placeholder-billing-receipts` → `PlaceholderBillingReceiptsComponent`
- `app-placeholder-feedback` → `PlaceholderFeedbackComponent`

Each placeholder component is standalone and has a template with `<p>Coming soon: [exact name as in nav]</p>`.

### 17. `onNavClick()` Method

Defined in `AdminShellComponent`:

```ts
onNavClick() {
  if (this.isMobile()) {
    this.sidebarOpen.set(false);
  }
}
```

_End of Specsheet: Admin Shell & Route Configuration .md_

