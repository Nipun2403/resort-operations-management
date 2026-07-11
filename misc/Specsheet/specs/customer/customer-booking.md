# Specsheet: Customer Bookings Page (Component Decomposition & Enhanced Determinism)

## 1. Purpose

- Replace the `PlaceholderCustomerBookingsComponent` with a fully modular Bookings page.
- The page is a **thin orchestrator** that hosts a toggle between **BookingHistoryComponent** and **BookingWizardComponent**, and manages shared state (user email from JWT, view mode).
- All heavy logic is delegated to dedicated child components, keeping each file small and testable.
- Modal dialogs for detail, cancel, billing, and feedback are separate standalone components.

## 2. Architecture – Component Tree

```
BookingsComponent (page shell)
├── BookingHistoryComponent
│   ├── BookingDetailDialog (opened on row click)
│   ├── CancelConfirmationDialog (shared, reused)
│   ├── BillingDialog (opened on billing action)
│   └── FeedbackDialog (opened on feedback action)
└── BookingWizardComponent
    ├── Step 1: Dates & Guests
    ├── Step 2: Room Selection (calls availability API, quantity selectors)
    ├── Step 3: Amenities (checkbox list)
    └── Step 4: Review & Confirm (price computation, submit)
```

## 3. Route & Navigation

- Path: `/user/bookings` (lazy‑loaded under Customer Shell).
- **Overwrite** the placeholder file: `src/app/features/user/pages/bookings.component.ts`.
- The page component (`BookingsComponent`) is the routed component.

## 4. Authorization

- Already protected by `customerGuard` from parent route.

## 5. BookingsComponent (Orchestrator)

**Selector**: `app-customer-bookings`  
**Standalone**: `true`  
**Imports**: `CommonModule`, `MatButtonToggleModule`, `BookingHistoryComponent`, `BookingWizardComponent`, `AuthApiService`, `DestroyRef`.  
**Exact import paths** (abbreviated, agent must use correct paths).

**Template**:

```html
<div class="bookings-page">
  <div class="toggle-row">
    <mat-button-toggle-group
      [formControl]="viewMode"
      aria-label="View"
    >
      <mat-button-toggle value="history">My Bookings</mat-button-toggle>
      <mat-button-toggle value="new">New Booking</mat-button-toggle>
    </mat-button-toggle-group>
  </div>

  @if (viewMode.value === 'history') {
  <app-booking-history [userEmail]="userEmail()" />
  } @if (viewMode.value === 'new') {
  <app-booking-wizard
    [userProfile]="userProfile()"
    (bookingCreated)="onBookingCreated()"
  />
  }
</div>
```

**State & Logic**:

```ts
viewMode = new FormControl<'history' | 'new'>('history', { nonNullable: true });
userEmail = signal('');
userProfile = signal<{ firstName: string; lastName: string; email: string } | null>(null);

private authApi = inject(AuthApiService);
private destroyRef = inject(DestroyRef);

ngOnInit(): void {
  this.authApi.getMe().pipe(takeUntilDestroyed(this.destroyRef)).subscribe(me => {
    const given = me.claims?.find(c => c.type === '...claims/givenname')?.value ?? '';
    const surname = me.claims?.find(c => c.type === '...claims/surname')?.value ?? '';
    const email = me.claims?.find(c => c.type === '...claims/name')?.value ?? '';
    this.userEmail.set(email);
    this.userProfile.set({ firstName: given, lastName: surname, email });
  });
}

onBookingCreated(): void {
  // Switch to history view and trigger refresh via a key change or direct method?
  // We'll use a simple approach: viewMode set to history, and BookingHistoryComponent will auto-refresh on init.
  // To force refresh, we can use a signal that changes each time we need a refresh, but since history component will be re-created when we toggle view? Actually both are always in DOM with @if, so not re-created.
  // Better: Add an output from wizard that triggers a method here, which then sends a refresh input to history.
  // We'll add a `refresh` input to BookingHistoryComponent (a number signal that increments).
  // So we'll include a `refreshTrigger = signal(0)` and pass it.
}
```

**Template update**:

```html
<app-booking-history
  [userEmail]="userEmail()"
  [refresh]="refreshTrigger()"
/>
```

And `onBookingCreated()`:

```ts
this.refreshTrigger.update(n => n + 1);
this.viewMode.setValue('history');
// After switching to history, the new booking should be highlighted.
// The history component will have a `highlightBookingId` input.
// We can store the new booking ID from the wizard's output event.
newBookingId = signal<number | null>(null);
(bookingCreated) event emits the created booking's ID.
```

