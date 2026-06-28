# Patch Specsheet: Room Service – Add to Cart UX, Confirmations, Category Filter & Mobile Optimizations

## 1. Purpose
- Enhance Food Order UX: the “Add to Cart” button transforms into a +/- quantity selector after the first addition.
- Add confirmation dialogs before placing a food order, submitting a service request, and creating a new booking (in the Bookings wizard).
- Introduce a category dropdown filter in the menu grid, defaulting to “All”.
- Optimize mobile views for 320px width screens; simplify My Requests table to three columns with a detail modal; convert service type toggle to a dropdown on mobile.

## 2. Files to Modify

| File | Change |
|------|--------|
| `src/app/features/user/components/food-order/menu-grid.component.ts` | Add internal cart state and quantity selector logic. |
| `src/app/features/user/components/food-order/menu-grid.component.html` | Toggle between “Add” button and +/- controls per item. |
| `src/app/features/user/components/food-order/food-order.component.ts` | Add confirmation dialog before placing order. |
| `src/app/features/user/components/food-order/food-order.component.html` | No changes needed. |
| `src/app/features/user/components/request-service/request-service.component.ts` | Add confirmation dialog before submit; mobile dropdown logic. |
| `src/app/features/user/components/request-service/request-service.component.html` | Show dropdown on mobile, toggle on desktop. |
| `src/app/features/user/components/my-requests/my-requests.component.ts` | Add detail modal component and row click handler. |
| `src/app/features/user/components/my-requests/my-requests.component.html` | Simplify table columns on mobile; open modal on row click. |
| `src/app/features/user/components/my-requests/my-requests.component.scss` | Mobile styling. |
| `src/app/features/user/components/booking-wizard/booking-wizard.component.ts` | Add confirmation dialog before final submit. |
| `src/app/features/user/components/booking-wizard/booking-wizard.component.html` | No changes needed. |
| `src/app/features/user/components/food-order/menu-grid.component.scss` | Responsive adjustments. |
| `src/styles.scss` (or global) | Add 320px breakpoint safety rules. |

## 3. Menu Grid – Inline Quantity Selector

### 3.1 Component State
Add an internal `cartMap = signal<Record<number, number>>({})` to track the quantity of each item added. This avoids coupling with the parent cart; the grid will emit `addToCart` on first addition and `updateQuantity` on subsequent changes. The parent (FoodOrderComponent) already handles `updateQuantity` from the cart, but now the grid will also emit it directly.

**Change in `menu-grid.component.ts`:**

```ts
import { output, input, signal, computed } from '@angular/core';

// Existing:
menuItems = input.required<MenuItem[]>();
addToCart = output<MenuItem>();

// New:
updateQuantity = output<{ menuItemId: number; delta: number }>();

cartMap = signal<Record<number, number>>({});

getQuantity(menuItemId: number): number {
  return this.cartMap()[menuItemId] || 0;
}

increment(item: MenuItem): void {
  const current = this.cartMap()[item.id] || 0;
  if (current === 0) {
    // First addition: emit addToCart to parent to add item to cart
    this.addToCart.emit(item);
  } else {
    this.updateQuantity.emit({ menuItemId: item.id, delta: 1 });
  }
  this.cartMap.update(m => ({ ...m, [item.id]: (m[item.id] || 0) + 1 }));
}

decrement(item: MenuItem): void {
  const current = this.cartMap()[item.id] || 0;
  if (current > 0) {
    this.updateQuantity.emit({ menuItemId: item.id, delta: -1 });
    this.cartMap.update(m => {
      const newQty = (m[item.id] || 0) - 1;
      if (newQty <= 0) {
        const { [item.id]: _, ...rest } = m;
        return rest;
      }
      return { ...m, [item.id]: newQty };
    });
  }
}
```

### 3.2 Template
For each menu item card, replace the static “Add” button with:

```html
@if (getQuantity(item.id) === 0) {
  <button mat-raised-button (click)="increment(item)">Add to Cart</button>
} @else {
  <div class="qty-controls">
    <button type="button" mat-icon-button (click)="decrement(item)"><mat-icon>remove</mat-icon></button>
    <span>{{ getQuantity(item.id) }}</span>
    <button type="button" mat-icon-button (click)="increment(item)"><mat-icon>add</mat-icon></button>
  </div>
}
```

## 4. Confirmation Dialogs

