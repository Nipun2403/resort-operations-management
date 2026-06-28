# Patch Specsheet: Customer Module – UX Improvements & Fixes

## 1. Purpose
- Apply five focused enhancements to the customer portal without introducing regressions.
- Cart: enable direct quantity adjustment (+/-) inside the cart drawer.
- Mobile UX: improve Request Service and My Requests layout on small screens.
- Booking Details: fetch and display room type names in the detail modal.
- Bookings Toggle: swap “New Booking” to first position, fix pill styling.
- Food Order: group menu items by category.

## 2. CartDrawerComponent – Quantity Controls

### Files to Modify
- `src/app/features/user/components/food-order/cart-drawer.component.ts`
- `src/app/features/user/components/food-order/cart-drawer.component.html`

### Changes

**Template** – add +/- buttons next to each cart item.

Inside the `@for` loop that renders cart items, replace the static display with:
```html
@for (item of cartItems(); track item.menuItemId) {
  <div class="cart-item">
    <span class="item-name">{{ item.name }}</span>
    <div class="qty-controls">
      <button type="button" mat-icon-button (click)="decrementQty(item.menuItemId)">
        <mat-icon>remove</mat-icon>
      </button>
      <span class="qty">{{ item.quantity }}</span>
      <button type="button" mat-icon-button (click)="incrementQty(item.menuItemId)">
        <mat-icon>add</mat-icon>
      </button>
    </div>
    <span class="item-price">{{ item.price * item.quantity | currency }}</span>
  </div>
}
```

**Component class** – add new output `updateQuantity` and methods to handle changes.

Add output:
```ts
updateQuantity = output<{ menuItemId: number; delta: number }>();
```

Add methods:
```ts
incrementQty(menuItemId: number) {
  this.updateQuantity.emit({ menuItemId, delta: 1 });
}
decrementQty(menuItemId: number) {
  this.updateQuantity.emit({ menuItemId, delta: -1 });
}
```

**In `FoodOrderComponent`** – listen to `updateQuantity` and modify the `cartItems` signal accordingly.

Add an event handler:
```ts
onUpdateCartQty(event: { menuItemId: number; delta: number }) {
  this.cartItems.update(items => {
    const index = items.findIndex(i => i.menuItemId === event.menuItemId);
    if (index === -1) return items;
    const newQty = items[index].quantity + event.delta;
    if (newQty <= 0) {
      // Remove item
      return items.filter(i => i.menuItemId !== event.menuItemId);
    }
    return items.map(i => i.menuItemId === event.menuItemId ? { ...i, quantity: newQty } : i);
  });
}
```

Bind in template of `FoodOrderComponent`:
```html
<app-cart-drawer 
  ...
  (updateQuantity)="onUpdateCartQty($event)" />
```

## 3. Request Service & My Requests – Mobile Friendly

### Files to Modify
- `src/app/features/user/components/request-service/request-service.component.scss`
- `src/app/features/user/components/my-requests/my-requests.component.scss`

### Changes

**Request Service** – adjust card padding and form field widths for mobile:

```scss
@media (max-width: 599px) {
  .request-service {
    mat-card {
      margin: 8px;
      padding: 12px;
    }
    mat-form-field {
      width: 100%;
    }
    mat-button-toggle-group {
      width: 100%;
      display: flex;
      mat-button-toggle {
        flex: 1 1 50%;
      }
    }
  }
}
```

**My Requests** – ensure table scrolls horizontally and buttons are touch‑friendly:

```scss
@media (max-width: 599px) {
  .my-requests {
    overflow-x: auto;
    table {
      min-width: 600px;
    }
    .mat-mdc-cell, .mat-mdc-header-cell {
      padding: 8px 4px;
      font-size: 0.85rem;
    }
  }
}
```

Add a general container padding and make the “No requests” message centered and legible.

## 4. Booking Detail Modal – Show Room Type Names

### Files to Modify
- `src/app/features/user/components/booking-detail-dialog/booking-detail-dialog.component.ts`
- `src/app/features/user/components/booking-detail-dialog/booking-detail-dialog.component.html`
- `src/app/features/user/components/booking-detail-dialog/booking-detail-dialog.component.scss` (if needed)

### Approach
- Inject `RoomTypeApiService` into the dialog.
- On init, for each room in the booking, fetch its room type using `GET /room-types/{room.roomTypeId}`.
- Display the room type name instead of (or alongside) the room number.

**Template** – update the rooms list:

```html
@for (room of enrichedRooms(); track room.bookingRoomId) {
  <div class="room-item">
    <p><strong>Room:</strong> {{ room.roomNumber ?? 'Unassigned' }}</p>
    <p><strong>Type:</strong> {{ room.roomTypeName }}</p>
    <p><strong>Price:</strong> {{ room.lockedInPrice | currency }}</p>
  </div>
}
```

**Component logic** – add enriched rooms signal:

```ts
import { RoomTypeApiService } from '../../../../services/room-type-api.service'; // adjust path

private roomTypeApi = inject(RoomTypeApiService);

enrichedRooms = signal<(BookingRoom & { roomTypeName: string })[]>([]);

ngOnInit(): void {
  // after getting booking data via MAT_DIALOG_DATA
  this.enrichRooms();
}

private enrichRooms(): void {
  const rooms = this.data.rooms ?? [];
  if (rooms.length === 0) return;
  const requests = rooms.map(room => 
    this.roomTypeApi.getById(room.roomTypeId).pipe(
      map(roomType => ({
        ...room,
        bookingRoomId: room.id, // original room id
        roomTypeName: roomType?.name ?? `Room Type ${room.roomTypeId}`
      })),
      catchError(() => of({
        ...room,
        bookingRoomId: room.id,
        roomTypeName: `Room Type ${room.roomTypeId}`
      }))
    )
  );
  forkJoin(requests).pipe(takeUntilDestroyed(this.destroyRef)).subscribe(enriched => {
    this.enrichedRooms.set(enriched);
  });
}
```

