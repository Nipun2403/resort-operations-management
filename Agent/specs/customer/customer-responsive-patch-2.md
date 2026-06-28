# Patch Specsheet: Customer Dashboard Enhancements & Room Service Fixes

## 1. Purpose

- **Dashboard**:
  - Display room type names in the Upcoming Stay card.
  - Add a new “Room Service Status” section showing pending/in‑progress housekeeping, maintenance, and food orders for the current booking.
- **Room Service**:
  - Revert My Requests table to the full-column layout (removing the mobile‑only simplified table).
  - Confirm category filter works correctly in Food Order.
  - Confirm request service type toggle changes to a dropdown on mobile (already implemented).

## 2. Files to Modify

| File                                                                      | Change                                                                  |
| ------------------------------------------------------------------------- | ----------------------------------------------------------------------- |
| `src/app/features/user/pages/dashboard.component.ts`                      | Fetch room type names for upcoming booking; fetch room service status.  |
| `src/app/features/user/pages/dashboard.component.html`                    | Add room types to upcoming card; add new “Room Service Status” section. |
| `src/app/features/user/pages/dashboard.component.scss`                    | Styling for new section (responsive).                                   |
| `src/app/features/user/components/my-requests/my-requests.component.html` | Revert to single full-column table with horizontal scroll.              |
| `src/app/features/user/components/my-requests/my-requests.component.scss` | Ensure table scrolls on mobile.                                         |
| `src/app/features/user/components/food-order/menu-grid.component.ts`      | Ensure category filter works (minor fix if needed).                     |
| `src/app/features/user/components/food-order/menu-grid.component.html`    | Ensure category dropdown is correctly bound.                            |

## 3. Dashboard – Upcoming Stay Room Types

### 3.1 Component Logic

When the `upcomingBooking` is loaded, fetch the room type details for each `roomTypeId` in its rooms array.

**Add to `dashboard.component.ts`:**

- Inject `RoomTypeApiService`.
- Add a signal `upcomingRoomTypes = signal<string[]>([])` to hold room type names.
- Use `effect` to react when `upcomingBooking` changes. For each unique `roomTypeId`, call `GET /room-types/{id}`, collect names, and set the signal.

**Exact code:**

```ts
private roomTypeApi = inject(RoomTypeApiService);
upcomingRoomTypes = signal<string[]>([]);

private loadUpcomingRoomTypes(booking: Booking): void {
  if (!booking.rooms || booking.rooms.length === 0) {
    this.upcomingRoomTypes.set([]);
    return;
  }
  const ids = [...new Set(booking.rooms.map(r => r.roomTypeId))];
  const requests = ids.map(id =>
    this.roomTypeApi.getById(id).pipe(
      catchError(() => of(null)),
      map(rt => rt?.name ?? `Room Type ${id}`)
    )
  );
  forkJoin(requests).pipe(takeUntilDestroyed(this.destroyRef)).subscribe(names => {
    this.upcomingRoomTypes.set(names);
  });
}
```

Call `loadUpcomingRoomTypes` inside the `fetchBookings` method when `upcomingBooking` is set.

**In `fetchBookings`’s `next` callback:**

```ts
if (upcomingRes.data.length > 0) {
  this.upcomingBooking.set(upcomingRes.data[0]);
  this.loadUpcomingRoomTypes(upcomingRes.data[0]);
}
```

### 3.2 Template Update

In the upcoming booking card, add a line showing room types:

```html
@if (upcomingRoomTypes().length > 0) {
<p><strong>Room Type(s):</strong> {{ upcomingRoomTypes().join(', ') }}</p>
}
```

## 4. Dashboard – Room Service Status Section

### 4.1 New Data

We need to fetch housekeeping, maintenance, and food orders that are not completed.

- **Housekeeping**: `GET /housekeeping?status=Pending&status=InProgress` filtered by the user’s rooms (using `roomIds` computed from `activeBooking`). Since backend filters by user, we can use `roomId` query param. Or we can reuse the secure per‑room fetching like in My Requests but only for active booking. We'll use the approach from My Requests but with status filters.
- **Maintenance**: similarly.
- **Food orders**: `GET /orders?status=Pending&status=Preparing&bookingId=<currentBookingId>`