### 4.1 Food Order – Place Order
In `FoodOrderComponent`, the `placeOrder()` method currently calls the API directly. Wrap the API call inside a confirmation dialog using the shared `ConfirmDialogComponent`.

**Change in `food-order.component.ts`:**

```ts
private dialog = inject(MatDialog);

placeOrder(): void {
  if (!this.canCheckout()) return;
  const dialogRef = this.dialog.open(ConfirmDialogComponent, {
    data: {
      title: 'Confirm Order',
      message: `Place this order for ${this.cartItems().length} item(s)? Total: ${this.subtotal() | currency}`,
    },
  });
  dialogRef.afterClosed().pipe(takeUntilDestroyed(this.destroyRef)).subscribe(confirmed => {
    if (confirmed) {
      this.submitOrder();
    }
  });
}

private submitOrder(): void {
  // existing API call logic, emits orderPlaced on success
}
```

### 4.2 Request Service – Submit
Similarly, in `RequestServiceComponent`, wrap the submit logic:

```ts
private dialog = inject(MatDialog);

submitRequest(): void {
  if (this.description.invalid || this.submitting()) return;
  const room = this.activeBooking().rooms.find(r => r.roomId === this.selectedRoomId.value);
  const roomLabel = room?.roomNumber ?? 'selected room';
  const dialogRef = this.dialog.open(ConfirmDialogComponent, {
    data: {
      title: 'Confirm Service Request',
      message: `Send a ${this.requestType.value} request for ${roomLabel}?`,
    },
  });
  dialogRef.afterClosed().pipe(takeUntilDestroyed(this.destroyRef)).subscribe(confirmed => {
    if (confirmed) {
      this.performSubmit();
    }
  });
}

private performSubmit(): void {
  // existing API call logic, emits requestCreated on success
}
```

### 4.3 Bookings Wizard – Confirm Booking
In `BookingWizardComponent`, before calling `POST /bookings` in `submitBooking()`, open a confirmation.

```ts
submitBooking(): void {
  const dialogRef = this.dialog.open(ConfirmDialogComponent, {
    data: {
      title: 'Confirm Booking',
      message: `Create this booking? Total estimated: ${this.estimatedTotal() | currency}`,
    },
  });
  dialogRef.afterClosed().pipe(takeUntilDestroyed(this.destroyRef)).subscribe(confirmed => {
    if (confirmed) {
      this.performBooking();
    }
  });
}

private performBooking(): void {
  // existing create booking logic, emits bookingCreated on success
}
```

All dialogs import `ConfirmDialogComponent` from `'src/app/shared/components/confirm-dialog/confirm-dialog.component'`. Ensure it's added to the component's `imports` array.

## 5. Category Dropdown Filter

### 5.1 Menu Grid Changes
Add a `categoryFilter` control and computed filtered groups.

**In `menu-grid.component.ts`:**

```ts
import { FormControl } from '@angular/forms';

categoryFilter = new FormControl('All', { nonNullable: true });

categories = computed(() => {
  const cats = new Set(this.menuItems().map(i => i.category || 'Other'));
  return Array.from(cats).sort();
});

filteredGroups = computed(() => {
  const selected = this.categoryFilter.value;
  const items = selected === 'All' 
    ? this.menuItems() 
    : this.menuItems().filter(i => (i.category || 'Other') === selected);
  // Group the filtered items
  const groups: Record<string, MenuItem[]> = {};
  for (const item of items) {
    const cat = item.category || 'Other';
    if (!groups[cat]) groups[cat] = [];
    groups[cat].push(item);
  }
  return Object.entries(groups).map(([category, items]) => ({ category, items }));
});
```

**In the template**, add a `mat-form-field` with `mat-select` above the menu items:

```html
<mat-form-field appearance="outline">
  <mat-label>Category</mat-label>
  <mat-select [formControl]="categoryFilter">
    <mat-option value="All">All</mat-option>
    @for (cat of categories(); track cat) {
      <mat-option [value]="cat">{{ cat }}</mat-option>
    }
  </mat-select>
</mat-form-field>
```

Then iterate over `filteredGroups()` instead of `groupedMenu()`.

## 6. Mobile Optimizations (320px and specific components)

### 6.1 Global 320px Safeguard
Add to `styles.scss`:

```scss
@media (max-width: 360px) {
  body {
    font-size: 14px;
  }
  mat-card {
    margin: 4px;
    padding: 8px;
  }
}
```