Thus `(bookingCreated)="onBookingCreated($event)"`:

```ts
onBookingCreated(bookingId: number): void {
  this.newBookingId.set(bookingId);
  this.refreshTrigger.update(n => n + 1);
  this.viewMode.setValue('history');
}
```

Pass `highlightBookingId` to history component.

## 6. BookingHistoryComponent

**Selector**: `app-booking-history`  
**Standalone**: `true`  
**Inputs**:

```ts
userEmail = input.required<string>();
refresh = input(0);
highlightBookingId = input<number | null>(null);
```

**Outputs**: none (handles dialogs internally).  
**Imports**: MatTable, MatSort, MatPaginator, MatDialog, etc.

**Template** (exact):

```html
<div class="history-view">
  <div class="controls">
    <mat-form-field appearance="outline">
      <mat-label>Status</mat-label>
      <mat-select
        [formControl]="statusFilter"
        (selectionChange)="onFilterChange()"
      >
        <mat-option value="">All</mat-option>
        <mat-option value="Booked">Booked</mat-option>
        <mat-option value="CheckedIn">Checked In</mat-option>
        <mat-option value="CheckedOut">Checked Out</mat-option>
        <mat-option value="Cancelled">Cancelled</mat-option>
      </mat-select>
    </mat-form-field>
    @if (statusFilter.value) {
    <button
      mat-button
      (click)="clearFilter()"
    >
      Clear Filter
    </button>
    }
  </div>

  @if (loading() && bookings().length === 0) {
  <mat-spinner diameter="40"></mat-spinner>
  } @else if (error()) {
  <app-alert
    type="error"
    [message]="error()!"
    (closed)="error.set(null)"
  >
    <button
      mat-button
      (click)="fetchData()"
    >
      Retry
    </button>
  </app-alert>
  } @if (bookings().length > 0 || loading()) { @if (loading()) {
  <mat-progress-bar mode="indeterminate"></mat-progress-bar>
  }
  <table
    mat-table
    [dataSource]="bookings()"
    matSort
    matSortDisableClear
    (matSortChange)="onSortChange($event)"
  >
    <!-- columns: ID, Check-in, Check-out, Status, Rooms, Actions -->
    <ng-container matColumnDef="id"
      ><th
        mat-header-cell
        *matHeaderCellDef
        mat-sort-header="id"
      >
        ID
      </th>
      <td
        mat-cell
        *matCellDef="let b"
      >
        {{ b.id }}
      </td></ng-container
    >
    <ng-container matColumnDef="checkIn"
      ><th
        mat-header-cell
        *matHeaderCellDef
      >
        Check‑in
      </th>
      <td
        mat-cell
        *matCellDef="let b"
      >
        {{ b.checkInDate }}
      </td></ng-container
    >
    <ng-container matColumnDef="checkOut"
      ><th
        mat-header-cell
        *matHeaderCellDef
      >
        Check‑out
      </th>
      <td
        mat-cell
        *matCellDef="let b"
      >
        {{ b.checkOutDate }}
      </td></ng-container
    >
    <ng-container matColumnDef="status"
      ><th
        mat-header-cell
        *matHeaderCellDef
        mat-sort-header="bookingStatus"
      >
        Status
      </th>
      <td
        mat-cell
        *matCellDef="let b"
      >
        {{ b.bookingStatus }}
      </td></ng-container
    >
    <ng-container matColumnDef="rooms"
      ><th
        mat-header-cell
        *matHeaderCellDef
      >
        Rooms
      </th>
      <td
        mat-cell
        *matCellDef="let b"
      >
        {{ getRoomsSummary(b) }}
      </td></ng-container
    >
    <ng-container matColumnDef="actions"
      ><th
        mat-header-cell
        *matHeaderCellDef
      >
        Actions
      </th>
      <td
        mat-cell
        *matCellDef="let b"
      >
        <button
          mat-icon-button
          (click)="openDetail(b)"
          aria-label="View"
        >
          <mat-icon>visibility</mat-icon>
        </button>
        @if (b.bookingStatus === 'Booked') {
        <button
          mat-icon-button
          (click)="cancelBooking(b)"
          aria-label="Cancel"
        >
          <mat-icon>cancel</mat-icon>
        </button>
        } @if (b.bookingStatus === 'CheckedOut') {
        <button
          mat-icon-button
          (click)="openFeedback(b)"
          aria-label="Feedback"
        >
          <mat-icon>feedback</mat-icon>
        </button>
        }
        <button
          mat-icon-button
          (click)="openBilling(b)"
          aria-label="Billing"
        >
          <mat-icon>receipt</mat-icon>
        </button>
      </td></ng-container
    >
    <tr
      mat-header-row
      *matHeaderRowDef="displayedColumns"
    ></tr>
    <tr
      mat-row
      *matRowDef="let row; columns: displayedColumns"
      [class.highlight]="highlightBookingId() === row.id"
    ></tr>
  </table>
  <mat-paginator ...></mat-paginator>
  } @else {
  <p>No bookings found.</p>
  }
</div>
```