We'll create three signals: `pendingHousekeeping`, `pendingMaintenance`, `pendingFoodOrders`.  
Use `forkJoin` to fetch them after the active booking is loaded.

**Add to dashboard.component.ts:**

```ts
private housekeepingApi = inject(HousekeepingApiService);
private maintenanceApi = inject(MaintenanceApiService);
private orderApi = inject(OrderApiService); // need a service for orders

pendingHousekeeping = signal<CustomerRequest[]>([]);
pendingMaintenance = signal<CustomerRequest[]>([]);
pendingFoodOrders = signal<FoodOrderSummary[]>([]); // define interface { id, items: string, status }
```

When `activeBooking` changes, call a method `loadRoomServiceStatus()`.

**Implementation:**

```ts
private loadRoomServiceStatus(): void {
  const booking = this.activeBooking();
  if (!booking) return;
  const roomIds = booking.rooms.map(r => r.roomId).filter(id => id != null) as number[];

  // Housekeeping
  const hkReqs = roomIds.map(roomId =>
    this.housekeepingApi.getAll({ roomId, status: 'Pending,InProgress', pageSize: 10 }).pipe(
      map(res => res.data.map(hk => ({...hk, type: 'Housekeeping' as const})))
    )
  );
  // Maintenance
  const mtReqs = roomIds.map(roomId =>
    this.maintenanceApi.getAll({ roomId, status: 'Pending,InProgress', pageSize: 10 }).pipe(
      map(res => res.data.map(mt => ({...mt, type: 'Maintenance' as const})))
    )
  );
  // Food orders for the booking
  const food$ = this.orderApi.getAll({ bookingId: booking.id, status: 'Pending,Preparing', pageSize: 20 });

  forkJoin({
    hk: forkJoin(hkReqs).pipe(map(results => results.flat())),
    mt: forkJoin(mtReqs).pipe(map(results => results.flat())),
    food: food$
  }).pipe(takeUntilDestroyed(this.destroyRef)).subscribe(({ hk, mt, food }) => {
    this.pendingHousekeeping.set(hk);
    this.pendingMaintenance.set(mt);
    this.pendingFoodOrders.set(food.data);
  });
}
```

**Note:** The order endpoint may not support `bookingId` filter; if not, we may have to fetch all orders and filter client‑side. But since the user said the backend filters by user data, it's likely that `/orders` returns only the logged-in user's orders. We'll assume we can pass `bookingId` as query param. If not, we'll get all orders and filter by `bookingId` client‑side. For determinism, we'll query with `bookingId` parameter, and if the API doesn't support it, we can fallback to fetching all and filtering. I'll specify a safe approach: use `guestQuery` or directly use the user's email from JWT to filter orders. Actually, the order API has parameters: `status`, `roomId`, `pageNumber`, etc. No guest filter. So we must fetch all orders and filter by `bookingId` client‑side. That's acceptable for small data.

Simpler: Since we already have the active booking ID, we can call `GET /orders?status=Pending,Preparing&pageSize=200` and filter by `bookingId` in the subscription. That's a small client‑side filter, not a security leak because we only filter orders that belong to the current booking which we already know. I'll specify that.

**Food orders fetch:**

```ts
this.orderApi
  .getAll({ status: "Pending,Preparing", pageSize: 200 })
  .pipe(
    map((res) => res.data.filter((order) => order.bookingId === booking.id)),
  );
```

### 4.2 Template Addition

Add a new section below the booking cards:

