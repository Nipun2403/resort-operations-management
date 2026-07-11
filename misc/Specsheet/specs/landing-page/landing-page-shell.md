# Specsheet: Public Shell & Routes

## 1. Purpose
- Provide the layout container for all public‑facing pages of the hotel website.
- **No authentication** required – this shell is fully public.
- Includes a **header** with navigation links (Home, Rooms, Menu, Amenities), a **Login** button, and an **Availability** button.
- A minimal **footer** with hotel name and address.
- Child pages (Home, Rooms, Menu, Amenities, Availability) are rendered inside a `<router-outlet>`.

## 2. Route Configuration
Add to `src/app/app.routes.ts` a new lazy‑loaded route for the public site:

```typescript
{
  path: '',
  loadComponent: () => import('./features/public/public-shell.component')
    .then(m => m.PublicShellComponent),
  children: [
    { path: '', redirectTo: 'home', pathMatch: 'full' },
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
      path: 'menu',
      loadComponent: () => import('./features/public/pages/menu.component')
        .then(m => m.MenuComponent)
    },
    {
      path: 'amenities',
      loadComponent: () => import('./features/public/pages/amenities.component')
        .then(m => m.AmenitiesComponent)
    },
    {
      path: 'availability',
      loadComponent: () => import('./features/public/pages/availability.component')
        .then(m => m.AvailabilityComponent)
    },
    { path: '**', redirectTo: 'home' }
  ]
}
```

**Note:** The `/auth` route already exists and should be outside this public shell, as it has its own layout. Ensure the public routes do not conflict with existing `/operations/admin`, `/operations/front-desk`, etc.

## 3. PublicShellComponent

### 3.1 API
- **Selector**: `app-public-shell`
- **Standalone**: `true`
- **Imports**: `CommonModule`, `RouterModule`, `MatToolbarModule`, `MatButtonModule`, `MatIconModule`, `MatMenuModule`, `BreakpointObserver` (from CDK).
- **Exact import paths** (abbreviated; agent must use correct paths).

### 3.2 Template (exact – Angular 18 control flow)
```html
<!-- Header -->
<mat-toolbar color="primary" class="public-header">
  <!-- Logo / Hotel Name -->
  <span class="logo" routerLink="/home">Hotel Name</span>
  <span class="spacer"></span>

  <!-- Desktop Navigation -->
  <nav class="desktop-nav" @if (!isMobile())>
    <a mat-button routerLink="/home" routerLinkActive="active">Home</a>
    <a mat-button routerLink="/rooms" routerLinkActive="active">Rooms</a>
    <a mat-button routerLink="/menu" routerLinkActive="active">Menu</a>
    <a mat-button routerLink="/amenities" routerLinkActive="active">Amenities</a>
    <a mat-raised-button color="accent" routerLink="/availability">Check Availability</a>
    <a mat-stroked-button routerLink="/auth">Login</a>
  </nav>

  <!-- Mobile Hamburger -->
  @if (isMobile()) {
    <button mat-icon-button [matMenuTriggerFor]="mobileMenu" aria-label="Menu">
      <mat-icon>menu</mat-icon>
    </button>
    <mat-menu #mobileMenu="matMenu">
      <a mat-menu-item routerLink="/home">Home</a>
      <a mat-menu-item routerLink="/rooms">Rooms</a>
      <a mat-menu-item routerLink="/menu">Menu</a>
      <a mat-menu-item routerLink="/amenities">Amenities</a>
      <a mat-menu-item routerLink="/availability">Check Availability</a>
      <a mat-menu-item routerLink="/auth">Login</a>
    </mat-menu>
  }
</mat-toolbar>

<!-- Main Content -->
<main>
  <router-outlet></router-outlet>
</main>

<!-- Footer -->
<footer class="public-footer">
  <p>&copy; 2026 Hotel Name. All rights reserved.</p>
  <p>123 Luxury Lane, Paradise City</p>
</footer>
```

### 3.3 State & Logic
```typescript
export class PublicShellComponent {
  private breakpointObserver = inject(BreakpointObserver);
  isMobile = toSignal(
    this.breakpointObserver.observe('(max-width: 768px)').pipe(map(r => r.matches)),
    { initialValue: false }
  );
}
```

### 3.4 Styling (public-shell.component.scss)
```scss
.public-header {
  position: sticky;
  top: 0;
  z-index: 10;
  .logo {
    font-size: 1.4rem;
    font-weight: 600;
    cursor: pointer;
    text-decoration: none;
    color: white;
  }
  .spacer { flex: 1 1 auto; }
  .desktop-nav { display: flex; gap: 8px; }
  a.active { font-weight: bold; border-bottom: 2px solid white; }
}
.public-footer {
  background: #f5f5f5;
  text-align: center;
  padding: 16px;
  margin-top: 48px;
  p { margin: 4px 0; color: #666; }
}
```

## 4. Placeholder Pages
Create minimal placeholder components for Home, Rooms, RoomDetail, Menu, Amenities, and Availability so that navigation works on day one. Each placeholder simply displays `<p>Coming soon: [page name]</p>`. We will replace them with real specs later.

**Example placeholder:**
```typescript
@Component({
  selector: 'app-placeholder-home',
  standalone: true,
  template: `<p>Coming soon: Home</p>`
})
export class PlaceholderHomeComponent {}
```

## 5. Self‑Review Checklist
- [ ] Public shell loads at `/` and shows the header with navigation links.
- [ ] Clicking each nav link navigates to the corresponding child route and placeholder page.
- [ ] On mobile (≤768px), nav links collapse into a hamburger menu.
- [ ] “Check Availability” and “Login” buttons navigate correctly.
- [ ] Footer appears at the bottom of every page.
- [ ] No console errors, no auth guards blocking public routes.
- [ ] Existing routes (admin, front-desk, user, etc.) are unaffected.

## 6. Integration Notes
- The `PublicShellComponent` is added as a lazy‑loaded component in `app.routes.ts`. It does not use the existing admin or user guards.
- The `RouterModule` is already imported in the shell; child routes are configured in the same route block.
- No additional services are needed.

---

