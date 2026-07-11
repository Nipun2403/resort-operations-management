# Patch Specsheet: Front Desk – Guest Details Fixes & Enhancements

## 1. Purpose

- Fix the inline quantity selector (`- {quantity} +`) in the food order section of the Room Service tab.
- Disable the Room Service and Billing tabs when the guest does not have an active CheckedIn booking.
- Enlarge the Extend Stay date picker dialog so the calendar is fully visible.
- Remove the confirmation dialog when cancelling a booking.
- Replace the booking list in the **Overview** tab with guest profile details fetched from `GET /api/v1/guests?search=<email>`.

## 2. Files to Modify

| File                                                                                                           | Change                                                                                                           |
| -------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------- |
| `src/app/features/front-desk/pages/guest-details.component.ts`                                                 | Add guest profile fetch, remove cancel confirmation, increase extend‑stay dialog size, import `GuestApiService`. |
| `src/app/features/front-desk/pages/guest-details.component.html`                                               | Update Overview tab to show guest profile; disable Room Service and Billing tabs when `activeBooking` is null.   |
| `src/app/features/front-desk/components/booking-action-modal/food-order-panel/food-order-panel.component.html` | Verify `updateQuantity` binding is present; if missing, add it.                                                  |
| `src/app/features/front-desk/components/booking-action-modal/food-order-panel/food-order-panel.component.ts`   | Ensure `onUpdateCartQty` correctly updates `cartItems` signal.                                                   |
| **New:** `src/app/features/front-desk/services/guest-api.service.ts`                                           | Create service with `search(query: string): Observable<GuestProfile[]>` method.                                  |
| **New:** `src/app/features/front-desk/models/guest-profile.model.ts`                                           | Define `GuestProfile` interface.                                                                                 |

## 3. Guest Profile Overview

### 3.1 Create `GuestApiService`

**File:** `src/app/features/front-desk/services/guest-api.service.ts`

```typescript
import { Injectable, inject } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { environment } from "../../../../../environments/environment";
import { GuestProfile } from "../models/guest-profile.model";

@Injectable({ providedIn: "root" })
export class GuestApiService {
  private http = inject(HttpClient);
  private baseUrl = environment.baseUrl;

  search(
    query: string,
  ): Observable<{ totalCount: number; data: GuestProfile[] }> {
    return this.http.get<{ totalCount: number; data: GuestProfile[] }>(
      `${this.baseUrl}/guests`,
      { params: { search: query, pageSize: 25 } },
    );
  }
}
```

### 3.2 Define `GuestProfile` model

**File:** `src/app/features/front-desk/models/guest-profile.model.ts`

```typescript
export interface GuestProfile {
  guestName: string;
  guestEmail: string;
  totalStays: number;
  lastCheckInDate: string;
}
```

### 3.3 Integrate into `GuestDetailsComponent`

**Add to imports:** `GuestApiService`

**Add signal:**

```typescript
guestProfile = signal<GuestProfile | null>(null);
```

**Fetch in `ngOnInit` or `fetchBookings`:**

```typescript
private guestApi = inject(GuestApiService);

// Inside ngOnInit, after setting email and fetching bookings, also call:
this.guestApi.search(this.email()).pipe(
  takeUntilDestroyed(this.destroyRef)
).subscribe({
  next: res => {
    if (res.data && res.data.length > 0) {
      this.guestProfile.set(res.data[0]);
    }
  },
  error: (err: any) => console.error('Failed to load guest profile', err)
});
```

### 3.4 Update Overview Tab Template

Replace the current booking list in the Overview tab with:

```html
<mat-tab label="Overview">
  <div class="tab-content">
    @if (guestProfile()) {
    <div class="profile-info">
      <h3>{{ guestProfile()!.guestName }}</h3>
      <p><strong>Email:</strong> {{ guestProfile()!.guestEmail }}</p>
      <p><strong>Total Stays:</strong> {{ guestProfile()!.totalStays }}</p>
      <p>
        <strong>Last Check‑In:</strong> {{ guestProfile()!.lastCheckInDate }}
      </p>
      <p><strong>Current Status:</strong> {{ getOverallStatus() }}</p>
    </div>
    } @else {
    <p>Loading guest profile...</p>
    }
  </div>
</mat-tab>
```

