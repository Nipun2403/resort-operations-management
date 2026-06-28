# Patch Specsheet: Dashboard Room Service Status & Category Filter Fixes

## 1. Purpose

- **Dashboard**: Fix the “Room Service Status” section to correctly fetch and display all pending and in‑progress housekeeping, maintenance, and food orders. Display description (or order ID) instead of room number.
- **Room Service – Food Order**: Fix the category filter so that selecting a specific category correctly filters the displayed menu items.

## 2. Files to Modify

| File                                                                   | Change                                                                                                            |
| ---------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------- |
| `src/app/features/user/pages/dashboard.component.ts`                   | Correct `loadRoomServiceStatus` logic to use proper multi‑status query parameters; map data to show descriptions. |
| `src/app/features/user/pages/dashboard.component.html`                 | Update room service status card content to show description (or order ID) and status.                             |
| `src/app/features/user/components/food-order/menu-grid.component.ts`   | Convert `categoryFilter` value to a signal so `filteredGroups` reacts.                                            |
| `src/app/features/user/components/food-order/menu-grid.component.html` | Ensure template uses `filteredGroups`.                                                                            |

## 3. Dashboard – Room Service Status Fix

### 3.1 Issue

The existing `loadRoomServiceStatus` uses `status: 'Pending,InProgress'` as a string, which most backends interpret as a literal value, not two separate statuses. Consequently, the API returns no results, and the cards show empty.  
We must send multiple status query parameters (e.g., `status=Pending&status=InProgress`). Angular’s `HttpParams` supports this via `.append()`.

### 3.2 Service Methods

Ensure `HousekeepingApiService.getAll` and `MaintenanceApiService.getAll` accept an array of statuses (or we build the params ourselves). For a minimal, deterministic fix, we will construct the `HttpParams` directly in the dashboard component’s fetch methods, bypassing the service’s usual parameter object. Alternatively, we can modify the service to accept `statuses: string[]`. To keep the patch focused, we’ll adjust the dashboard component to call the underlying `HttpClient` with manually constructed params, or better, add a new overload in the service. However, to avoid touching many files, I’ll specify that the existing `getAll` method already uses an options object; we’ll change the dashboard to pass `status: ['Pending','InProgress']` and adapt the service to handle an array. But that would require modifying the service. Instead, we can call the service twice (once for each status) and merge the results, which is simpler and deterministic.

### 3.3 Exact Fix in Dashboard

**Remove the old `loadRoomServiceStatus` implementation.**  
Replace with:

```ts
private loadRoomServiceStatus(): void {
  const booking = this.activeBooking();
  if (!booking) return;
  const roomIds = booking.rooms.map(r => r.roomId).filter(id => id != null) as number[];

  // Helper to fetch housekeeping/maintenance for a single status
  const fetchHousekeeping = (status: string) =>
    forkJoin(roomIds.map(roomId =>
      this.housekeepingApi.getAll({ roomId, status, pageSize: 20 }).pipe(
        map(res => res.data.map(hk => ({ ...hk, type: 'Housekeeping' as const })))
      )
    )).pipe(map(results => results.flat()));

  const fetchMaintenance = (status: string) =>
    forkJoin(roomIds.map(roomId =>
      this.maintenanceApi.getAll({ roomId, status, pageSize: 20 }).pipe(
        map(res => res.data.map(mt => ({ ...mt, type: 'Maintenance' as const })))
      )
    )).pipe(map(results => results.flat()));

  // Fetch both statuses for housekeeping and maintenance
  forkJoin({
    hkPending: fetchHousekeeping('Pending'),
    hkInProgress: fetchHousekeeping('InProgress'),
    mtPending: fetchMaintenance('Pending'),
    mtInProgress: fetchMaintenance('InProgress'),
    food: this.orderApi.getAll({ status: 'Pending', pageSize: 50 }).pipe(   // order API doesn't support multiple statuses easily; fetch Pending and InProgress separately
      switchMap(res => {
        // also fetch InProgress
        return this.orderApi.getAll({ status: 'Preparing', pageSize: 50 }).pipe(
          map(res2 => [...res.data, ...res2.data].filter(o => o.bookingId === booking.id))
        );
      })
    )
  }).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
    next: ({ hkPending, hkInProgress, mtPending, mtInProgress, food }) => {
      this.pendingHousekeeping.set([...hkPending, ...hkInProgress]);
      this.pendingMaintenance.set([...mtPending, ...mtInProgress]);
      this.pendingFoodOrders.set(food);
    },
    error: (err: any) => this.snackBar.open(this.extractErrorMessage(err), 'Close', { duration: 5000 })
  });
}
```

