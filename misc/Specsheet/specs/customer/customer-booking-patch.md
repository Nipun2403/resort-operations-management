# Patch Specsheet: Customer Bookings – Reactivity & Stability Fixes

## 1. Purpose
- Fix critical reactivity and layout bugs in the BookingWizard and BookingHistory components, ensuring deterministic signal‑driven updates, proper stepper orientation, and stable API calls.

## 2. Files to Modify

| File | Change |
|------|--------|
| `src/app/features/user/pages/bookings.component.ts` | Add loading guard for user profile, pass refresh/highlight inputs. |
| `src/app/features/user/pages/bookings.component.html` | Wrap child components with `@if (userProfile())`. |
| `src/app/features/user/components/booking-wizard/booking-wizard.component.ts` | Convert form values to signals for `computed`, use `mat-stepper`, trigger validation on quantity changes. |
| `src/app/features/user/components/booking-wizard/booking-wizard.component.html` | Replace `mat-vertical-stepper` with `mat-stepper`, add `type="button"` to quantity buttons. |

## 3. Fix 1: Prevent Race Condition in Orchestrator (Section 5)

**Root cause:** The template renders child components before `userEmail` is populated, causing an API call with an empty email.

**Change in `bookings.component.html`:**

Replace:
```html
@if (viewMode.value === 'history') {
  <app-booking-history ... />
}
@if (viewMode.value === 'new') {
  <app-booking-wizard ... />
}
```
With:
```html
@if (userProfile()) {
  @if (viewMode.value === 'history') {
    <app-booking-history [userEmail]="userEmail()" [highlightBookingId]="newBookingId()" [refresh]="refreshTrigger()" />
  }
  @if (viewMode.value === 'new') {
    <app-booking-wizard [userProfile]="userProfile()!" (bookingCreated)="onBookingCreated($event)" />
  }
} @else {
  <mat-spinner diameter="40"></mat-spinner>
}
```

**In `bookings.component.ts`:** No additional changes; the `userProfile` signal is already set after `getMe` completes. Ensure `newBookingId` and `refreshTrigger` signals are defined.

## 4. Fix 2: Stepper Orientation (Section 7 & 11)

**Root cause:** Using `<mat-vertical-stepper>` prevents switching to horizontal orientation on desktop.

**Change in `booking-wizard.component.html`:**

Replace:
```html
<mat-vertical-stepper linear #stepper>
```
With:
```html
<mat-stepper linear #stepper [orientation]="isMobile() ? 'vertical' : 'horizontal'">
```

Also add `isMobile` signal in the wizard component (similar to other components using `BreakpointObserver`). Inject `BreakpointObserver` and set:
```ts
private breakpointObserver = inject(BreakpointObserver);
isMobile = toSignal(
  this.breakpointObserver.observe('(max-width: 767px)').pipe(map(r => r.matches)),
  { initialValue: false }
);
```
Import `BreakpointObserver` from `@angular/cdk/layout`, `map` from `rxjs`, `toSignal` from `@angular/core/rxjs-interop`.

## 5. Fix 3: Reactivity of `computed()` – Form to Signal Conversion (Section 7)

**Root cause:** `estimatedTotal` reads form control values directly; changes in form controls do not trigger recomputation.

**Add to `booking-wizard.component.ts`:**
```ts
// Convert form values to signals so computed reacts
private datesValues = toSignal(this.datesForm.valueChanges, { initialValue: this.datesForm.value });
private amenitiesValues = toSignal(this.amenitiesForm.valueChanges, { initialValue: this.amenitiesForm.value });
```

**Update `estimatedTotal` to use these signals:**
```ts
estimatedTotal = computed(() => {
  const dates = this.datesValues();
  const amenitiesVal = this.amenitiesValues();
  const nights = this.nights(); // nights derived from datesValues()
  // ... rest of calculation, using amenitiesVal instead of getting individual controls
});
```

