# Specsheet: Kitchen, Housekeeping & Maintenance Shells and Dashboards

## 1. Purpose
- Create the role‑specific portals for **Kitchen**, **Housekeeping**, and **Maintenance** staff.
- Each portal consists of a minimal shell (sidebar with “Dashboard” link, top bar with dynamic title, profile/logout menu) and a single dashboard page that hosts the shared `TaskDashboardComponent`.
- All three roles are identical in structure; only the configuration (entity name, API calls, status options, detail sections) differs.
- Strong guards prevent role escalation and URL manipulation.

## 2. Common Architecture
For each role `X` (Kitchen, Housekeeping, Maintenance), we create:

- `src/app/features/X/X-shell.component.ts` – layout container (sidebar, top bar, router‑outlet).
- `src/app/features/X/pages/dashboard.component.ts` – thin wrapper that builds the `TaskDashboardConfig` and passes it to `<app-task-dashboard>`.
- `src/app/core/guards/X.guard.ts` – functional guard checking JWT role.
- Route configuration under `/operations/X`.

All shells reuse the same pattern as AdminShell and FrontDeskShell (using `MatSidenav`, `BreakpointObserver`, etc.) but with only one nav item: “Dashboard”. Profile is accessed via the top‑right user icon dropdown.

The dashboard page injects the appropriate API service(s) and constructs the configuration for `TaskDashboardComponent`. No additional logic is needed.

## 3. Route Configuration
Add to `app.routes.ts` (after existing routes):

```typescript
import { kitchenGuard } from './core/guards/kitchen.guard';
import { housekeepingGuard } from './core/guards/housekeeping.guard';
import { maintenanceGuard } from './core/guards/maintenance.guard';

// Kitchen
{
  path: 'operations/kitchen',
  canMatch: [kitchenGuard],
  canActivate: [kitchenGuard],
  loadComponent: () => import('./features/kitchen/kitchen-shell.component')
    .then(m => m.KitchenShellComponent),
  children: [
    { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
    {
      path: 'dashboard',
      loadComponent: () => import('./features/kitchen/pages/dashboard.component')
        .then(m => m.KitchenDashboardComponent)
    },
    { path: '**', redirectTo: 'dashboard' }
  ]
},
// Housekeeping (similar structure, replace guard and paths with 'housekeeping')
// Maintenance (similar)
```

## 4. Guards (exact code for each)

**File:** `src/app/core/guards/kitchen.guard.ts`
```typescript
import { inject } from '@angular/core';
import { CanActivateFn, CanMatchFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const kitchenGuard: CanActivateFn & CanMatchFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  if (auth.isAuthenticated() && auth.role() === 'Kitchen') {
    return true;
  }
  return router.createUrlTree(['/auth']);
};
```
Similarly for `housekeepingGuard` (role `'Housekeeping'`) and `maintenanceGuard` (role `'Maintenance'`).

## 5. Shell Components
For each role, the shell is structurally identical to the FrontDeskShell but with a different title and only one sidebar link.

**Template (exact):**
```html
<mat-sidenav-container>
  <mat-sidenav #sidenav [mode]="isMobile() ? 'over' : 'side'" [opened]="isMobile() ? sidebarOpen() : true" aria-label="Navigation">
    <mat-toolbar color="primary">{{ roleTitle }}</mat-toolbar>
    <mat-nav-list>
      <a mat-list-item routerLink="./dashboard" routerLinkActive="active" (click)="onNavClick()">
        <mat-icon matListItemIcon>dashboard</mat-icon>
        <span matListItemTitle>Dashboard</span>
      </a>
    </mat-nav-list>
  </mat-sidenav>

  <mat-sidenav-content>
    <mat-toolbar color="primary">
      @if (isMobile()) {
        <button mat-icon-button (click)="sidebarOpen.set(!sidebarOpen())">
          <mat-icon>menu</mat-icon>
        </button>
      }
      <span>{{ roleTitle }}</span>
      <span class="spacer"></span>
      <button mat-icon-button [matMenuTriggerFor]="userMenu" aria-label="Open user menu">
        <mat-icon>account_circle</mat-icon>
      </button>
      <mat-menu #userMenu="matMenu">
        <button mat-menu-item routerLink="/operations/{{ role }}/profile" *ngIf="false">Profile</button>
        <button mat-menu-item (click)="logout()"><mat-icon>logout</mat-icon> Logout</button>
      </mat-menu>
    </mat-toolbar>

    <div class="content">
      <router-outlet></router-outlet>
    </div>
  </mat-sidenav-content>
</mat-sidenav-container>
```
**TypeScript:**
```typescript
@Component({...})
export class KitchenShellComponent {
  private breakpointObserver = inject(BreakpointObserver);
  private authService = inject(AuthService);
  private router = inject(Router);

  isMobile = toSignal(this.breakpointObserver.observe('(max-width: 1024px)').pipe(map(r => r.matches)), { initialValue: false });
  sidebarOpen = signal(false);
  roleTitle = 'Kitchen';

  onNavClick() { if (this.isMobile()) this.sidebarOpen.set(false); }
  logout() { this.authService.logout(); this.router.navigate(['/auth']); }
}
```
For housekeeping, `roleTitle = 'Housekeeping'`; for maintenance, `roleTitle = 'Maintenance'`.

