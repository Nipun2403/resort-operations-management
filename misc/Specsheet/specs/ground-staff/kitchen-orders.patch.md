# Patch Specsheet: Backend API Update – Food Orders Room Tracking

## 1. Purpose
- Update all frontend components that create or display food orders to comply with the new backend API:
  - `POST /api/v1/orders` now **requires** `roomId` in the request body.
  - `GET /api/v1/orders` and `GET /api/v1/orders/{id}` now return `roomNumber` (string or null) and use consistent field names (`orderStatus`, `orderItems`).
- Add a room selector dropdown to every food order placement form.
- Display `roomNumber` wherever orders are shown; fallback gracefully for null/legacy orders.
- Handle the new 400 error responses with user-friendly messages.

## 2. Backend Response Field Map (New → Old)
| New Field | Old Field (if different) | Notes |
|-----------|--------------------------|-------|
| `orderStatus` | `foodOrderStatus` or `status` | Use `orderStatus` everywhere. |
| `orderItems` | `items` or `foodOrderItems` | Array of `{ menuItemId, menuItemName, quantity, priceAtPurchase }`. |
| `roomNumber` | *(new)* | String or null. Display `"N/A"` if null. |
| `roomId` | `roomId` | Already existed. |
| `generatedAt` | `bookedAt` or `generatedAt` | Use `generatedAt`. |

## 3. Files to Modify

| # | File | Change |
|---|------|--------|
| 1 | `src/app/features/user/components/food-order/food-order.component.ts` | Add `rooms` input, room selector, update DTO. |
| 2 | `src/app/features/user/components/food-order/food-order.component.html` | Add room dropdown. |
| 3 | `src/app/features/front-desk/components/booking-action-modal/food-order-panel/food-order-panel.component.ts` | Add `rooms` input, room selector, update DTO. |
| 4 | `src/app/features/front-desk/components/booking-action-modal/food-order-panel/food-order-panel.component.html` | Add room dropdown. |
| 5 | `src/app/features/user/pages/room-service.component.ts` | Pass rooms to `FoodOrderComponent`. |
| 6 | `src/app/features/user/pages/room-service.component.html` | Update binding. |
| 7 | `src/app/features/front-desk/components/booking-action-modal/room-service-tab/room-service-tab.component.ts` | Pass rooms to `FoodOrderPanelComponent`. |
| 8 | `src/app/features/front-desk/components/booking-action-modal/room-service-tab/room-service-tab.component.html` | Update binding. |
| 9 | `src/app/features/kitchen/pages/dashboard.component.ts` | Update field names in `fetchTasks` and `getDetailSections`. |
| 10 | `src/app/features/front-desk/components/ticket-list/ticket-list.component.ts` | Update food order field names. |
| 11 | `src/app/features/user/pages/dashboard.component.ts` | Update food order status field name. |
| 12 | `src/app/features/user/components/my-requests/my-requests.component.ts` | Update food order field names. |
| 13 | `src/app/features/front-desk/pages/dashboard.component.ts` | Update active tickets food order fetch field names. |
| 14 | `src/app/shared/models/order.model.ts` (or wherever `CreateFoodOrderDTO` is defined) | Add `roomId` field. |

## 4. Detailed Changes

### 4.1 DTO Update – `CreateFoodOrderDTO`
Add `roomId: number` as required.

```typescript
export interface CreateFoodOrderDTO {
  bookingId: number;
  roomId: number;          // NEW – required
  items: { menuItemId: number; quantity: number }[];
}
```

### 4.2 Customer Food Order – `FoodOrderComponent`

**File:** `src/app/features/user/components/food-order/food-order.component.ts`

**Input changes:** Add `rooms = input.required<BookingRoom[]>()` alongside the existing `activeBookingId`.

**State additions:**
```typescript
selectedRoomId = new FormControl<number>(0, { nonNullable: true, validators: Validators.required });
```

In `ngOnInit`, set the default to the first room's `roomId`.