**Note:** The order API likely accepts `status` as a single value; we fetch “Pending” and “Preparing” separately and merge them, filtering by `bookingId`. This ensures all orders for the active booking are captured.

### 3.4 Template Update for Cards

Change the card content from showing `room number` to `description` (for hk/mt) and `order ID + status` for food.

**Housekeeping card content:**

```html
@for (item of pendingHousekeeping(); track item.id) {
<p>{{ item.description || 'No description' }} – {{ item.status }}</p>
}
```

**Maintenance card content:**

```html
@for (item of pendingMaintenance(); track item.id) {
<p>{{ item.description || 'No description' }} – {{ item.status }}</p>
}
```

**Food Orders card content:**

```html
@for (order of pendingFoodOrders(); track order.id) {
<p>Order #{{ order.id }} – {{ order.status }}</p>
}
```

Remove any references to `location` or `roomNumber`.

## 4. Room Service – Category Filter Fix

### 4.1 Issue

The `filteredGroups` computed reads `this.categoryFilter.value` directly, which is not reactive because `categoryFilter` is a `FormControl`. When the user selects a different category, the computed does not recalculate.  
**Solution:** Convert the form control’s value to a signal using `toSignal`.

### 4.2 Changes in `menu-grid.component.ts`

**Add import:**

```ts
import { toSignal } from "@angular/core/rxjs-interop";
```

**Inside the component class, add:**

```ts
private categoryFilterSignal = toSignal(this.categoryFilter.valueChanges, { initialValue: this.categoryFilter.value });
```

**Modify `filteredGroups` to use `this.categoryFilterSignal()` instead of `this.categoryFilter.value`:**

```ts
filteredGroups = computed(() => {
  const selected = this.categoryFilterSignal() ?? "All";
  const items =
    selected === "All"
      ? this.menuItems()
      : this.menuItems().filter((i) => (i.category || "Other") === selected);
  // grouping logic...
});
```

**Ensure that the `categories` computed uses the same grouping for empty categories:**

```ts
categories = computed(() => {
  const cats = new Set(this.menuItems().map((i) => i.category || "Other"));
  return Array.from(cats).sort();
});
```

**Template:** Already uses `filteredGroups()`, so no change needed. But verify that the `mat-select` options include an “Other” entry for items with no category. The `categories` signal already does this.

### 4.3 Template Verification

In the dropdown, the options should list all categories from the `categories()` signal. The default “All” option is always present.

## 5. Self‑Review Checklist

- [ ] Dashboard “Room Service Status” section now displays all pending and in‑progress housekeeping, maintenance, and food orders (for the active booking).
- [ ] Housekeeping/maintenance cards show description and status (not room number).
- [ ] Food orders card shows order ID and status.
- [ ] Category dropdown in Food Order works: selecting a specific category filters the displayed menu items; selecting “All” shows everything.
- [ ] No console errors; all subscriptions cleaned.

## 6. Integration Notes

- This patch modifies the dashboard’s data fetching logic to explicitly call APIs with separate status values, ensuring completeness.
- The category filter fix is a one‑line change (adding `toSignal`) but critical for reactivity.
- The `extractErrorMessage` helper should already exist in the dashboard component; if not, add it from previous specs.
- No changes to service signatures are required; we rely on existing `getAll` methods that accept a `status` string parameter. The status string is sent as a single query parameter; multiple calls are made for different statuses.
- The food orders merge may result in duplicate orders if an order appears in both “Pending” and “Preparing” responses – this is unlikely because an order status is singular.

