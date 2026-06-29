# Patch Specsheet: SignalR Notification Service – Align with Backend Events

## 1. Purpose
- Fix the SignalR integration to match the backend hub implementation.
- The backend sends events using the method **`ReceiveAlert`** with a string message. The frontend currently listens for non‑existent methods (`NewFoodOrder`, etc.), causing missed notifications.
- This patch updates the `NotificationService` to listen for `ReceiveAlert` and exposes a single `onAlert` observable.
- The role dashboards are updated to subscribe to `onAlert` and display the message in the custom snackbar.
- The connection logic from Specsheet A remains unchanged (skip negotiation, WebSocket transport). The connection is now successfully established, so no further URL changes are needed.

## 2. Files to Modify
| File | Change |
|------|--------|
| `src/app/core/services/notification.service.ts` | Replace three specific event listeners with a single `ReceiveAlert` handler; expose `onAlert` Subject. |
| `src/app/features/kitchen/pages/dashboard.component.ts` | Subscribe to `onAlert` instead of `onNewFoodOrder`. |
| `src/app/features/housekeeping/pages/dashboard.component.ts` | Subscribe to `onAlert` instead of `onNewHousekeepingTask`. |
| `src/app/features/maintenance/pages/dashboard.component.ts` | Subscribe to `onAlert` instead of `onNewMaintenanceTask`. |

## 3. Changes to `NotificationService`

### 3.1 Remove old event Subjects
Delete:
```typescript
readonly onNewFoodOrder = new Subject<NewTaskNotification>();
readonly onNewHousekeepingTask = new Subject<NewTaskNotification>();
readonly onNewMaintenanceTask = new Subject<NewTaskNotification>();
```

### 3.2 Add new generic `onAlert` Subject
```typescript
readonly onAlert = new Subject<NewTaskNotification>();
```

Keep the `NewTaskNotification` interface as is (it can hold message and optional room number). We'll populate it from the received string.

### 3.3 Register `ReceiveAlert` handler
In `startConnection()`, replace the three `.on(...)` registrations with:

```typescript
this.hubConnection.on('ReceiveAlert', (message: string) => {
  // Parse the message string to extract useful info
  const notification: NewTaskNotification = {
    id: 0,
    type: 'FoodOrder', // default, but we don't know type from string; we can try to infer
    description: message,
    roomNumber: undefined
  };
  // Try to extract type from message
  if (message.toLowerCase().includes('housekeeping')) {
    notification.type = 'Housekeeping';
  } else if (message.toLowerCase().includes('maintenance')) {
    notification.type = 'Maintenance';
  } else if (message.toLowerCase().includes('order') || message.toLowerCase().includes('food')) {
    notification.type = 'FoodOrder';
  }
  this.onAlert.next(notification);
});
```

**Note:** Method names in SignalR are case‑insensitive; registering `'ReceiveAlert'` will handle `'receivealert'` as seen in the error.

### 3.4 Update `showNotification` signature (if needed)
It already takes `title` and `message` strings. No change.

## 4. Changes to Role Dashboards

### 4.1 Kitchen Dashboard
Replace:
```typescript
this.notificationService.onNewFoodOrder.pipe(...)
```
with:
```typescript
this.notificationService.onAlert.pipe(
  takeUntilDestroyed(this.destroyRef)
).subscribe(notification => {
  this.refreshTrigger.update(n => n + 1);
  this.notificationService.showNotification(
    notification.type === 'FoodOrder' ? 'New Order!' : 'Alert',
    notification.description
  );
});
```

### 4.2 Housekeeping Dashboard
Replace:
```typescript
this.notificationService.onNewHousekeepingTask.pipe(...)
```
with:
```typescript
this.notificationService.onAlert.pipe(
  takeUntilDestroyed(this.destroyRef)
).subscribe(notification => {
  this.refreshTrigger.update(n => n + 1);
  this.notificationService.showNotification('New Task', notification.description);
});
```

### 4.3 Maintenance Dashboard
Replace:
```typescript
this.notificationService.onNewMaintenanceTask.pipe(...)
```
with:
```typescript
this.notificationService.onAlert.pipe(
  takeUntilDestroyed(this.destroyRef)
).subscribe(notification => {
  this.refreshTrigger.update(n => n + 1);
  this.notificationService.showNotification('New Task', notification.description);
});
```

## 5. Self‑Review Checklist
- [ ] WebSocket connection succeeds (already confirmed).
- [ ] `ReceiveAlert` events are received and trigger `onAlert`.
- [ ] Kitchen, Housekeeping, and Maintenance dashboards show a green notification snackbar when a relevant alert arrives.
- [ ] The dashboard table and summary counts refresh automatically after each alert.
- [ ] No console errors related to missing methods or parsing.
- [ ] Multiple alerts in succession update the UI correctly.

## 6. Integration Notes
- The `NotificationService.startConnection()` is already called by each dashboard on init; it's safe to call multiple times because of the guard `if (this.hubConnection) return;`.
- The `NewTaskNotification` interface is still used; the `type` field is inferred from the message string for display purposes. The `description` contains the original backend message.
- The backend cross‑broadcasts housekeeping alerts to Maintenance group; the maintenance dashboard will show those alerts as well, which is intentional for demo. The type inference will label them as "Housekeeping" or "Maintenance" based on keywords.
- No further changes to the TaskDashboardComponent or shells are needed.

---