## 4. Disable Room Service & Billing Tabs for Non‑CheckedIn Guests

### 4.1 Template Change

In the `<mat-tab-group>`, modify the Room Service and Billing tabs:

```html
<mat-tab
  label="Room Service"
  [disabled]="!activeBooking()"
>
  ...
</mat-tab>
<mat-tab
  label="Billing"
  [disabled]="!activeBooking()"
>
  ...
</mat-tab>
```

The `disabled` attribute will gray out the tabs and prevent clicks when no CheckedIn booking exists.

## 5. Enlarge Extend Stay Date Picker

### 5.1 In `GuestDetailsComponent` – `extendStay` method

Increase the dialog size:

```typescript
extendStay(booking: Booking): void {
  const extendRef = this.dialog.open(ExtendStayDialogComponent, {
    data: { bookingId: booking.id, currentCheckOut: booking.checkOutDate },
    width: '400px',
    maxWidth: '90vw',
  });
  ...
}
```

This gives the calendar sufficient space to open fully.

## 6. Remove Cancellation Confirmation

### 6.1 Modify `cancelBooking` method in `GuestDetailsComponent`

Replace the existing method with:

```typescript
cancelBooking(booking: Booking): void {
  this.bookingApi.cancel(booking.id).pipe(
    takeUntilDestroyed(this.destroyRef)
  ).subscribe({
    next: () => {
      this.snackBar.open('Booking cancelled.', 'Close', { duration: 3000 });
      this.fetchBookings();
    },
    error: (err) => this.snackBar.open(this.extractErrorMessage(err), 'Close', { duration: 5000 })
  });
}
```

**No confirmation dialog** is shown; cancellation happens immediately.

## 7. Fix Food Order Inline Quantity Selector

### 7.1 Verify `MenuGridComponent` usage

The `FoodOrderPanelComponent` should already bind `updateQuantity` to `onUpdateCartQty`. Check the template and add it if missing.

In `food-order-panel.component.html`, ensure:

```html
<app-menu-grid
  [menuItems]="menuItems()"
  (addToCart)="onAddToCart($event)"
  (updateQuantity)="onUpdateCartQty($event)"
/>
```

### 7.2 Ensure `onUpdateCartQty` works correctly

In `food-order-panel.component.ts`, verify that the method updates the `cartItems` signal:

```typescript
onUpdateCartQty(event: { menuItemId: number; delta: number }): void {
  this.cartItems.update(items => {
    return items.map(i => i.menuItemId === event.menuItemId ? { ...i, quantity: Math.max(0, i.quantity + event.delta) } : i)
                .filter(i => i.quantity > 0);
  });
}
```

If any deviation exists, correct it.

### 7.3 Verify `MenuGridComponent` is shared

The `MenuGridComponent` imported must be the one from the customer module (which already includes the inline +/- logic). The import path should be:

```typescript
import { MenuGridComponent } from "../../../customer/components/food-order/menu-grid/menu-grid.component";
```

If not, adjust the import to the correct shared location.

## 8. Self‑Review Checklist

- [ ] Guest Details page shows profile info (name, email, total stays, last check-in) in Overview tab.
- [ ] Room Service and Billing tabs are disabled when no CheckedIn booking exists.
- [ ] Extend Stay dialog opens with a large enough date picker.
- [ ] Cancelling a booking no longer shows a confirmation dialog; cancellation is immediate with snackbar feedback.
- [ ] In Room Service, the `- {quantity} +` selector appears after adding an item and correctly adjusts quantity.
- [ ] All existing features (check‑in, check‑out, extend stay, room service, billing) still work.

## 9. Integration Notes

- The `GuestApiService` uses the same environment base URL as other services; ensure the `HttpClient` module is provided in `app.config.ts`.
- The `ExtendStayDialogComponent` already exists; only the dialog open parameters are modified.
- The `MenuGridComponent` is reused from the customer feature; no changes are needed to the component itself unless the `updateQuantity` output is missing. If missing, add it as defined in the customer menu grid patch specsheet.
- No other components are affected.

