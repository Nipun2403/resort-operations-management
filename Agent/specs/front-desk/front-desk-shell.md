# Specsheet: Front Desk Shell & Route Configuration

## 1. Purpose

- Provide the layout container for all `/operations/front-desk` pages.
- Includes:
  - Persistent sidebar navigation (desktop) / overlay (mobile/tablet ≤1024px).
  - Top toolbar with dynamic title and user menu (Profile, Logout).
  - `<router-outlet>` for lazy‑loaded child pages.
  - Role guard (`FrontDesk` only).

## 2. Route & Navigation

- **Parent path**: `/operations/front-desk`
- **Route config** (added to `app.routes.ts` after existing routes):

  ```ts
  import { frontDeskGuard } from './core/guards/front-desk.guard';

  {
    path: 'operations/front-desk',
    canMatch: [frontDeskGuard],
    canActivate: [frontDeskGuard],
    loadComponent: () => import('./features/front-desk/front-desk-shell.component')
      .then(m => m.FrontDeskShellComponent),
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        loadComponent: () => import('./features/front-desk/pages/dashboard.component')
          .then(m => m.PlaceholderDashboardComponent)
      },
      {
        path: 'new-booking',
        loadComponent: () => import('./features/front-desk/pages/new-booking.component')
          .then(m => m.PlaceholderNewBookingComponent)
      },
      {
        path: 'profile',
        loadComponent: () => import('./features/front-desk/pages/profile.component')
          .then(m => m.PlaceholderProfileComponent)
      },
      { path: '**', redirectTo: 'dashboard' }
    ]
  }
  ```

- **Sidebar links**: Dashboard, New Booking. Profile is only accessed via the top‑right user icon dropdown.

## 3. Authorization

- **frontDeskGuard** (functional, file `src/app/core/guards/front-desk.guard.ts`):

  ```ts
  import { inject } from "@angular/core";
  import { CanActivateFn, CanMatchFn, Router } from "@angular/router";
  import { AuthService } from "../services/auth.service";

  export const frontDeskGuard: CanActivateFn & CanMatchFn = () => {
    const auth = inject(AuthService);
    const router = inject(Router);
    if (auth.isAuthenticated() && auth.role() === "FrontDesk") {
      return true;
    }
    return router.createUrlTree(["/auth"]);
  };
  ```

## 4. Required Supporting Infrastructure (new)

- `frontDeskGuard` as above.
- `FrontDeskShellComponent` (layout).
- 3 placeholder components: `PlaceholderDashboardComponent`, `PlaceholderNewBookingComponent`, `PlaceholderProfileComponent`.
- No modifications to existing services.

## 5. Component API (FrontDeskShellComponent)

- **Selector**: `app-front-desk-shell`
- **Standalone**: `true`
- **Imports**: `CommonModule`, `RouterModule`, `MatSidenavModule`, `MatToolbarModule`, `MatListModule`, `MatIconModule`, `MatButtonModule`, `MatMenuModule`, `MatDividerModule`, `BreakpointObserver` (CDK).
- **Exact import paths** (abbreviated, agent must include full paths).
- **Template** (exact):

  ```html
  <mat-sidenav-container>
    <mat-sidenav
      #sidenav
      [mode]="isMobile() ? 'over' : 'side'"
      [opened]="isMobile() ? sidebarOpen() : true"
      aria-label="Front Desk navigation"
    >
      <mat-toolbar color="primary">Front Desk</mat-toolbar>
      <mat-nav-list>
        <a
          mat-list-item
          routerLink="/operations/front-desk/dashboard"
          routerLinkActive="active"
          (click)="onNavClick()"
        >
          <mat-icon matListItemIcon>dashboard</mat-icon>
          <span matListItemTitle>Dashboard</span>
        </a>
        <a
          mat-list-item
          routerLink="/operations/front-desk/new-booking"
          routerLinkActive="active"
          (click)="onNavClick()"
        >
          <mat-icon matListItemIcon>add_circle</mat-icon>
          <span matListItemTitle>New Booking</span>
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
        <span>{{ title() }}</span>
        <span class="spacer"></span>
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
            routerLink="/operations/front-desk/profile"
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

title = signal('Front Desk'); // will be overridden by route data later, but static for now
```

(Include dynamic title logic as in Admin Shell if desired; for now, keep static “Front Desk”.)

```ts
onNavClick() { if (this.isMobile()) this.sidebarOpen.set(false); }
logout() { this.authService.logout(); this.router.navigate(['/auth']); }
```

## 7. Placeholder Components

- `PlaceholderDashboardComponent`: `<p>Coming soon: Front Desk Dashboard</p>`
- `PlaceholderNewBookingComponent`: `<p>Coming soon: New Booking</p>`
- `PlaceholderProfileComponent`: `<p>Coming soon: Profile</p>`
- Each standalone, selectors `app-placeholder-frontdesk-dashboard` etc.

## 8. Router Requirements

- Every sidebar item resolves.
- Unknown child routes redirect to dashboard.
- All placeholders render inside `<router-outlet>`.

## 9. Self‑Review Checklist

- [ ] Guard prevents non‑FrontDesk access.
- [ ] Sidebar shows Dashboard and New Booking links; Profile via user menu.
- [ ] Mobile/tablet overlay works.
- [ ] Logout clears token and redirects to `/auth`.
- [ ] Placeholder pages load correctly.

---