Specifically, replace `this.getAmenityControl(i).value` with `amenitiesVal.selectedAmenities?.[i] ?? false`. Since the form structure is a FormArray of boolean controls, `amenitiesVal.selectedAmenities` is an array of booleans. Adapt accordingly.

**Also adjust `nights`** to derive from `datesValues()` instead of directly from form controls. Use `computed`:
```ts
nights = computed(() => {
  const dates = this.datesValues();
  if (!dates.checkInDate || !dates.checkOutDate) return 0;
  const checkIn = new Date(dates.checkInDate);
  const checkOut = new Date(dates.checkOutDate);
  return Math.max(0, Math.ceil((checkOut.getTime() - checkIn.getTime()) / (1000 * 3600 * 24)));
});
```

`datesValues()` contains the raw values (Date objects) from the form. Since the form uses `FormControl<Date | null>`, the signal will emit the form's current value object `{ checkInDate: Date | null, checkOutDate: Date | null, guestCount: number }`.

## 6. Fix 4: Form Validation Reactivity (Section 7)

**Root cause:** Custom validators using signals are not re‑evaluated until `updateValueAndValidity()` is called.

**In `booking-wizard.component.ts`**, inside `incrementRoom` and `decrementRoom` methods, after updating the `selectedRoomQuantities` signal, call:
```ts
this.roomsForm.updateValueAndValidity();
```
Add a similar call inside the effect or method that initially sets up the rooms form, but the key is to trigger it after every quantity change.

Example:
```ts
incrementRoom(roomTypeId: number): void {
  this.selectedRoomQuantities.update(quants => {
    const current = quants[roomTypeId] || 0;
    const max = this.availableRooms().find(r => r.roomTypeId === roomTypeId)?.availableCount ?? 0;
    if (current < max) {
      return { ...quants, [roomTypeId]: current + 1 };
    }
    return quants;
  });
  this.roomsForm.updateValueAndValidity();
}
```
Same for `decrementRoom`.

## 7. Fix 5: Button Type Submission (Section 7)

**Root cause:** Un-typed `<button>` elements inside a form cause unwanted form submission.

**Change in `booking-wizard.component.html`**, inside step 2 quantity buttons:
Add `type="button"` to both `mat-icon-button` elements:

```html
<button type="button" mat-icon-button (click)="decrementRoom(room.roomTypeId)">
  <mat-icon>remove</mat-icon>
</button>
<button type="button" mat-icon-button (click)="incrementRoom(room.roomTypeId)" [disabled]="getRoomQuantity(room.roomTypeId) >= room.availableCount">
  <mat-icon>add</mat-icon>
</button>
```

Also check any other buttons within forms (like Next buttons may have type="submit" by default, that's fine). Ensure all buttons that don't submit the form are `type="button"`.

## 8. Self‑Review Checklist (for the agent)
- [ ] The bookings page shows a spinner while user profile loads; child components do not render prematurely.
- [ ] The stepper displays vertically on mobile (≤767px) and horizontally on desktop; using `mat-stepper` with bound orientation.
- [ ] The `estimatedTotal` updates instantly when rooms, dates, or amenities change.
- [ ] The "Next" button in step 2 becomes enabled/disabled correctly when room selection changes or capacity warning appears.
- [ ] Clicking the +/- buttons does not submit the form or cause any navigation.
- [ ] No console errors, no reactivity warnings.
- [ ] All other functionality from the original spec remains unchanged.

## 9. Integration Notes
- The `isMobile` signal must be added to the wizard component if not already present.
- The conversion to signals for form values requires importing `toSignal` and `takeUntilDestroyed` if needed; ensure imports are complete.
- The `roomsForm` custom validator may need to be re‑evaluated after `updateValueAndValidity()`. The validator function itself uses signals (`totalSelectedQuantity`, `capacityWarning`), but since the validator is re‑run on `updateValueAndValidity()`, it will return the correct result. This works because the validator function is called synchronously and reads the signals at that moment.

---