### 6.2 My Requests Table – Simplified Columns + Detail Modal
**Mobile view (≤599px):** hide all columns except `type`, `status`, and `createdAt`. The row becomes clickable, opening a modal with full details.

**Template changes:**
```html
<table mat-table [dataSource]="requests()" matSort matSortDisableClear>
  <ng-container matColumnDef="type" *ngIf="...">... existing ...</ng-container>
  <!-- Mobile visible columns -->
  <ng-container matColumnDef="mobileStatus">
    <th mat-header-cell *matHeaderCellDef>Status</th>
    <td mat-cell *matCellDef="let r">{{ r.status }}</td>
  </ng-container>
  <ng-container matColumnDef="mobileCreatedAt">
    <th mat-header-cell *matHeaderCellDef>Created</th>
    <td mat-cell *matCellDef="let r">{{ r.createdAt | date:'short' }}</td>
  </ng-container>
  <!-- Use CSS to show/hide columns based on media query via display:none. But better to conditionally render using isMobile signal and two different table structures. Since we already have isMobile available from BreakpointObserver, we can use @if to render two different tables. -->
</table>
```

Simpler: add an `isMobile` signal to the component (inject `BreakpointObserver` with `max-width: 599px`). Then:

```html
@if (isMobile()) {
  <!-- Mobile table: type, status, createdAt only -->
  <table mat-table [dataSource]="requests()" matSort matSortDisableClear class="mobile-table">
    <ng-container matColumnDef="type">...</ng-container>
    <ng-container matColumnDef="status">...</ng-container>
    <ng-container matColumnDef="createdAt">...</ng-container>
    <tr mat-header-row *matHeaderRowDef="['type','status','createdAt']"></tr>
    <tr mat-row *matRowDef="let row; columns: ['type','status','createdAt']" (click)="openDetail(row)"></tr>
  </table>
} @else {
  <!-- Desktop full table -->
}
```

Add a method `openDetail(request: CustomerRequest)` that opens a new `RequestDetailDialogComponent`. We'll create that component quickly (standalone) receiving the request object via `MAT_DIALOG_DATA` and displaying all fields.

**RequestDetailDialogComponent** (new file):
- Template: simple card with all request properties.
- Imports: `MatDialogModule`, `MatButtonModule`, `CommonModule`, `MatDividerModule`.
- Provided in the same component file or separate.

### 6.3 Request Service – Toggle to Dropdown on Mobile
Add `isMobile` signal to `RequestServiceComponent` (using BreakpointObserver). In the template:

```html
@if (isMobile()) {
  <mat-form-field appearance="outline">
    <mat-label>Service Type</mat-label>
    <mat-select [formControl]="requestType">
      <mat-option value="housekeeping">Housekeeping</mat-option>
      <mat-option value="maintenance">Maintenance</mat-option>
    </mat-select>
  </mat-form-field>
} @else {
  <mat-button-toggle-group [formControl]="requestType" aria-label="Service type">
    <mat-button-toggle value="housekeeping"><mat-icon>cleaning_services</mat-icon> Housekeeping</mat-button-toggle>
    <mat-button-toggle value="maintenance"><mat-icon>build</mat-icon> Maintenance</mat-button-toggle>
  </mat-button-toggle-group>
}
```

The `requestType` form control remains the same; its value is used for submission.

## 7. Self‑Review Checklist (for the agent)
- [ ] In the menu grid, clicking “Add to Cart” changes the button to a +/- quantity selector; subsequent clicks adjust quantity directly.
- [ ] Placing a food order shows a confirmation dialog with total amount; only proceeding after confirm.
- [ ] Submitting a service request shows a confirmation dialog; only after confirm.
- [ ] Creating a new booking shows a confirmation dialog before API call.
- [ ] The category dropdown filters the menu items correctly; selecting “All” shows everything.
- [ ] On mobile (≤599px), My Requests table shows only type, status, createdAt; clicking a row opens a detail modal with all info.
- [ ] On mobile, the request service type is a dropdown instead of button toggle group.
- [ ] Layouts are stable down to 320px width.
- [ ] No console errors; all imports correctly added.

