# Specsheet: Customer Shell & Route Configuration

## 1. Purpose
- Provide the **layout container** for all `/user` pages.
- Includes:
  - Persistent sidebar navigation (desktop) / overlay (mobile).
  - Top toolbar with app title and user menu (Profile, Logout).
  - `<router-outlet>` for lazy‑loaded child pages.
  - Authentication & role guard (`RegisteredUser` only).
- Ensure that navigating to any `/user/*` path without the `RegisteredUser` role redirects to `/auth`.

## 2. Route & Navigation
- **Parent path**: `/user`
- **Route config** (added to `app.routes.ts` after existing routes). Import `customerGuard`.
  ```ts
  import { customerGuard } from './core/guards/customer.guard';
  ```
- Full block:
  ```ts
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
          .then(m => m.PlaceholderCustomerBookingsComponent)
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
  ```
- **Navigation**: Sidebar links exactly match these paths. The sidebar is the primary navigation.

## 3. Authorization
- **customerGuard** (functional guard, file `src/app/core/guards/customer.guard.ts`):
  ```ts
  import { inject } from '@angular/core';
  import { CanActivateFn, CanMatchFn, Router } from '@angular/router';
  import { AuthService } from '../services/auth.service';

  export const customerGuard: CanActivateFn & CanMatchFn = () => {
    const auth = inject(AuthService);
    const router = inject(Router);
    if (auth.isAuthenticated() && auth.role() === 'RegisteredUser') {
      return true;
    }
    return router.createUrlTree(['/auth']);
  };
  ```
- Both `canActivate` and `canMatch` use this function.

## 4. Required Supporting Infrastructure (new)
- `customerGuard` as above.
- `UserShellComponent` (layout).
- 4 placeholder components (Dashboard, Bookings, Room Service, Profile).
- No modifications to existing services.

## 5. Component API (UserShellComponent)
- **Selector**: `app-user-shell`
- **Standalone**: `true`
- **Imports**: `CommonModule`, `RouterModule`, `MatSidenavModule`, `MatToolbarModule`, `MatListModule`, `MatIconModule`, `MatButtonModule`, `MatMenuModule`, `MatDividerModule`, `MatRippleModule`, `BreakpointObserver` (from CDK).
- **Exact import paths**:
  ```ts
  import { CommonModule } from '@angular/common';
  import { Component, inject, signal } from '@angular/core';
  import { RouterModule, Router } from '@angular/router';
  import { MatSidenavModule } from '@angular/material/sidenav';
  import { MatToolbarModule } from '@angular/material/toolbar';
  import { MatListModule } from '@angular/material/list';
  import { MatIconModule } from '@angular/material/icon';
  import { MatButtonModule } from '@angular/material/button';
  import { MatMenuModule } from '@angular/material/menu';
  import { MatDividerModule } from '@angular/material/divider';
  import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
  import { map } from 'rxjs/operators';
  import { toSignal } from '@angular/core/rxjs-interop';
  import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
  import { DestroyRef } from '@angular/core';
  import { AuthService } from '../../../core/services/auth.service';
  ```

- **Template** (exact – Angular 18 control flow):
  ```html
  <mat-sidenav-container>
    <!-- SIDEBAR -->
    <mat-sidenav
      #sidenav
      [mode]="isMobile() ? 'over' : 'side'"
      [opened]="isMobile() ? sidebarOpen() : true"
      aria-label="Customer navigation">
      <mat-toolbar color="primary">Hotel</mat-toolbar>
      <mat-nav-list>
        <a mat-list-item routerLink="/user/dashboard" routerLinkActive="active" (click)="onNavClick()">
          <mat-icon matListItemIcon>dashboard</mat-icon>
          <span matListItemTitle>Dashboard</span>
        </a>
        <a mat-list-item routerLink="/user/bookings" routerLinkActive="active" (click)="onNavClick()">
          <mat-icon matListItemIcon>book_online</mat-icon>
          <span matListItemTitle>My Bookings</span>
        </a>
        <a mat-list-item routerLink="/user/room-service" routerLinkActive="active" (click)="onNavClick()">
          <mat-icon matListItemIcon>room_service</mat-icon>
          <span matListItemTitle>Room Service</span>
        </a>
        <a mat-list-item routerLink="/user/profile" routerLinkActive="active" (click)="onNavClick()">
          <mat-icon matListItemIcon>account_circle</mat-icon>
          <span matListItemTitle>Profile</span>
        </a>
      </mat-nav-list>
    </mat-sidenav>

    <!-- MAIN CONTENT -->
    <mat-sidenav-content>
      <mat-toolbar color="primary">
        @if (isMobile()) {
          <button mat-icon-button (click)="sidebarOpen.set(!sidebarOpen())">
            <mat-icon>menu</mat-icon>
          </button>
        }
        <span>Hotel</span>
        <span class="spacer"></span>
        <button mat-icon-button [matMenuTriggerFor]="userMenu" aria-label="Open user menu">
          <mat-icon>account_circle</mat-icon>
        </button>
        <mat-menu #userMenu="matMenu">
          <button mat-menu-item routerLink="/user/profile">
            <mat-icon>manage_accounts</mat-icon> Profile
          </button>
          <button mat-menu-item (click)="logout()">
            <mat-icon>logout</mat-icon> Logout
          </button>
        </mat-menu>
      </mat-toolbar>

      <!-- ROUTER OUTLET -->
      <div class="content">
        <router-outlet></router-outlet>
      </div>
    </mat-sidenav-content>
  </mat-sidenav-container>
  ```