**Note**: The dialog must inject `DestroyRef` and `takeUntilDestroyed`. The `RoomTypeApiService` needs to expose a `getById(id: number): Observable<RoomType>` method if not already present; if it doesn't, add it (it's a simple call to the existing endpoint). Since we already have a `RoomTypeApiService` for admin, we can reuse it.

**Important**: The dialog must not block rendering while fetching room types; the enriched list can update asynchronously. The template will automatically update when the signal changes.

## 5. Bookings Toggle – Swap Order & Fix Pill

### Files to Modify
- `src/app/features/user/pages/bookings.component.ts`
- `src/app/features/user/pages/bookings.component.scss`

### Changes

**Toggle default value** – change initial value to `'new'` so that “New Booking” is selected first.

```ts
viewMode = new FormControl<'history' | 'new'>('new', { nonNullable: true });
```

**Template** – swap the order of `mat-button-toggle` elements:

```html
<mat-button-toggle-group [formControl]="viewMode" aria-label="View">
  <mat-button-toggle value="new">New Booking</mat-button-toggle>
  <mat-button-toggle value="history">My Bookings</mat-button-toggle>
</mat-button-toggle-group>
```

**Pill styling fix** – if the button toggle group is not visually pill‑shaped or has alignment issues, add/update CSS:

```scss
mat-button-toggle-group {
  border-radius: 24px;
  overflow: hidden;
  .mat-button-toggle {
    border-radius: 24px;
  }
  .mat-button-toggle-checked {
    background-color: #1976d2; // primary color
    color: white;
  }
}
```

Add the above to `bookings.component.scss`. If the class names differ (e.g., `mat-mdc-button-toggle`), adjust accordingly.

## 6. Food Order – Group Items by Category

### Files to Modify
- `src/app/features/user/components/food-order/menu-grid.component.ts`
- `src/app/features/user/components/food-order/menu-grid.component.html`
- `src/app/features/user/components/food-order/menu-grid.component.scss`

### Changes

**Service/model** – ensure `MenuItem` includes `category: string`. Already present in the DTO (from Swagger: `category`). If not, add it.

**Component logic** – group menu items by category:

```ts
import { computed } from '@angular/core';

private menuItems = input.required<MenuItem[]>();

groupedMenu = computed(() => {
  const groups: Record<string, MenuItem[]> = {};
  for (const item of this.menuItems()) {
    const cat = item.category || 'Other';
    if (!groups[cat]) groups[cat] = [];
    groups[cat].push(item);
  }
  return Object.entries(groups).map(([category, items]) => ({ category, items }));
});
```

**Template** – iterate over `groupedMenu()` and render category headings:

```html
@for (group of groupedMenu(); track group.category) {
  <h3 class="category-title">{{ group.category }}</h3>
  <div class="menu-items">
    @for (item of group.items; track item.id) {
      <mat-card class="menu-card">
        <mat-card-header>
          <mat-card-title>{{ item.name }}</mat-card-title>
          <mat-card-subtitle>{{ item.price | currency }}</mat-card-subtitle>
        </mat-card-header>
        <mat-card-actions>
          <button mat-raised-button (click)="addToCart.emit(item)">Add</button>
        </mat-card-actions>
      </mat-card>
    }
  </div>
}
```

**CSS** – style categories:

```scss
.category-title {
  margin: 16px 0 8px;
  padding-bottom: 4px;
  border-bottom: 2px solid #ddd;
}
.menu-items {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(240px, 1fr));
  gap: 16px;
}
```

**Responsive** – on mobile, the grid should collapse to one column automatically (via `auto-fill` with minmax). Already handled by the above CSS.

## 7. Self‑Review Checklist (for the agent)

- [ ] Cart items can be increased/decreased via +/- buttons in the cart drawer; removing an item when quantity reaches zero works correctly.
- [ ] Request Service and My Requests sections look clean on mobile (<600px) with full‑width form fields, scrollable tables, and adequate padding.
- [ ] Booking detail modal displays room type names (fetched from API) alongside room numbers.
- [ ] The bookings toggle has "New Booking" first and "My Bookings" second; the pill styling is rounded and clearly indicates the active selection.
- [ ] The menu grid groups food items under category headers, using the `category` field from the API.
- [ ] No console errors; all new subscriptions use `takeUntilDestroyed` where needed.
- [ ] All changes are isolated and do not break existing functionality.

## 8. Integration Notes
- For the `RoomTypeApiService.getById` method, if it doesn't exist, add it to the existing service using the endpoint `GET /api/v1/room-types/{id}` (already defined in Swagger). This is a simple addition.
- The `MenuItem` model must include `category: string`. It already exists in the Swagger DTOs.
- The cart quantity controls only emit an event; they do not directly modify cart state, preserving the existing unidirectional data flow.
- The bookings toggle swap may affect any state that depends on the default view; after this patch, the page will open on "New Booking". Ensure session storage keys remain the same so that if a user previously had "history" saved, it will be overridden on next save; that's acceptable.