**State & Logic**:

- `bookings`, `totalCount`, `loading`, `error` signals.
- `pageIndex`, `pageSize`, `sortField`, `sortDescending`, `statusFilter` (FormControl).
- `fetchData()`: uses injected `BookingApiService.getAll` with `guestQuery=userEmail()`, status, pagination, sort.
- On `refresh` input change (effect), re-fetch and go to page 0.
- On `highlightBookingId` input: if set, after fetch, scroll to that row using `scrollIntoView` (similar to admin generic crud highlight).
- Actions: `openDetail` opens `BookingDetailDialogComponent` with booking data. `cancelBooking` opens confirmation dialog, calls `BookingApiService.cancel(id)`, refreshes. `openFeedback` opens `FeedbackDialogComponent` (if feedback exists, show it; else show form). `openBilling` opens `BillingDialogComponent` with billing data from `BillingApiService.getByBookingId(id)`.

## 7. BookingWizardComponent

**Selector**: `app-booking-wizard`  
**Standalone**: `true`  
**Inputs**: `userProfile = input.required<{firstName:string;lastName:string;email:string}>()`  
**Outputs**: `bookingCreated = output<number>()`

**Template**:

```html
<mat-vertical-stepper
  linear
  #stepper
>
  <!-- Step 1: Dates & Guests -->
  <mat-step
    [stepControl]="datesForm"
    label="Dates & Guests"
  >
    <form [formGroup]="datesForm">
      <mat-form-field
        ><mat-label>Check-in</mat-label
        ><input
          matInput
          [matDatepicker]="cinPicker"
          formControlName="checkInDate" /><mat-datepicker
          #cinPicker
        ></mat-datepicker
      ></mat-form-field>
      <mat-form-field
        ><mat-label>Check-out</mat-label
        ><input
          matInput
          [matDatepicker]="coutPicker"
          formControlName="checkOutDate" /><mat-datepicker
          #coutPicker
        ></mat-datepicker
      ></mat-form-field>
      <mat-form-field
        ><mat-label>Guests</mat-label
        ><input
          matInput
          type="number"
          formControlName="guestCount"
          min="1"
          max="20"
      /></mat-form-field>
      <div>
        <button
          mat-button
          matStepperNext
        >
          Next
        </button>
      </div>
    </form>
  </mat-step>

  <!-- Step 2: Room Selection -->
  <mat-step
    [stepControl]="roomsForm"
    label="Select Rooms"
  >
    <form [formGroup]="roomsForm">
      <div class="room-list">
        @for (room of availableRooms(); track room.roomTypeId) {
        <div class="room-item">
          <p>
            {{ room.name }} – {{ room.basePrice | currency }}/night – Max
            occupancy: {{ room.maxOccupancy }} – Available: {{
            room.availableCount }}
          </p>
          <div class="quantity-selector">
            <button
              mat-icon-button
              (click)="decrementRoom(room.roomTypeId)"
            >
              <mat-icon>remove</mat-icon>
            </button>
            <span>{{ getRoomQuantity(room.roomTypeId) }}</span>
            <button
              mat-icon-button
              (click)="incrementRoom(room.roomTypeId)"
              [disabled]="getRoomQuantity(room.roomTypeId) >= room.availableCount"
            >
              <mat-icon>add</mat-icon>
            </button>
          </div>
        </div>
        }
      </div>
      @if (capacityWarning()) {
      <p class="warning">{{ capacityWarning() }}</p>
      }
      <button
        mat-button
        matStepperNext
        [disabled]="totalSelectedQuantity() === 0 || capacityWarning()"
      >
        Next
      </button>
    </form>
  </mat-step>

  <!-- Step 3: Amenities -->
  <mat-step
    [stepControl]="amenitiesForm"
    label="Add Amenities"
  >
    <form [formGroup]="amenitiesForm">
      <div class="amenity-list">
        @for (amenity of availableAmenities(); track amenity.id; let i = $index)
        {
        <mat-checkbox [formControl]="getAmenityControl(i)"
          >{{ amenity.name }} – {{ amenity.price | currency }}</mat-checkbox
        >
        }
      </div>
      <button
        mat-button
        matStepperNext
      >
        Next
      </button>
    </form>
  </mat-step>

  <!-- Step 4: Review & Confirm -->
  <mat-step label="Review & Confirm">
    <div class="summary">
      <h3>
        Guest: {{ userProfile().firstName }} {{ userProfile().lastName }} ({{
        userProfile().email }})
      </h3>
      <p>Check-in: {{ datesForm.value.checkInDate | date }}</p>
      <p>Check-out: {{ datesForm.value.checkOutDate | date }}</p>
      <p>Nights: {{ nights() }}</p>
      <p>Guests: {{ datesForm.value.guestCount }}</p>
      <h4>Rooms:</h4>
      <ul>
        @for (item of selectedRoomEntries(); track item.roomTypeId) {
        <li>
          {{ item.name }} x{{ item.quantity }} – {{ item.basePrice | currency
          }}/night – Subtotal: {{ item.quantity * item.basePrice * nights() |
          currency }}
        </li>
        }
      </ul>
      <h4>Amenities:</h4>
      <ul>
        @for (item of selectedAmenityEntries(); track item.id) {
        <li>{{ item.name }} – {{ item.price | currency }}</li>
        }
      </ul>
      <p><strong>Total Estimated: {{ estimatedTotal() | currency }}</strong></p>
    </div>
    <button
      mat-raised-button
      color="primary"
      (click)="submitBooking()"
    >
      Confirm Booking
    </button>
  </mat-step>
</mat-vertical-stepper>
```