## 8. Integration Notes
- The `ConfirmDialogComponent` is already in shared; ensure it's imported in all components that need it.
- The `isMobile` signals should use `BreakpointObserver` observing `'(max-width: 599px)'` as done in other components.
- The `RequestDetailDialogComponent` is a new, simple standalone component. Place it in `src/app/features/user/components/my-requests/request-detail-dialog.component.ts`.
- The quantity selector in menu grid does not persist across category filter changes? The `cartMap` is local to the grid component, so it will reset when the component is destroyed/recreated? The grid is always rendered inside `FoodOrderComponent` and not destroyed unless the tab changes. That's acceptable; if the user switches tabs, the state might reset, but it's okay for now. The parent `FoodOrderComponent` holds the actual cart, but the grid's `cartMap` is just for display of quantity in the button. We may want to sync with the parent's cart on init, but it's simpler to let it be independent; the user can still adjust quantities from the cart drawer anyway. So no issue.# Patch Specsheet: Room Service – Add to Cart UX, Confirmations, Category Filter & Mobile Optimizations

## 1. Purpose
- Enhance Food Order UX: the “Add to Cart” button transforms into a +/- quantity selector after the first addition.
- Add confirmation dialogs before placing a food order, submitting a service request, and creating a new booking (in the Bookings wizard).
- Introduce a category dropdown filter in the menu grid, defaulting to “All”.
- Optimize mobile views for 320px width screens; simplify My Requests table to three columns with a detail modal; convert service type toggle to a dropdown on mobile.

## 2. Files to Modify

| File | Change |
|------|--------|
| `src/app/features/user/components/food-order/menu-grid.component.ts` | Add internal cart state and quantity selector logic. |
| `src/app/features/user/components/food-order/menu-grid.component.html` | Toggle between “Add” button and +/- controls per item. |
| `src/app/features/user/components/food-order/food-order.component.ts` | Add confirmation dialog before placing order. |
| `src/app/features/user/components/request-service/request-service.component.ts` | Add confirmation dialog before submit; mobile dropdown logic. |
| `src/app/features/user/components/request-service/request-service.component.html` | Show dropdown on mobile, toggle on desktop. |
| `src/app/features/user/components/my-requests/my-requests.component.ts` | Add detail modal component and row click handler; `isMobile` signal. |
| `src/app/features/user/components/my-requests/my-requests.component.html` | Render two tables (mobile vs desktop) based on `isMobile`. |
| `src/app/features/user/components/my-requests/my-requests.component.scss` | Mobile styling. |
| `src/app/features/user/components/booking-wizard/booking-wizard.component.ts` | Add confirmation dialog before final submit. |
| `src/app/features/user/components/food-order/menu-grid.component.scss` | Responsive adjustments. |
| `src/styles.scss` (or global) | Add 320px breakpoint safety rules. |
| **New:** `src/app/features/user/components/my-requests/request-detail-dialog.component.ts` | Standalone dialog for request details on mobile. |

## 3. Menu Grid – Inline Quantity Selector

### 3.1 Component State
Add an internal `cartMap = signal<Record<number, number>>({})` to track the quantity of each item added. This avoids coupling with the parent cart; the grid will emit `addToCart` on first addition and `updateQuantity` on subsequent changes. The parent (FoodOrderComponent) already handles `updateQuantity` from the cart, but now the grid will also emit it directly.

**Change in `menu-grid.component.ts`:**

```ts
import { output, input, signal } from '@angular/core';
import { MenuItem } from '../../../models/menu-item.model';

export class MenuGridComponent {
  menuItems = input.required<MenuItem[]>();
  addToCart = output<MenuItem>();
  updateQuantity = output<{ menuItemId: number; delta: number }>();

  cartMap = signal<Record<number, number>>({});

  getQuantity(menuItemId: number): number {
    return this.cartMap()[menuItemId] || 0;
  }

  increment(item: MenuItem): void {
    const current = this.cartMap()[item.id] || 0;
    if (current === 0) {
      this.addToCart.emit(item);
    } else {
      this.updateQuantity.emit({ menuItemId: item.id, delta: 1 });
    }
    this.cartMap.update(m => ({ ...m, [item.id]: (m[item.id] || 0) + 1 }));
  }

  decrement(item: MenuItem): void {
    const current = this.cartMap()[item.id] || 0;
    if (current > 0) {
      this.updateQuantity.emit({ menuItemId: item.id, delta: -1 });
      this.cartMap.update(m => {
        const newQty = (m[item.id] || 0) - 1;
        if (newQty <= 0) {
          const { [item.id]: _, ...rest } = m;
          return rest;
        }
        return { ...m, [item.id]: newQty };
      });
    }
  }
}
```