```html
@if (activeBooking()) {
<div class="room-service-status">
  <h2>Room Service Status</h2>
  <div class="status-grid">
    <!-- Housekeeping -->
    <mat-card>
      <mat-card-header>
        <mat-card-title>Housekeeping</mat-card-title>
        <mat-card-subtitle
          >{{ pendingHousekeeping().length }} pending</mat-card-subtitle
        >
      </mat-card-header>
      <mat-card-content>
        @for (item of pendingHousekeeping(); track item.id) {
        <p>
          {{ item.location ?? 'Room '+item.roomId }} – {{ item.status }} – {{
          item.description }}
        </p>
        } @empty {
        <p>No pending requests.</p>
        }
      </mat-card-content>
    </mat-card>
    <!-- Maintenance -->
    ...
    <!-- Food Orders -->
    <mat-card>
      <mat-card-header>
        <mat-card-title>Food Orders</mat-card-title>
        <mat-card-subtitle
          >{{ pendingFoodOrders().length }} pending</mat-card-subtitle
        >
      </mat-card-header>
      <mat-card-content>
        @for (order of pendingFoodOrders(); track order.id) {
        <p>
          Order #{{ order.id }} – Status: {{ order.status }} – Items: {{
          getOrderItemsSummary(order) }}
        </p>
        } @empty {
        <p>No pending orders.</p>
        }
      </mat-card-content>
    </mat-card>
  </div>
</div>
}
```

**Define a helper** to summarize order items if the DTO contains items array. The order response might not include item details; we may need to fetch each order detail to display items. To keep it simple, we'll display order ID and status. For now, we won't show items.

### 4.3 Responsive Styling

Add to `dashboard.component.scss`:

```scss
.room-service-status {
  margin-top: 24px;
  .status-grid {
    display: flex;
    flex-wrap: wrap;
    gap: 16px;
    mat-card {
      flex: 1 1 300px;
    }
  }
}
@media (max-width: 599px) {
  .room-service-status .status-grid {
    flex-direction: column;
  }
}
```

## 5. Room Service Fixes

### 5.1 My Requests Table – Revert to Full Columns

Remove the mobile-only table structure added in the previous patch. Keep only one table that displays all columns (`type`, `room`, `description`, `status`, `createdAt`). Ensure horizontal scroll on mobile.

**Template (`my-requests.component.html`):**  
Keep a single `<table>` with all columns, no `@if (isMobile())` branching for tables. Remove the mobile-specific table. The table will naturally scroll if its container has `overflow-x: auto`.

Add a container div with class `table-container` and CSS:

```scss
.table-container {
  overflow-x: auto;
}
```

Place the table inside it.

### 5.2 Category Filter in Food Order

The category dropdown is already present. To ensure it works deterministically, verify that `filteredGroups` is used and that the `categories` signal is correctly derived. No code changes needed unless it's broken. I'll include a note that the `filteredGroups` computed should filter `menuItems()` by selected category before grouping. That logic is already correct as per previous spec. So no change, but I'll explicitly state that the agent must ensure the filter is functional.

### 5.3 Request Service Toggle to Dropdown on Mobile

Already implemented in previous patch. No changes needed. Confirm that the `isMobile` condition shows `<mat-select>` on mobile and `<mat-button-toggle-group>` on desktop.

## 6. Self‑Review Checklist

- [ ] Upcoming Stay card shows room type names (e.g., “Oceanfront Villa, Penthouse”).
- [ ] New “Room Service Status” section appears below booking cards when user has an active booking.
- [ ] Status section displays pending housekeeping, maintenance, and food orders with counts and brief details.
- [ ] My Requests table shows all columns on all screen sizes; horizontally scrolls on mobile.
- [ ] Category dropdown in food order filters items correctly.
- [ ] Request service type is a dropdown on mobile and a toggle on desktop.
- [ ] No console errors; all subscriptions cleaned.

## 7. Integration Notes

- The `RoomTypeApiService.getById` method must exist (add if not).
- For food orders, the `OrderApiService.getAll` must accept `status` and `pageSize` parameters; the returned DTO should include `id`, `bookingId`, `status`, and ideally items, but we only need status and ID for display. We'll assume the standard `Order` model from Swagger includes `items` array, but we can skip displaying items.
- The housekeeping and maintenance API calls must accept `roomId` as a query parameter; the backend supports it as per earlier user confirmation.
- The new “Room Service Status” section should only be visible when `activeBooking` exists; if no active booking, it's hidden (already covered by the outer `@if (activeBooking())`).
- No changes to the overall layout structure of the dashboard.
