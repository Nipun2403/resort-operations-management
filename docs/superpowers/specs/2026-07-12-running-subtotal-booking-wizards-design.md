# Running Subtotal in Booking Wizards

## Purpose

Add a small inline running subtotal preview at each step of the booking wizard in both the User Portal and Front Desk Portal, giving users instant visibility of their accumulating costs.

## Scope

- **Front Desk Wizard** — `FrontDeskBookingWizardComponent` (`new-booking.component.ts/.html`)
- **User Wizard** — `BookingWizardComponent` (`booking-wizard.component.ts/.html`)
- Does **not** touch the Availability search page (no quantity selection there)
- Does **not** touch the Review step (already shows estimated total)
- Does **not** touch any backend, services, or TypeScript logic

## Approach

Both components already have all needed computed signals:

| Signal | Front Desk | User Wizard |
|--------|-----------|-------------|
| `estimatedTotal()` | line 141 | line 198 |
| `nights()` | line 133 | line 153 |
| `totalSelectedQuantity()` | line 118 | line 161 |
| `selectedRoomEntries()` | line 156 | line 178 |

**No new TypeScript.** Only HTML template additions + a few lines of SCSS.

## Changes

### Front Desk Wizard (`new-booking.component.html`)

After the `step-actions` div on each applicable step, add:

**Step 3 (Select Rooms)** — room subtotal preview:
```html
@if (nights() > 0 && totalSelectedQuantity() > 0) {
  <p class="subtotal-inline">Room subtotal: {{ estimatedTotal() | currency }} · {{ nights() }} {{ nights() === 1 ? 'night' : 'nights' }}</p>
}
```

**Step 4 (Amenities)** — full estimated total:
```html
@if (estimatedTotal() > 0) {
  <p class="subtotal-inline">Estimated: {{ estimatedTotal() | currency }}</p>
}
```

### User Wizard (`booking-wizard.component.html`)

After the `actions` div on each applicable step, add the same patterns:

**Step 2 (Select Rooms)** — room subtotal preview:
```html
@if (nights() > 0 && totalSelectedQuantity() > 0) {
  <p class="subtotal-inline">Room subtotal: {{ estimatedTotal() | currency }} · {{ nights() }} {{ nights() === 1 ? 'night' : 'nights' }}</p>
}
```

**Step 3 (Amenities)** — full estimated total:
```html
@if (estimatedTotal() > 0) {
  <p class="subtotal-inline">Estimated: {{ estimatedTotal() | currency }}</p>
}
```

### Styles (shared pattern)

Add `subtotal-inline` class to each component's SCSS:

```scss
.subtotal-inline {
  text-align: right;
  font-size: 13px;
  color: var(--color-secondary);
  margin-top: 8px;
  letter-spacing: 0.5px;
}
```

## Files Modified (4)

1. `Frontend/src/app/features/front-desk/pages/new-booking.component.html`
2. `Frontend/src/app/features/front-desk/pages/new-booking.component.scss`
3. `Frontend/src/app/features/user/components/booking-wizard/booking-wizard.component.html`
4. `Frontend/src/app/features/user/components/booking-wizard/booking-wizard.component.scss`

## Verification

- `npx tsc --noEmit` — zero errors
- Visual check: subtotal appears inline below Next/Back buttons once calculable