### 3.2 Template
Replace the static “Add to Cart” button inside the menu item cards with:

```html
@if (getQuantity(item.id) === 0) {
  <button mat-raised-button (click)="increment(item)">Add to Cart</button>
} @else {
  <div class="qty-controls">
    <button type="button" mat-icon-button (click)="decrement(item)"><mat-icon>remove</mat-icon></button>
    <span>{{ getQuantity(item.id) }}</span>
    <button type="button" mat-icon-button (click)="increment(item)"><mat-icon>add</mat-icon></button>
  </div>
}
```

## 4. Confirmation Dialogs

### 4.1 Food Order – Place Order
**File:** `food-order.component.ts`

- Inject `MatDialog`.
- Import `ConfirmDialogComponent` from `'../../../../shared/components/confirm-dialog/confirm-dialog.component'` (adjust path).
- Modify `placeOrder()`:

```ts
placeOrder(): void {
  if (!this.canCheckout()) return;
  const dialogRef = this.dialog.open(ConfirmDialogComponent, {
    data: {
      title: 'Confirm Order',
      message: `Place this order? Total: ${this.subtotal() | currency}`,
    },
  });
  dialogRef.afterClosed().pipe(takeUntilDestroyed(this.destroyRef)).subscribe(confirmed => {
    if (confirmed) {
      this.submitOrder();
    }
  });
}

private submitOrder(): void {
  // existing API call logic, emits orderPlaced on success
}
```

### 4.2 Request Service – Submit
**File:** `request-service.component.ts`

- Inject `MatDialog`.
- Import `ConfirmDialogComponent`.
- Modify the submit handler:

```ts
submitRequest(): void {
  if (this.description.invalid || this.submitting()) return;
  const room = this.activeBooking().rooms.find(r => r.roomId === this.selectedRoomId.value);
  const roomLabel = room?.roomNumber ?? 'selected room';
  const dialogRef = this.dialog.open(ConfirmDialogComponent, {
    data: {
      title: 'Confirm Service Request',
      message: `Send a ${this.requestType.value} request for ${roomLabel}?`,
    },
  });
  dialogRef.afterClosed().pipe(takeUntilDestroyed(this.destroyRef)).subscribe(confirmed => {
    if (confirmed) {
      this.performSubmit();
    }
  });
}

private performSubmit(): void {
  // existing API call logic, emits requestCreated on success
}
```

### 4.3 Bookings Wizard – Confirm Booking
**File:** `booking-wizard.component.ts`

- Inject `MatDialog`, import `ConfirmDialogComponent`.
- Modify `submitBooking()`:

```ts
submitBooking(): void {
  const dialogRef = this.dialog.open(ConfirmDialogComponent, {
    data: {
      title: 'Confirm Booking',
      message: `Create this booking? Total estimated: ${this.estimatedTotal() | currency}`,
    },
  });
  dialogRef.afterClosed().pipe(takeUntilDestroyed(this.destroyRef)).subscribe(confirmed => {
    if (confirmed) {
      this.performBooking();
    }
  });
}

private performBooking(): void {
  // existing create booking logic, emits bookingCreated on success
}
```

Ensure `ConfirmDialogComponent` is added to the `imports` array of each component.

## 5. Category Dropdown Filter

### 5.1 Menu Grid Changes
**File:** `menu-grid.component.ts`

- Add `FormControl` import.
- Add `categoryFilter` control and computed properties:

```ts
import { FormControl } from '@angular/forms';

categoryFilter = new FormControl('All', { nonNullable: true });

categories = computed(() => {
  const cats = new Set(this.menuItems().map(i => i.category || 'Other'));
  return Array.from(cats).sort();
});

filteredGroups = computed(() => {
  const selected = this.categoryFilter.value;
  const items = selected === 'All' 
    ? this.menuItems() 
    : this.menuItems().filter(i => (i.category || 'Other') === selected);
  const groups: Record<string, MenuItem[]> = {};
  for (const item of items) {
    const cat = item.category || 'Other';
    if (!groups[cat]) groups[cat] = [];
    groups[cat].push(item);
  }
  return Object.entries(groups).map(([category, items]) => ({ category, items }));
});
```

**In the template**, add a `mat-form-field` with `mat-select` above the menu items, and iterate over `filteredGroups()` instead of `groupedMenu()`.