No profile link in the sidebar (profile accessible via top‑right icon only, but we haven't built profile pages yet; for now, just logout).

## 6. Dashboard Components
Each dashboard page is a minimal standalone component that constructs the configuration and renders `<app-task-dashboard>`.

### 6.1 Kitchen Dashboard
**File:** `src/app/features/kitchen/pages/dashboard.component.ts`

```typescript
import { Component, inject } from '@angular/core';
import { TaskDashboardComponent } from '../../../../shared/components/task-dashboard/task-dashboard.component';
import { TaskDashboardConfig, Task, DetailSection } from '../../../../shared/models/task.model';
import { OrderApiService } from '../../../admin/services/order-api.service'; // adjust path

@Component({
  selector: 'app-kitchen-dashboard',
  standalone: true,
  imports: [TaskDashboardComponent],
  template: `<app-task-dashboard [config]="config" />`,
})
export class KitchenDashboardComponent {
  private orderApi = inject(OrderApiService);

  config: TaskDashboardConfig = {
    entityName: 'Food Order',
    fetchTasks: (params) => this.orderApi.getAll(params).pipe(
      map(res => ({
        totalCount: res.totalCount,
        data: res.data.map(order => ({
          id: order.id,
          status: order.status, // 'Pending', 'Preparing', 'Delivered'
          location: order.roomId ? `Room ${order.roomId}` : 'N/A',
          description: `Order #${order.id}`,
          createdAt: order.bookedAt || order.generatedAt,
          raw: order,
        } as Task))
      }))
    ),
    updateTaskStatus: (id, newStatus) => this.orderApi.updateStatus(id, { status: newStatus }), // ensure method exists
    statusOptions: [
      { value: 'All', label: 'All' },
      { value: 'Pending', label: 'Pending' },
      { value: 'Preparing', label: 'Preparing' },
      { value: 'Delivered', label: 'Delivered' },
    ],
    getLocation: (t) => t.location,
    getDescription: (t) => t.description,
    getDetailSections: (t) => {
      const order = t.raw as any;
      const items = order.items ? order.items.map((i: any) => `${i.quantity}x ${i.name}`).join(', ') : 'None';
      return [
        { title: 'Order Information', fields: [
          { label: 'Order ID', value: String(order.id) },
          { label: 'Status', value: order.status },
          { label: 'Items', value: items },
          { label: 'Created At', value: order.bookedAt ? new Date(order.bookedAt).toLocaleString() : 'N/A' },
        ]},
      ] as DetailSection[];
    },
  };
}
```
**Note:** The `OrderApiService` must have an `updateStatus(id, body)` method (calling `PATCH /api/v1/orders/{id}`). If not present, add it.

### 6.2 Housekeeping Dashboard
Similar to kitchen but uses `HousekeepingApiService`. Map `HousekeepingTask` to `Task`:
- `status`: Pending, InProgress, Completed
- `location`: `task.location` (already "Room xxx")
- `description`: `task.description`
- `createdAt`: `task.createdAt`
- `raw`: the full DTO.

`updateTaskStatus` calls `PATCH /api/v1/housekeeping/{id}/status` with body `{ status: newStatus }`.

Status options: All, Pending, In Progress (InProgress), Completed.

Detail sections show roomId, location, description, status, createdAt, originType.

### 6.3 Maintenance Dashboard
Uses `MaintenanceApiService`. Identical to housekeeping but calls maintenance endpoints.

`updateTaskStatus` calls `PATCH /api/v1/maintenance/{id}/status`.

Status options: All, Pending, In Progress, Completed.

Detail sections show roomId, location, description, status, createdAt, originType.

## 7. Shared Services Update
Ensure the following API methods exist (add if missing):

- `OrderApiService.updateStatus(id: number, dto: { status: string }): Observable<void>` – `PATCH /api/v1/orders/{id}` (per Swagger).
- `HousekeepingApiService.updateStatus(id: number, dto: { status: string }): Observable<void>` – `PATCH /api/v1/housekeeping/{id}/status`.
- `MaintenanceApiService.updateStatus(id: number, dto: { status: string }): Observable<void>` – `PATCH /api/v1/maintenance/{id}/status`.

## 8. Responsive Behaviour
- Shell: same overlay sidebar on ≤1024px as in admin.
- Dashboard: `TaskDashboardComponent` already responsive.

## 9. Self‑Review Checklist
- [ ] Guards prevent access by other roles; navigating to `/operations/kitchen` as a non‑kitchen role redirects to `/auth`.
- [ ] Shell loads with sidebar (Dashboard link) and top bar.
- [ ] Dashboard shows correct summary counts and table for the role’s tasks.
- [ ] Status filter works; clicking summary card filters.
- [ ] Detail modal displays full information.
- [ ] Start/Complete buttons transition status correctly and refresh data.
- [ ] Mobile sidebar works.
- [ ] Logout clears token and redirects.
- [ ] No console errors, subscriptions cleaned.

## 10. Integration Notes
- The three shells are created in separate feature folders: `kitchen/`, `housekeeping/`, `maintenance/`.
- Placeholder profile components can be omitted; the user menu only shows Logout.
- The `TaskDashboardComponent` is imported from `shared/components/task-dashboard`.
- The API services are already provided in root; no new providers needed.
- After these three portals are built, the only remaining piece is the shared Profile page for all roles (including Admin, Front Desk, etc.), which can be built later.

---