**Template addition** (above the menu grid):
```html
<mat-form-field appearance="outline">
  <mat-label>Deliver to Room</mat-label>
  <mat-select [formControl]="selectedRoomId">
    @for (room of rooms(); track room.roomId) {
      <mat-option [value]="room.roomId">
        {{ room.roomNumber ?? 'Room ' + room.roomId }}
      </mat-option>
    }
  </mat-select>
  @if (selectedRoomId.invalid && selectedRoomId.touched) {
    <mat-error>Please select a room for delivery.</mat-error>
  }
</mat-form-field>
```

**Update `submitOrder()`:**
```typescript
private submitOrder(): void {
  if (this.selectedRoomId.invalid) return;
  const dto: CreateFoodOrderDTO = {
    bookingId: this.activeBookingId(),
    roomId: this.selectedRoomId.value,
    items: this.cartItems().map(i => ({ menuItemId: i.menuItemId, quantity: i.quantity })),
  };
  this.orderApi.create(dto).pipe(...).subscribe(...);
}
```

**Error handling:** Catch 400 errors and display `err.error.message` in snackbar.

### 4.3 Customer Room Service Page – Pass Rooms

**File:** `src/app/features/user/pages/room-service.component.html`

Update the Food Order tab:
```html
<app-food-order [activeBookingId]="booking.id" [rooms]="booking.rooms" (orderPlaced)="onOrderPlaced()" />
```

### 4.4 Front Desk Food Order Panel – `FoodOrderPanelComponent`

**File:** `src/app/features/front-desk/components/booking-action-modal/food-order-panel/food-order-panel.component.ts`

**Input changes:** Add `rooms = input.required<BookingRoom[]>()` alongside existing `bookingId`.

**Add `selectedRoomId` FormControl** (same as customer, default to first room).

**Template addition** (same room dropdown as customer).

**Update `placeOrder` / `submitOrder`:** Add `roomId: this.selectedRoomId.value` to the DTO.

**Error handling:** Same as customer – display `err.error.message`.

### 4.5 Front Desk Room Service Tab – Pass Rooms

**File:** `src/app/features/front-desk/components/booking-action-modal/room-service-tab/room-service-tab.component.html`

Update:
```html
<app-food-order-panel [bookingId]="booking().id" [rooms]="booking().rooms" />
```

### 4.6 Kitchen Dashboard – Field Name Updates

**File:** `src/app/features/kitchen/pages/dashboard.component.ts`