```html
<mat-form-field appearance="outline">
  <mat-label>Category</mat-label>
  <mat-select [formControl]="categoryFilter">
    <mat-option value="All">All</mat-option>
    @for (cat of categories(); track cat) {
      <mat-option [value]="cat">{{ cat }}</mat-option>
    }
  </mat-select>
</mat-form-field>
```

## 6. Mobile Optimizations

### 6.1 Global 320px Safeguard
Add to `styles.scss`:

```scss
@media (max-width: 360px) {
  body {
    font-size: 14px;
  }
  mat-card {
    margin: 4px;
    padding: 8px;
  }
}
```

### 6.2 My Requests Table – Simplified Columns + Detail Modal
**New Component:** `RequestDetailDialogComponent`  
File: `src/app/features/user/components/my-requests/request-detail-dialog.component.ts`

- Standalone.
- Imports: `MatDialogModule`, `MatButtonModule`, `CommonModule`, `MatDividerModule`, `MatCardModule`.
- Template: displays all fields of `CustomerRequest` in a simple card.
- Receives data via `MAT_DIALOG_DATA` injection.

**MyRequestsComponent changes:**
- Inject `BreakpointObserver`, create `isMobile` signal.
- Add `openDetail(request: CustomerRequest)` method that opens the above dialog.
- Adjust template:

```html
@if (isMobile()) {
  <table mat-table [dataSource]="requests()" matSort matSortDisableClear>
    <ng-container matColumnDef="type">...</ng-container>
    <ng-container matColumnDef="status">...</ng-container>
    <ng-container matColumnDef="createdAt">...</ng-container>
    <tr mat-header-row *matHeaderRowDef="['type','status','createdAt']"></tr>
    <tr mat-row *matRowDef="let row; columns: ['type','status','createdAt']" (click)="openDetail(row)"></tr>
  </table>
} @else {
  <!-- Full desktop table -->
}
```

### 6.3 Request Service – Toggle to Dropdown on Mobile
Add `isMobile` signal (BreakpointObserver) to `RequestServiceComponent`. Use it in template:

```html
@if (isMobile()) {
  <mat-form-field appearance="outline">
    <mat-label>Service Type</mat-label>
    <mat-select [formControl]="requestType">
      <mat-option value="housekeeping">Housekeeping</mat-option>
      <mat-option value="maintenance">Maintenance</mat-option>
    </mat-select>
  </mat-form-field>
} @else {
  <mat-button-toggle-group [formControl]="requestType" aria-label="Service type">
    <mat-button-toggle value="housekeeping"><mat-icon>cleaning_services</mat-icon> Housekeeping</mat-button-toggle>
    <mat-button-toggle value="maintenance"><mat-icon>build</mat-icon> Maintenance</mat-button-toggle>
  </mat-button-toggle-group>
}
```

## 7. Self‑Review Checklist
- [ ] In the menu grid, clicking “Add to Cart” changes the button to a +/- quantity selector; subsequent clicks adjust quantity directly.
- [ ] Placing a food order shows a confirmation dialog with total amount; only proceeding after confirm.
- [ ] Submitting a service request shows a confirmation dialog; only after confirm.
- [ ] Creating a new booking shows a confirmation dialog before API call.
- [ ] The category dropdown filters the menu items correctly; selecting “All” shows everything.
- [ ] On mobile (≤599px), My Requests table shows only type, status, createdAt; clicking a row opens a detail modal with all info.
- [ ] On mobile, the request service type is a dropdown instead of button toggle group.
- [ ] Layouts are stable down to 320px width.
- [ ] No console errors; all imports correctly added.

## 8. Integration Notes
- The `ConfirmDialogComponent` is already in shared; ensure it's imported in all components that need it.
- The `isMobile` signals should use `BreakpointObserver` observing `'(max-width: 599px)'` as done in other components.
- The `RequestDetailDialogComponent` is a new, simple standalone component. Place it in `src/app/features/user/components/my-requests/request-detail-dialog.component.ts`.
- The quantity selector in menu grid does not persist across category filter changes? The `cartMap` is local to the grid component, so it will reset when the component is destroyed/recreated? The grid is always rendered inside `FoodOrderComponent` and not destroyed unless the tab changes. That's acceptable; if the user switches tabs, the state might reset, but it's okay for now. The parent `FoodOrderComponent` holds the actual cart, but the grid's `cartMap` is just for display of quantity in the button. We may want to sync with the parent's cart on init, but it's simpler to let it be independent; the user can still adjust quantities from the cart drawer anyway. So no issue.