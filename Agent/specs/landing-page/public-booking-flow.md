# Patch Specsheet: Booking Flow Integration (Auth, Dashboard, Booking Wizard)

## 1. Purpose
- Complete the end‑to‑end public booking flow: from the Availability page → login/register → user dashboard → new booking wizard with pre‑filled dates, guests, and selected room type.
- The Auth page is patched to support a `returnUrl` query parameter; after successful login, the user is redirected to that URL if present, otherwise to the role dashboard.
- The Customer Dashboard checks for a `pendingBooking` object in `sessionStorage` (set by the public Availability page) and, if found, automatically navigates to the booking wizard with pre‑filled parameters.
- The Customer Booking Wizard is enhanced to accept pre‑fill values via query parameters, automatically switching to new‑booking mode and populating the dates, guests, and room selection steps.

## 2. Files to Modify

| File | Change |
|------|--------|
| `src/app/features/auth/auth-page.component.ts` | Read `returnUrl` from query params; on successful login, navigate to it if present, else to role dashboard. |
| `src/app/core/guards/auth-redirect.guard.ts` | Read `returnUrl` from the URL’s query params; if already authenticated and `returnUrl` exists, redirect there instead of the role dashboard. |
| `src/app/features/user/pages/dashboard.component.ts` | On init, check for `pendingBooking` in session storage; if found, navigate to `/user/bookings` with query params and clear the storage. |
| `src/app/features/user/pages/bookings.component.ts` | Read query params (`new`, `roomTypeId`, `checkIn`, `checkOut`, `guests`); if `new` is true, switch to new‑booking mode and pass pre‑fill data to the wizard. |
| `src/app/features/user/pages/bookings.component.html` | Pass new inputs to `<app-booking-wizard>`. |
| `src/app/features/user/components/booking-wizard/booking-wizard.component.ts` | Add inputs for initial dates, guests, and room type ID; pre‑fill the wizard steps accordingly. |

## 3. Auth Page Patch – Handle `returnUrl`

### 3.1 `AuthPageComponent`
In the component, inject `ActivatedRoute`. In `ngOnInit` (or the method that handles successful login), capture the `returnUrl` query parameter:

```typescript
private returnUrl: string | null = null;

ngOnInit(): void {
  this.route.queryParams.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(params => {
    this.returnUrl = params['returnUrl'] || null;
  });
}
```

In the login success handler (where the token is stored and the router navigates), replace the role‑based redirect with:

```typescript
// After successful login
if (this.returnUrl) {
  this.router.navigateByUrl(this.returnUrl);
} else {
  // existing role‑based navigation
}
```

**Note:** The `returnUrl` should be a valid Angular route, e.g., `/user/dashboard`. Ensure it doesn’t contain external URLs for security. We'll only navigate if it starts with `/`.

### 3.2 `AuthRedirectGuard`
The guard already redirects authenticated users away from `/auth`. Modify it to check for `returnUrl` in the current URL’s query params. If present, redirect to that instead of the role dashboard.

In the guard function, after determining the user is authenticated, extract the `returnUrl` from the current navigation’s query params:

```typescript
const urlTree = router.parseUrl(state.url);
const returnUrl = urlTree.queryParams['returnUrl'];
if (returnUrl && typeof returnUrl === 'string' && returnUrl.startsWith('/')) {
  return router.parseUrl(returnUrl);
}
// fallback to role‑based redirect
```

Use `state.url` or inject `ActivatedRouteSnapshot` from `CanActivateFn`. The guard receives `(route, state)`; `state.url` contains the full URL.

**Exact code:**

```typescript
export const authRedirectGuard: CanActivateFn = (route, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  if (auth.isAuthenticated()) {
    const urlTree = router.parseUrl(state.url);
    const returnUrl = urlTree.queryParams['returnUrl'];
    if (returnUrl && typeof returnUrl === 'string' && returnUrl.startsWith('/')) {
      return router.parseUrl(returnUrl);
    }
    // existing role‑based redirect
    // ...
  }
  return true;
};
```

## 4. Customer Dashboard Patch – Pending Booking Detection

### 4.1 `UserDashboardComponent`
In `ngOnInit`, after the existing initialization logic, add:

```typescript
const pending = sessionStorage.getItem('pendingBooking');
if (pending) {
  try {
    const data = JSON.parse(pending);
    sessionStorage.removeItem('pendingBooking'); // clear immediately
    // Navigate to bookings with pre‑fill query params
    this.router.navigate(['/user/bookings'], {
      queryParams: {
        new: true,
        roomTypeId: data.roomTypeId,
        checkIn: data.checkIn,
        checkOut: data.checkOut,
        guests: data.guests
      }
    });
    return; // skip normal dashboard loading
  } catch { /* ignore */ }
}
```

Inject `Router`.

## 5. Customer Bookings Page Patch – Query Params to Wizard

### 5.1 `BookingsComponent`
Inject `ActivatedRoute`. In `ngOnInit`, subscribe to query params:

```typescript
ngOnInit(): void {
  this.route.queryParams.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(params => {
    if (params['new'] === 'true') {
      this.viewMode.setValue('new'); // switch to new booking view
      // Set pre‑fill values
      this.initialCheckIn = params['checkIn'] ? new Date(params['checkIn']) : null;
      this.initialCheckOut = params['checkOut'] ? new Date(params['checkOut']) : null;
      this.initialGuests = params['guests'] ? +params['guests'] : null;
      this.initialRoomTypeId = params['roomTypeId'] ? +params['roomTypeId'] : null;
    }
  });
}
```

Add these properties as plain class fields or signals; we'll pass them to the wizard as inputs. For simplicity, use plain properties that are set before the wizard initializes (since `viewMode` is set to `'new'`, the wizard will be rendered and will read these values via `ngOnInit`).

```typescript
initialCheckIn: Date | null = null;
initialCheckOut: Date | null = null;
initialGuests: number | null = null;
initialRoomTypeId: number | null = null;
```

### 5.2 Template update
Pass the initial values to the wizard:

```html
@if (viewMode.value === 'new') {
  <app-booking-wizard
    [userProfile]="userProfile()"
    [initialCheckIn]="initialCheckIn"
    [initialCheckOut]="initialCheckOut"
    [initialGuests]="initialGuests"
    [initialRoomTypeId]="initialRoomTypeId"
    (bookingCreated)="onBookingCreated($event)" />
}
```

## 6. Booking Wizard Patch – Accept Initial Values

### 6.1 `BookingWizardComponent` inputs
Add the following `@Input()` properties (using signals or `input.required` is not necessary; they are optional). Since the wizard already uses `input.required` for `userProfile`, we'll add optional inputs for the initial data:

```typescript
initialCheckIn = input<Date | null>(null);
initialCheckOut = input<Date | null>(null);
initialGuests = input<number | null>(null);
initialRoomTypeId = input<number | null>(null);
```

### 6.2 Lifecycle adjustments
In `ngOnInit` (or using `effect`), after the form is built, if the initial values are present, patch the `datesForm` and later after available rooms are loaded, auto‑select the room.

**Step 1 – Pre‑fill dates and guests:**
```typescript
if (this.initialCheckIn() && this.initialCheckOut() && this.initialGuests()) {
  this.datesForm.patchValue({
    checkInDate: this.initialCheckIn(),
    checkOutDate: this.initialCheckOut(),
    guestCount: this.initialGuests()
  });
  // Optionally mark the step as completed to skip it? The user can still modify.
}
```

**Step 2 – Auto‑select room type after loading:**
In the method that fetches available rooms (when entering step 2 or on init if pre‑fill is active), after the rooms are loaded, if `initialRoomTypeId()` is set, automatically set the quantity to 1 for that room type.

In the `availableRooms` setter (or in the subscription), after updating `availableRooms` signal, check:
```typescript
if (this.initialRoomTypeId()) {
  const room = this.availableRooms().find(r => r.roomTypeId === this.initialRoomTypeId());
  if (room && room.availableCount > 0) {
    this.selectedRoomQuantities.update(q => ({ ...q, [room.roomTypeId]: 1 }));
  }
  // clear the initial value so it doesn't re‑apply on subsequent loads
  this.initialRoomTypeId = signal(null); // but input signals are read-only; we can't clear them. We'll keep it, and it will be applied only once because after the first fetch, the user can adjust. It's fine.
}
```

Since `initialRoomTypeId` is a signal input, we cannot modify it. But the auto‑selection logic will run every time `availableRooms` is set (e.g., when the user returns to step 2 after changing dates), which could re‑select the room type even after the user removed it. To avoid that, we can use a flag that indicates we've already applied the initial selection.

Add a private property `private initialRoomApplied = false;`. In the fetch callback, if `!this.initialRoomApplied && this.initialRoomTypeId()` is set, apply the selection and set `this.initialRoomApplied = true;`. This ensures it only happens once.

**Step 1 auto‑validation:** The wizard’s stepper normally requires the user to manually go through steps. If we pre‑fill step 1, the step is still valid. The user can simply click "Next" or the stepper will already be at step 1; they can proceed. No automatic skipping is needed.

## 7. Self‑Review Checklist
- [ ] Visiting `/auth?returnUrl=/user/dashboard` and logging in successfully redirects to `/user/dashboard`.
- [ ] If already logged in, visiting `/auth?returnUrl=/user/dashboard` redirects to `/user/dashboard` immediately.
- [ ] Without `returnUrl`, login still redirects to the role dashboard.
- [ ] From the public Availability page, clicking “Book Now” as an unauthenticated user stores `pendingBooking`, goes to `/auth?returnUrl=/user/dashboard`, login, and then the dashboard immediately navigates to `/user/bookings?new=true&...`.
- [ ] The bookings page automatically switches to the new‑booking wizard and pre‑fills check‑in, check‑out, guests, and room selection with the chosen room type (quantity = 1).
- [ ] The user can still modify all pre‑filled values and proceed with the booking.
- [ ] No console errors; all subscriptions cleaned.
- [ ] Existing booking wizard functionality remains unchanged when no initial values are provided.

## 8. Integration Notes
- The `pendingBooking` is cleared immediately after reading, preventing repeated navigation.
- The `returnUrl` validation (`startsWith('/')`) prevents open redirects.
- The `initialRoomApplied` flag ensures the room type is selected only once, preserving user adjustments.
- No changes to the public availability page are needed; it already stores `pendingBooking` for unauthenticated users.