## 6. State Management (All Signals)
```ts
private breakpointObserver = inject(BreakpointObserver);
private authService = inject(AuthService);
private router = inject(Router);

isMobile = toSignal(
  this.breakpointObserver.observe('(max-width: 1024px)').pipe(map(r => r.matches)),
  { initialValue: false }
);

sidebarOpen = signal(false);

onNavClick() {
  if (this.isMobile()) {
    this.sidebarOpen.set(false);
  }
}

logout() {
  this.authService.logout();
  this.router.navigate(['/auth']);
}
```

No other state needed.

## 7. Data Flow & API Calls
- **AuthService** (existing) provides `logout()` that clears JWT, resets signals, and navigates to `/auth`.
- No new API calls in the shell.

## 8. UI States
- **Desktop** (>1024px): Sidebar persistent, content area fills remaining space.
- **Mobile/Tablet** (≤1024px): Sidebar overlay; hamburger icon visible in toolbar. Sidebar opens on top of content; clicking a nav item closes it.
- Active navigation item highlighted via `routerLinkActive`.

## 9. Responsive Behaviour
- **Desktop**: Sidebar width 250px, persistent.
- **Mobile/Tablet**: Sidebar overlay, full width when open, closes on navigation.
- Toolbar always visible; user menu accessible via icon button.
- Breakpoint set to 1024px to cover tablets as well.

## 10. Accessibility
- Sidebar: `aria-label="Customer navigation"`.
- Toolbar menu button: `aria-label="Open user menu"`.
- Mat-icons are decorative (`aria-hidden="true"`).
- Keyboard navigation: Tab order through nav items; Enter/Space to activate links.
- On mobile sidebar open, focus moves to first nav item.

## 11. Integration Notes
- The `customerGuard` is imported and used in the main route config.
- Placeholder components must be created so that every navigation resolves; they will be replaced later.
- The shell component must be added to the root route config as a lazy‑loaded component.
- The `AuthService` must already have a `logout()` method that navigates to `/auth`.

## 12. Dependencies (Imports for UserShellComponent)
- `CommonModule`, `RouterModule`, `MatSidenavModule`, `MatToolbarModule`, `MatListModule`, `MatIconModule`, `MatButtonModule`, `MatMenuModule`, `MatDividerModule`.
- `LayoutModule` from `@angular/cdk/layout` (for `BreakpointObserver`).
- `AuthService` (injected).
- `Router` (injected).

## 13. File Structure (for this spec)
```
src/
  app/
    core/
      guards/
        customer.guard.ts
    features/
      user/
        user-shell.component.ts
        user-shell.component.html
        user-shell.component.scss
        pages/
          dashboard.component.ts          (placeholder)
          bookings.component.ts           (placeholder)
          room-service.component.ts       (placeholder)
          profile.component.ts            (placeholder)
```

## 14. Placeholder Components
Each placeholder component is a minimal standalone component with a single line of text indicating what it is, e.g.:
```ts
@Component({
  selector: 'app-placeholder-customer-dashboard',
  standalone: true,
  template: `<p>Coming soon: Customer Dashboard</p>`,
})
export class PlaceholderCustomerDashboardComponent {}
```
Selectors and class names:
- `app-placeholder-customer-dashboard` → `PlaceholderCustomerDashboardComponent`
- `app-placeholder-customer-bookings` → `PlaceholderCustomerBookingsComponent`
- `app-placeholder-customer-room-service` → `PlaceholderCustomerRoomServiceComponent`
- `app-placeholder-customer-profile` → `PlaceholderCustomerProfileComponent`

## 15. Router Requirements (Mandatory)
- Every sidebar item must resolve successfully.
- No navigation produces a 404.
- All placeholder routes render inside the parent `<router-outlet>`.
- Unknown child routes redirect to `dashboard`.

## 16. Shell Self‑Review Checklist
- [ ] All 4 routes exist in config.
- [ ] All 4 placeholder components exist with unique selectors.
- [ ] Every `routerLink` in template matches route path.
- [ ] All components are standalone.
- [ ] `<router-outlet>` present in shell template.
- [ ] `customerGuard` attached via `canMatch` and `canActivate`.
- [ ] Responsive: mobile/tablet overlay works, sidebar opens/closes.
- [ ] Logout calls `AuthService.logout()` and navigates to `/auth`.
- [ ] Sidebar links close on mobile via `onNavClick()`.
- [ ] Welcome message is NOT in shell (it will be on Dashboard).

---