**State & Logic**:

- `datesForm`, `roomsForm`, `amenitiesForm` as defined in earlier spec but now with quantity controls.
- `availableRooms` signal of `AvailableRoomType[]` fetched on step 2 init (triggered by `stepper.selectedIndexChange` or on next click). We'll fetch when entering step 2 using `(animationDone)` or use step interaction. Simpler: fetch in `ngAfterViewInit` and when step changes via `selectedIndex` two-way binding. We'll add a method `loadRooms()` called when step index becomes 1.
- `availableAmenities` signal fetched similarly when step index becomes 2.
- Quantity selector: For each room type, we maintain a `FormControl` of type number inside a `FormRecord`? We'll use a simpler approach: `selectedRoomQuantities` signal of `Record<number, number>` (roomTypeId → quantity). We'll use that to drive UI and validation. The `roomsForm` will contain a custom validator that ensures total quantity > 0 and capacity check. We'll use a reactive approach: on step 2 entry, we create/update the form with a `FormArray` of groups for selected rooms, but the quantity buttons directly modify the signal and then patch the form. For simplicity and determinism, we'll implement:

  ```ts
  selectedRoomQuantities = signal<Record<number, number>>({});
  incrementRoom(roomTypeId: number) { ... update signal, recalc capacity ... }
  decrementRoom(roomTypeId: number) { ... }
  getRoomQuantity(roomTypeId: number): number { return this.selectedRoomQuantities()[roomTypeId] || 0; }
  totalSelectedQuantity = computed(() => Object.values(this.selectedRoomQuantities()).reduce((a,b)=>a+b,0));
  ```

  The form validity for step 2: we can mark the step as valid when `totalSelectedQuantity() > 0 && !capacityWarning()`. Use `stepControl` that is a `FormGroup` containing a hidden control that we manually set validity. Or better, use a `FormGroup` with a single control that we update based on the conditions. We'll define `roomsForm = new FormGroup({ dummy: new FormControl(true) })` and override its validity via custom validator: `roomsForm.setValidators(() => { if (totalSelectedQuantity() === 0 || capacityWarning()) return { invalid: true }; return null; })`. This works.

- Capacity warning: computed:

  ```ts
  capacityWarning = computed(() => {
    const totalCap = this.availableRooms().reduce(
      (sum, r) =>
        sum +
        (this.selectedRoomQuantities()[r.roomTypeId] || 0) * r.maxOccupancy,
      0,
    );
    const guests = this.datesForm.value.guestCount || 0;
    if (totalCap < guests)
      return `The selected rooms can only accommodate ${totalCap} guests. You need ${guests}.`;
    return null;
  });
  ```

- Nights: computed from dates difference.

- `selectedRoomEntries()`: maps availableRooms filtered to those with quantity>0, and creates array with name, basePrice, quantity.

- `selectedAmenityEntries()`: from availableAmenities filtered by checked controls.