**Update `fetchTasks`:**
```typescript
fetchTasks: (params) => this.orderApi.getAll(params).pipe(
  map(res => ({
    totalCount: res.totalCount,
    data: res.data.map(order => ({
      id: order.id,
      status: order.orderStatus ?? 'Pending',
      location: order.roomNumber ?? (order.roomId ? `Room ${order.roomId}` : 'N/A'),
      description: `Order #${order.id}`,
      createdAt: order.generatedAt ?? '',
      raw: order,
    } as Task))
  }))
),
```

**Update `getDetailSections`:**
```typescript
getDetailSections: (t) => {
  const order = t.raw;
  const itemsArray = order.orderItems || [];
  const items = itemsArray.length > 0
    ? itemsArray.map((i: any) => `${i.quantity}x ${i.menuItemName ?? 'Item #' + i.menuItemId}`).join(', ')
    : 'None';
  return [
    { title: 'Order Information', fields: [
      { label: 'Order ID', value: String(order.id) },
      { label: 'Status', value: t.status },
      { label: 'Room', value: t.location },
      { label: 'Items', value: items },
      { label: 'Created At', value: t.createdAt ? new Date(t.createdAt).toLocaleString() : 'N/A' },
    ]},
  ] as DetailSection[];
},
```

**Status options:** Ensure the status filter uses `orderStatus` values. Since the backend sends `orderStatus`, the `updateStatus` method must also send the correct status string back. The `updateTaskStatus` function currently sends `{ status: newStatus }`. The backend expects the body for `PATCH /orders/{id}` to have `{ status: newStatus }` per the Swagger `UpdateOrderStatusDTO`. The enum values are `Pending`, `Preparing`, `Delivered`. So status transitions work correctly.

### 4.7 Front Desk Ticket List – Field Name Updates

**File:** `src/app/features/front-desk/components/ticket-list/ticket-list.component.ts`

In the food order branch of `fetch()`:
- Use `order.orderStatus` instead of `order.status`.
- Use `order.roomNumber ?? (order.roomId ? 'Room ' + order.roomId : 'N/A')` for room display.
- Use `order.orderItems` instead of `order.items`.
- Use `order.generatedAt` instead of `order.bookedAt` or `order.generatedAt`.

### 4.8 Customer Dashboard – Food Order Status Field

**File:** `src/app/features/user/pages/dashboard.component.ts`

In the `loadRoomServiceStatus` method, where food orders are fetched:
- The response data array elements now have `orderStatus` instead of `status`. Update the mapping to use `o.orderStatus`.

In the template, the food orders card displays `{{ order.status }}`. Update to `{{ order.orderStatus }}`.

### 4.9 Customer My Requests – Food Order Field Names

**File:** `src/app/features/user/components/my-requests/my-requests.component.ts`

In the food order mapping (inside `fetchRequests`):
- Use `order.orderStatus` instead of `order.status`.
- Use `order.roomNumber ?? 'N/A'` for room display.
- Use `order.generatedAt` for `createdAt`.

### 4.10 Front Desk Dashboard – Active Tickets Food Orders

**File:** `src/app/features/front-desk/pages/dashboard.component.ts`

In `loadSummary`, where food orders are fetched for the active tickets card:
- Use `res.data` elements with `orderStatus` property. Update the status chip and mapping.
- The counts come from `totalCount`, which is unaffected.

In the `ActiveTicketsDialogComponent` / `TicketListComponent` (already addressed in 4.7).

## 5. Error Handling (All Food Order Submissions)

In every place that calls `POST /api/v1/orders`, ensure the error callback extracts the message correctly:

```typescript
error: (err: HttpErrorResponse) => {
  const message = typeof err.error === 'string' ? err.error : (err.error?.message || 'Order failed');
  this.snackBar.open(message, 'Close', { duration: 5000 });
}
```

This handles:
- `"The RoomId field is required."`
- `"The specified room does not belong to this booking."`
- `"Food orders can only be placed for guests currently checked in."`
- `"Cannot add food orders to a booking that has already been paid."`

## 6. Display Fallback for Legacy Orders

Anywhere `roomNumber` is displayed, use a fallback:
```html
{{ order.roomNumber ?? 'N/A' }}
```
or in TypeScript:
```typescript
location: order.roomNumber ?? (order.roomId ? `Room ${order.roomId}` : 'N/A')
```

## 7. Self‑Review Checklist

- [ ] Customer food order form shows a room selector dropdown populated from the booking's rooms.
- [ ] Front desk food order form shows a room selector.
- [ ] Submitting an order without selecting a room shows a validation error.
- [ ] Order creation sends `roomId` in the DTO and succeeds.
- [ ] Backend error messages (missing room, invalid room, not checked in, already paid) are displayed in snackbar.
- [ ] Kitchen dashboard shows `roomNumber` in the location column and detail modal.
- [ ] Kitchen dashboard shows correct item names from `orderItems`.
- [ ] Kitchen dashboard shows `orderStatus` with correct chip colors.
- [ ] Customer dashboard pending food orders show correct status.
- [ ] Customer My Requests shows food orders with room number and status.
- [ ] Front desk active tickets show food orders with room number.
- [ ] Legacy orders with null `roomNumber` display "N/A".
- [ ] No console errors; all subscriptions cleaned.

## 8. Integration Notes
- The `CreateFoodOrderDTO` interface update affects all callers of the order create method. Ensure the method signature in `OrderApiService.create()` accepts the updated DTO.
- The `OrderApiService.getAll()` return type should be updated to reflect the new field names in the response (or use a generic mapping that doesn't depend on specific field names). Since we map fields explicitly in each component, the service can remain unchanged; only the consuming components adapt.
- No other APIs or pages are affected.