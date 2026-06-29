# Specsheet C: Role Dashboard – SignalR Notification Integration

## 1. Purpose
- Integrate the `NotificationService` into the three role dashboard pages (Kitchen, Housekeeping, Maintenance) so that real‑time alerts appear when a new task or order is created.
- Each dashboard subscribes to its relevant event, increments a `refreshTrigger` signal to reload the `TaskDashboardComponent`, and shows a custom snackbar notification.
- The SignalR connection is started once per session (first dashboard visit after login).

## 2. Files to Modify

| File | Change |
|------|--------|
| `src/app/features/kitchen/pages/dashboard.component.ts` | Add refreshTrigger, inject NotificationService, subscribe to onNewFoodOrder, start connection. |
| `src/app/features/kitchen/pages/dashboard.component.html` | Bind `[refresh]` to `refreshTrigger()`. |
| `src/app/features/housekeeping/pages/dashboard.component.ts` | Add refreshTrigger, inject NotificationService, subscribe to onNewHousekeepingTask, start connection. |
| `src/app/features/housekeeping/pages/dashboard.component.html` | Bind `[refresh]`. |
| `src/app/features/maintenance/pages/dashboard.component.ts` | Add refreshTrigger, inject NotificationService, subscribe to onNewMaintenanceTask, start connection. |
| `src/app/features/maintenance/pages/dashboard.component.html` | Bind `[refresh]`. |

## 3. Changes – Kitchen Dashboard

### 3.1 TypeScript (`dashboard.component.ts`)
Add the following inside the class:

```typescript
import { Component, inject, signal } from '@angular/core';
import { NotificationService } from '../../../../core/services/notification.service';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

export class KitchenDashboardComponent {
  // ... existing config and injections

  private notificationService = inject(NotificationService);
  refreshTrigger = signal(0);

  constructor() {
    // Start SignalR connection
    this.notificationService.startConnection();

    // Subscribe to new food orders
    this.notificationService.onNewFoodOrder.pipe(
      takeUntilDestroyed()
    ).subscribe(order => {
      this.refreshTrigger.update(n => n + 1);
      this.notificationService.showNotification(
        'New Order!',
        `Order #${order.id}${order.roomNumber ? ' for Room ' + order.roomNumber : ''}`
      );
    });
  }
}
```

### 3.2 Template (`dashboard.component.html`)
Update the `<app-task-dashboard>` element to include the refresh binding:

```html
<app-task-dashboard [config]="config" [refresh]="refreshTrigger()" />
```

## 4. Changes – Housekeeping Dashboard
Identical to kitchen, but subscribe to `onNewHousekeepingTask`:

```typescript
this.notificationService.onNewHousekeepingTask.pipe(
  takeUntilDestroyed()
).subscribe(task => {
  this.refreshTrigger.update(n => n + 1);
  this.notificationService.showNotification(
    'New Housekeeping Task',
    `${task.description}${task.roomNumber ? ' – Room ' + task.roomNumber : ''}`
  );
});
```

## 5. Changes – Maintenance Dashboard
Identical, subscribe to `onNewMaintenanceTask`:

```typescript
this.notificationService.onNewMaintenanceTask.pipe(
  takeUntilDestroyed()
).subscribe(task => {
  this.refreshTrigger.update(n => n + 1);
  this.notificationService.showNotification(
    'New Maintenance Task',
    `${task.description}${task.roomNumber ? ' – Room ' + task.roomNumber : ''}`
  );
});
```

## 6. Ensure `destroyRef` Usage
If `takeUntilDestroyed()` without an explicit `DestroyRef` argument is used, it must be called inside an injection context (which the constructor is). That works. However, to be safe, we can inject `DestroyRef` and pass it:

```typescript
private destroyRef = inject(DestroyRef);

// then:
.pipe(takeUntilDestroyed(this.destroyRef))
```

Use this pattern for consistency with other specs.

## 7. Connection Start Guard
The `startConnection()` method in `NotificationService` checks if the connection already exists or token is missing; it's safe to call multiple times. So each dashboard can call it.

To avoid starting the connection before login, we can also call it in the shell after login? But the dashboard is only accessible after login, so it's fine.

## 8. Self‑Review Checklist
- [ ] When a new food order is created (by guest/front desk), Kitchen dashboard automatically refreshes and shows a green notification.
- [ ] Housekeeping and Maintenance dashboards similarly react to their respective events.
- [ ] Notification snackbar appears at top‑right with correct message and auto‑dismisses.
- [ ] The table and summary counts update without manual refresh.
- [ ] SignalR connection is established once and persists across navigation within the role portal.
- [ ] No console errors; subscriptions are cleaned up on component destroy.

## 9. Integration Notes
- The `NotificationService` is already provided in root; no additional providers are needed.
- The `refresh` input on `TaskDashboardComponent` was added in Specsheet B; this spec completes the wiring.
- The shell components are unchanged; only the dashboard pages are modified.
- After these three specs, the entire notification feature is complete.

---