- **Estimated Total**: computed:

  ```ts
  estimatedTotal = computed(() => {
    const nights = this.nights();
    const roomCost = this.availableRooms().reduce(
      (sum, r) =>
        sum +
        (this.selectedRoomQuantities()[r.roomTypeId] || 0) *
          r.basePrice *
          nights,
      0,
    );
    const amenityCost = this.availableAmenities().reduce(
      (sum, a, i) => sum + (this.getAmenityControl(i).value ? a.price : 0),
      0,
    );
    return roomCost + amenityCost;
  });
  ```

- Submit: builds DTO exactly as previously specified: roomTypeIds flat array, amenityIds, guest details, dates ISO strings. Calls `POST /bookings`. On success, emits `bookingCreated.emit(response.id)`.

## 8. Dialogs (standalone components)

### BookingDetailDialogComponent

- Receives booking via `MAT_DIALOG_DATA`.
- Displays all booking fields, rooms, amenities (maybe fetch amenity names? The booking has amenityIds array, but not names. We can omit amenity names or call amenity API to resolve them. For now, just display IDs, or later enhance. We'll include a TODO to resolve amenity names.)

### CancelConfirmationDialogComponent (reuse shared)

- Already exists in shared.

### BillingDialogComponent

- Injects `BillingApiService`, given bookingId from data.
- On init, calls `GET /billing/{bookingId}` and displays billing info (amount paid, method, etc.). If no billing, show "No billing details."

### FeedbackDialogComponent

- Injects `FeedbackApiService`, given bookingId from data.
- On init, calls `GET /feedback/booking/{bookingId}` to check existing feedback.
- If exists: display read-only (rating, comments). If not: show form with rating (1‑5) and comments (optional), submit calls `POST /feedback`.

## 9. History Refresh & Highlight after New Booking

- `BookingsComponent` passes `highlightBookingId` input to `BookingHistoryComponent`.
- In `BookingHistoryComponent`, effect watches `highlightBookingId` and when non-null, after data is fetched, scroll to row with that ID and add a highlight class for 2 seconds (CSS animation). Then reset the input via output? We'll add an output `highlightDone` to clear it, or the parent can reset after timeout. Simpler: the history component internally handles it: when `highlightBookingId` changes, after fetch, set a local signal `highlightRowId` and use that in template, then after 2s clear it. That avoids needing to reset input. So we'll implement that.

## 10. Strong Validation Recap (already in spec, but ensure)

- Date validators: checkInDate not before today, checkOutDate > checkInDate.
- Guest count: 1‑20.
- Room selection: at least 1 room, capacity check.
- All steps must be valid before proceeding.

## 11. Responsive Behaviour

- Stepper orientation: `isMobile` signal (≤767px) sets `orientation="vertical"`, else `"horizontal"`. Use `BreakpointObserver` in wizard component.
- Quantity buttons should be large enough for touch.

## 12. Session Storage

- For history view, save filter, sort, page as in other list pages. Implement in `BookingHistoryComponent` with schema: `{ status: string, sortField: string, sortDescending: boolean, pageIndex: number, pageSize: number }`.

## 13. File Structure

```
src/app/features/user/
  pages/
    bookings.component.ts
    bookings.component.html
    bookings.component.scss
  components/
    booking-history/
      booking-history.component.ts
      booking-history.component.html
      booking-history.component.scss
    booking-wizard/
      booking-wizard.component.ts
      booking-wizard.component.html
      booking-wizard.component.scss
    booking-detail-dialog/
      booking-detail-dialog.component.ts
      booking-detail-dialog.component.html
    billing-dialog/
      billing-dialog.component.ts
      billing-dialog.component.html
    feedback-dialog/
      feedback-dialog.component.ts
      feedback-dialog.component.html
```

## 14. Self‑Review Checklist

- [ ] Toggle works; history shows user's bookings filtered by status, sorted, paginated.
- [ ] Detail modal opens with booking info.
- [ ] Cancel button appears only for Booked; confirmation; on success list refreshes.
- [ ] Feedback button appears only for CheckedOut; dialog shows existing feedback or form to create.
- [ ] Billing button opens billing details.
- [ ] New Booking wizard: all steps validate correctly; capacity warning displayed and prevents next; room quantity selectors respect available counts.
- [ ] Review step shows complete breakdown with correct estimated total.
- [ ] Submitting booking creates it, emits ID, switches to history view, and highlights the new booking row.
- [ ] History state persists in session storage.
- [ ] Responsive: stepper orientation adapts, quantity controls touch-friendly.
- [ ] No console errors, subscriptions cleaned.

