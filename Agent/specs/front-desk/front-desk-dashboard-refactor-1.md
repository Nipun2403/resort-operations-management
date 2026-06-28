# Specsheet: Front Desk Dashboard Refactor – Part 1 (Dashboard)

## 1. Purpose

- Refactor the Front Desk Dashboard to remove the complex modal-based workflow and replace it with a clean navigation to a dedicated Guest Details page.
- **Keep** the summary cards (arrivals, departures, active tickets) and their refresh logic exactly as built in Part 1 of the original dashboard.
- **Remove** the `MovementTableComponent` entirely.
- **Introduce** a new **Search Results Table** that groups bookings by guest email, shows one row per guest, and navigates to `/operations/front-desk/guest/:encodedEmail` on row click.
- **Move** the **Today’s Movement Table** below the search table and enforce strict status filters: arrivals only `bookingStatus=Booked` & `movementStatus=incoming`, departures only `bookingStatus=CheckedIn` & `movementStatus=outgoing`.
- **Add** a **Create Internal Ticket** button that opens the existing `InternalTicketPanelComponent` in a `MatDialog`.

## 2. Files to Modify (exact list)

| File                                                                            | Action                                                                                                                                                                                                                               |
| ------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| `src/app/features/front-desk/pages/dashboard.component.ts`                      | Remove all references to `MovementTableComponent`. Add new signals and logic for search grouping, movement table with strict filters, internal ticket button, navigation to guest page. Keep all existing summary card logic intact. |
| `src/app/features/front-desk/pages/dashboard.component.html`                    | Replace `<app-movement-table>` with the new search results table and movement table. Add internal ticket button.                                                                                                                     |
| `src/app/features/front-desk/pages/dashboard.component.scss`                    | Add responsive styles for the two tables and button.                                                                                                                                                                                 |
| **Delete** `src/app/features/front-desk/components/movement-table/` (all files) | This component is no longer used.                                                                                                                                                                                                    |

## 3. Dashboard Component – Exact Changes

### 3.1 Imports to Adjust

- **Remove** import of `MovementTableComponent`.
- **Add** imports: `InternalTicketPanelComponent` (from `../components/booking-action-modal/internal-ticket-panel/internal-ticket-panel.component`), `Router` (from `@angular/router`), `MatDialog` and `MatDialogModule` if not already imported.

### 3.2 New Signals & Form Controls

Add the following to the existing component class (do **not** duplicate existing signals like `arrivalsCount`, etc.):

```typescript
// Search
searchControl = new FormControl("", { nonNullable: true });
searchResults = signal<SearchResult[]>([]);
searchLoading = signal(false);
searchError = signal<string | null>(null);

// Movement table (today's arrivals/departures)
movementData = signal<Booking[]>([]);
movementTotal = signal(0);
movementLoading = signal(false);
movementError = signal<string | null>(null);
movementPage = signal(0);
movementPageSize = signal(10);
movementActiveFilter = new FormControl<"arrivals" | "departures">("arrivals", {
  nonNullable: true,
});

interface SearchResult {
  guestName: string;
  guestEmail: string;
  currentStatus: string;
  bookings: Booking[];
}
```

### 3.3 Logic to Implement

**Initialization:**  
Call `this.fetchMovement();` in `ngOnInit()` alongside existing `loadSummary()`.  
Set up a subscription to `searchControl.valueChanges` with debounce (300ms) and distinctUntilChanged, calling `this.onSearch(value.trim())`. Use `takeUntilDestroyed(this.destroyRef)`.

**Search handler:**

```typescript
private onSearch(query: string): void {
  if (!query) {
    this.searchResults.set([]);
    return;
  }
  this.searchLoading.set(true);
  this.bookingApi.getAll({ guestQuery: query, pageSize: 200 }).pipe(
    takeUntilDestroyed(this.destroyRef),
    finalize(() => this.searchLoading.set(false))
  ).subscribe({
    next: res => {
      const grouped = this.groupByEmail(res.data);
      this.searchResults.set(grouped);
    },
    error: (err: any) => this.searchError.set(this.extractErrorMessage(err))
  });
}

private groupByEmail(bookings: Booking[]): SearchResult[] {
  const map = new Map<string, Booking[]>();
  bookings.forEach(b => {
    if (!b.guestEmail) return;
    const arr = map.get(b.guestEmail) || [];
    arr.push(b);
    map.set(b.guestEmail, arr);
  });
  return Array.from(map.entries()).map(([email, bookings]) => {
    const statuses = bookings.map(b => b.bookingStatus);
    let currentStatus = 'Cancelled';
    if (statuses.includes('CheckedIn')) currentStatus = 'CheckedIn';
    else if (statuses.includes('Booked')) currentStatus = 'Booked';
    else if (statuses.includes('CheckedOut')) currentStatus = 'CheckedOut';
    return {
      guestName: bookings[0].guestName,
      guestEmail: email,
      currentStatus,
      bookings
    };
  });
}
```

**Movement table fetch:**

```typescript
fetchMovement(): void {
  this.movementLoading.set(true);
  const params: any = {
    pageNumber: this.movementPage() + 1,
    pageSize: this.movementPageSize(),
  };
  if (this.movementActiveFilter.value === 'arrivals') {
    params.movementStatus = 'incoming';
    params.bookingStatus = 'Booked';
  } else {
    params.movementStatus = 'outgoing';
    params.bookingStatus = 'CheckedIn';
  }
  this.bookingApi.getAll(params).pipe(
    takeUntilDestroyed(this.destroyRef),
    finalize(() => this.movementLoading.set(false))
  ).subscribe({
    next: res => {
      this.movementData.set(res.data);
      this.movementTotal.set(res.totalCount);
    },
    error: (err: any) => this.movementError.set(this.extractErrorMessage(err))
  });
}
```

**Navigation:**

```typescript
constructor(private router: Router) {} // or inject

navigateToGuest(email: string): void {
  const encoded = encodeURIComponent(email);
  this.router.navigate(['/operations/front-desk/guest', encoded]);
}
```

**Internal Ticket dialog:**

```typescript
openInternalTicket(): void {
  this.dialog.open(InternalTicketPanelComponent, {
    width: '95vw',
    maxWidth: '500px',
  });
}
```

### 3.4 Template (Replace existing `<app-movement-table>` block completely)

```html
<!-- Internal Ticket button (floating right) -->
<div class="top-actions">
  <button
    mat-raised-button
    color="accent"
    (click)="openInternalTicket()"
  >
    <mat-icon>add_task</mat-icon> Create Internal Ticket
  </button>
</div>

<!-- Search Bar -->
<div class="search-box">
  <mat-form-field
    appearance="outline"
    class="search-field"
  >
    <mat-label>Search guest name or email</mat-label>
    <input
      matInput
      [formControl]="searchControl"
    />
    <mat-icon matSuffix>search</mat-icon>
  </mat-form-field>
</div>

<!-- Search Results Table -->
<h2>Search Results</h2>
@if (searchLoading()) {
<mat-spinner diameter="30"></mat-spinner>
} @else if (searchError()) {
<app-alert
  type="error"
  [message]="searchError()!"
  (closed)="searchError.set(null)"
></app-alert>
} @else if (searchResults().length > 0) {
<table
  mat-table
  [dataSource]="searchResults()"
  class="search-table"
>
  <ng-container matColumnDef="guestName">
    <th
      mat-header-cell
      *matHeaderCellDef
    >
      Guest Name
    </th>
    <td
      mat-cell
      *matCellDef="let r"
    >
      {{ r.guestName }}
    </td>
  </ng-container>
  <ng-container matColumnDef="guestEmail">
    <th
      mat-header-cell
      *matHeaderCellDef
    >
      Email
    </th>
    <td
      mat-cell
      *matCellDef="let r"
    >
      {{ r.guestEmail }}
    </td>
  </ng-container>
  <ng-container matColumnDef="currentStatus">
    <th
      mat-header-cell
      *matHeaderCellDef
    >
      Current Status
    </th>
    <td
      mat-cell
      *matCellDef="let r"
    >
      {{ r.currentStatus }}
    </td>
  </ng-container>
  <ng-container matColumnDef="actions">
    <th
      mat-header-cell
      *matHeaderCellDef
    >
      Actions
    </th>
    <td
      mat-cell
      *matCellDef="let r"
    >
      <button
        mat-icon-button
        (click)="navigateToGuest(r.guestEmail)"
        aria-label="View guest details"
      >
        <mat-icon>visibility</mat-icon>
      </button>
    </td>
  </ng-container>
  <tr
    mat-header-row
    *matHeaderRowDef="['guestName','guestEmail','currentStatus','actions']"
  ></tr>
  <tr
    mat-row
    *matRowDef="let row; columns: ['guestName','guestEmail','currentStatus','actions']"
    class="clickable-row"
    (click)="navigateToGuest(row.guestEmail)"
  ></tr>
</table>
} @else { @if (searchControl.value) {
<p>No results found.</p>
} }

<!-- Today's Movement Table -->
<h2>Today’s Movement</h2>
<div class="movement-controls">
  <mat-button-toggle-group
    [formControl]="movementActiveFilter"
    (change)="onMovementToggle()"
  >
    <mat-button-toggle value="arrivals">Arrivals</mat-button-toggle>
    <mat-button-toggle value="departures">Departures</mat-button-toggle>
  </mat-button-toggle-group>
</div>

@if (movementLoading() && movementData().length === 0) {
<mat-spinner diameter="30"></mat-spinner>
} @else if (movementError()) {
<app-alert
  type="error"
  [message]="movementError()!"
  (closed)="movementError.set(null)"
></app-alert>
} @else if (movementData().length > 0) {
<table
  mat-table
  [dataSource]="movementData()"
  class="movement-table"
>
  <ng-container matColumnDef="guestName">
    <th
      mat-header-cell
      *matHeaderCellDef
    >
      Guest Name
    </th>
    <td
      mat-cell
      *matCellDef="let b"
    >
      {{ b.guestName }}
    </td>
  </ng-container>
  <ng-container matColumnDef="room">
    <th
      mat-header-cell
      *matHeaderCellDef
    >
      Room
    </th>
    <td
      mat-cell
      *matCellDef="let b"
    >
      {{ getRoomNumbers(b) || 'Unassigned' }}
    </td>
  </ng-container>
  <ng-container matColumnDef="status">
    <th
      mat-header-cell
      *matHeaderCellDef
    >
      Status
    </th>
    <td
      mat-cell
      *matCellDef="let b"
    >
      {{ b.bookingStatus }}
    </td>
  </ng-container>
  <ng-container matColumnDef="actions">
    <th
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
        (click)="navigateToGuest(b.guestEmail)"
        aria-label="View guest details"
      >
        <mat-icon>visibility</mat-icon>
      </button>
    </td>
  </ng-container>
  <tr
    mat-header-row
    *matHeaderRowDef="['guestName','room','status','actions']"
  ></tr>
  <tr
    mat-row
    *matRowDef="let row; columns: ['guestName','room','status','actions']"
    (click)="navigateToGuest(row.guestEmail)"
    class="clickable-row"
  ></tr>
</table>
<mat-paginator
  [length]="movementTotal()"
  [pageIndex]="movementPage()"
  [pageSize]="movementPageSize()"
  [pageSizeOptions]="[10,25,50]"
  (page)="onMovementPageChange($event)"
></mat-paginator>
} @else {
<p>No {{ movementActiveFilter.value }} today.</p>
}
```

**Methods for movement table:**

```typescript
onMovementToggle(): void {
  this.movementPage.set(0);
  this.fetchMovement();
}
onMovementPageChange(event: PageEvent): void {
  this.movementPage.set(event.pageIndex);
  this.movementPageSize.set(event.pageSize);
  this.fetchMovement();
}
getRoomNumbers(booking: Booking): string {
  return booking.rooms?.filter(r => r.roomNumber).map(r => r.roomNumber).join(', ') || '';
}
```

### 3.5 Remove Old Code

- Delete the `refreshTable` signal and all references to it (it was used to refresh the old movement table; the new movement table does not use an external refresh signal).
- Remove the old `openBookingModal` method entirely.
- Remove the `bookingSelected` event handling from the template.

### 3.6 SCSS Additions

```scss
.top-actions {
  display: flex;
  justify-content: flex-end;
  margin-bottom: 12px;
}
.search-box {
  margin-bottom: 16px;
}
.search-field {
  width: 100%;
  max-width: 400px;
}
.clickable-row {
  cursor: pointer;
  &:hover {
    background-color: rgba(0, 0, 0, 0.04);
  }
}
```

### 3.7 Remove `MovementTableComponent`

Delete the entire folder `src/app/features/front-desk/components/movement-table/`. No other component references it after this refactor.

## 4. Self‑Review Checklist (Part 1)

- [ ] Summary cards still load and function correctly.
- [ ] Active Tickets dialog still works.
- [ ] Search input groups results by email, shows one row per guest, and navigates to guest page on click.
- [ ] Movement table only shows arrivals with status “Booked” and departures with status “CheckedIn”.
- [ ] Movement table pagination and toggle work.
- [ ] Create Internal Ticket button opens the dialog with the existing form.
- [ ] No references to the old `MovementTableComponent` remain.
- [ ] No horizontal overflow on mobile; tables scroll correctly.

---

# Specsheet: Front Desk Dashboard Refactor – Part 2 (Guest Details Page)

## 1. Purpose

- Replace the old multi-tab modal with a dedicated page at `/operations/front-desk/guest/:email`.
- The page provides a spacious interface with tabs: Overview, Bookings, Room Service, Billing.
- All booking actions (Check‑In, Cancel, Extend Stay, Check‑Out) are performed from this page.
- Back navigation returns to the dashboard.

## 2. Route Configuration

Add to `src/app/app.routes.ts` (inside the front-desk parent route):

```typescript
{
  path: 'guest/:email',
  loadComponent: () => import('./features/front-desk/pages/guest-details.component')
    .then(m => m.GuestDetailsComponent),
  canActivate: [frontDeskGuard]
}
```

The guard is already defined (`frontDeskGuard`). Ensure the import path for `frontDeskGuard` is correct.

## 3. GuestDetailsComponent – Exact Implementation

### 3.1 File to Create

- `src/app/features/front-desk/pages/guest-details.component.ts`
- `src/app/features/front-desk/pages/guest-details.component.html`
- `src/app/features/front-desk/pages/guest-details.component.scss`

### 3.2 Selector & Dependencies

- **Selector**: `app-guest-details`
- **Standalone**: `true`
- **Imports**:
  - `CommonModule`, `ReactiveFormsModule`, `RouterModule`
  - `MatTabsModule`, `MatCardModule`, `MatButtonModule`, `MatIconModule`, `MatDividerModule`
  - `MatTableModule`, `MatProgressSpinnerModule`
  - `MatSnackBarModule`, `MatDialogModule`
  - `ConfirmDialogComponent` (shared)
  - `AlertComponent` (shared)
  - `CheckoutDialogComponent` (from `../components/checkout-dialog/checkout-dialog.component`)
  - `ExtendStayDialogComponent` (from `../components/extend-stay-dialog/extend-stay-dialog.component`)
  - `RoomServiceTabComponent` (from `../components/booking-action-modal/room-service-tab/room-service-tab.component`)
  - `BillingTabComponent` (from `../components/booking-action-modal/billing-tab/billing-tab.component`)
  - `BookingApiService`, `BillingApiService`, `OrderApiService`, etc. (as needed)
  - `ActivatedRoute` (from `@angular/router`)

**Do NOT import** `InternalTicketPanelComponent` – it stays on the dashboard only.

### 3.3 Component State (All Signals)

```typescript
email = signal("");
bookings = signal<Booking[]>([]);
loading = signal(false);
error = signal<string | null>(null);

// For Room Service / Billing context – default to the first CheckedIn booking or the most recent
activeBooking = computed(() => {
  return (
    this.bookings().find((b) => b.bookingStatus === "CheckedIn") ||
    this.bookings()[0] ||
    null
  );
});
activeBookingId = computed(() => this.activeBooking()?.id ?? 0);
```

### 3.4 Data Fetching

In `ngOnInit`:

```typescript
private route = inject(ActivatedRoute);
private bookingApi = inject(BookingApiService);
// ... other injections

ngOnInit(): void {
  const encodedEmail = this.route.snapshot.paramMap.get('email') || '';
  const decodedEmail = decodeURIComponent(encodedEmail);
  this.email.set(decodedEmail);
  this.fetchBookings();
}

private fetchBookings(): void {
  this.loading.set(true);
  this.bookingApi.getAll({ guestQuery: this.email(), pageSize: 200 }).pipe(
    takeUntilDestroyed(this.destroyRef),
    finalize(() => this.loading.set(false))
  ).subscribe({
    next: res => this.bookings.set(res.data),
    error: (err: any) => this.error.set(this.extractErrorMessage(err))
  });
}
```

### 3.5 Template Structure

```html
<div class="guest-details">
  <!-- Header with back button -->
  <div class="header">
    <button
      mat-icon-button
      routerLink="/operations/front-desk/dashboard"
      aria-label="Back to Dashboard"
    >
      <mat-icon>arrow_back</mat-icon>
    </button>
    <h1>Guest: {{ email() }}</h1>
  </div>

  @if (loading()) {
  <mat-spinner diameter="40"></mat-spinner>
  } @else if (error()) {
  <app-alert
    type="error"
    [message]="error()!"
    (closed)="error.set(null)"
  >
    <button
      mat-button
      (click)="fetchBookings()"
    >
      Retry
    </button>
  </app-alert>
  } @else {
  <mat-tab-group>
    <mat-tab label="Overview">
      <div class="tab-content">
        <h3>Current Status: {{ getOverallStatus() }}</h3>
        <div class="booking-summary">
          @for (b of bookings(); track b.id) {
          <div class="booking-card">
            <p><strong>Booking #{{ b.id }}</strong></p>
            <p>Status: {{ b.bookingStatus }}</p>
            <p>Check‑in: {{ b.checkInDate }}</p>
            <p>Check‑out: {{ b.checkOutDate }}</p>
            <p>Rooms: {{ getRoomNumbers(b) }}</p>
          </div>
          }
        </div>
      </div>
    </mat-tab>

    <mat-tab label="Bookings">
      <div class="tab-content">
        @for (b of bookings(); track b.id) {
        <div class="booking-item">
          <p>
            <strong>ID:</strong> {{ b.id }} | <strong>Status:</strong> {{
            b.bookingStatus }} | <strong>Rooms:</strong> {{ getRoomNumbers(b) }}
          </p>
          <div class="actions">
            @if (b.bookingStatus === 'Booked') {
            <button
              mat-raised-button
              color="primary"
              (click)="checkIn(b)"
            >
              Check‑In
            </button>
            <button
              mat-raised-button
              color="warn"
              (click)="cancelBooking(b)"
            >
              Cancel
            </button>
            } @if (b.bookingStatus === 'CheckedIn') {
            <button
              mat-raised-button
              (click)="extendStay(b)"
            >
              Extend Stay
            </button>
            <button
              mat-raised-button
              color="accent"
              (click)="checkOut(b)"
            >
              Check‑Out
            </button>
            }
          </div>
        </div>
        }
      </div>
    </mat-tab>

    <mat-tab label="Room Service">
      <div class="tab-content">
        @if (activeBooking()) {
        <app-room-service-tab [booking]="activeBooking()" />
        } @else {
        <p>No active booking for room service.</p>
        }
      </div>
    </mat-tab>

    <mat-tab label="Billing">
      <div class="tab-content">
        @if (activeBookingId() > 0) {
        <app-billing-tab [bookingId]="activeBookingId()" />
        } @else {
        <p>No booking selected for billing.</p>
        }
      </div>
    </mat-tab>
  </mat-tab-group>
  }
</div>
```

### 3.6 Action Methods (Exactly as in the Old Modal, but Now in Page)

```typescript
private dialog = inject(MatDialog);
private snackBar = inject(MatSnackBar);
private bookingApi = inject(BookingApiService);
private billingApi = inject(BillingApiService);
private destroyRef = inject(DestroyRef);

getOverallStatus(): string {
  const statuses = this.bookings().map(b => b.bookingStatus);
  if (statuses.includes('CheckedIn')) return 'CheckedIn';
  if (statuses.includes('Booked')) return 'Booked';
  if (statuses.includes('CheckedOut')) return 'CheckedOut';
  return 'Cancelled';
}

getRoomNumbers(booking: Booking): string {
  return booking.rooms?.filter(r => r.roomNumber).map(r => r.roomNumber).join(', ') || 'Unassigned';
}

checkIn(booking: Booking): void {
  const confirmRef = this.dialog.open(ConfirmDialogComponent, {
    data: { title: 'Confirm Check‑In', message: `Check in guest: ${booking.guestName}?` }
  });
  confirmRef.afterClosed().pipe(takeUntilDestroyed(this.destroyRef)).subscribe(confirmed => {
    if (!confirmed) return;
    this.bookingApi.checkIn(booking.id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (updated) => {
        this.snackBar.open(`Checked in. Room: ${updated.rooms?.[0]?.roomNumber || 'assigned'}`, 'Close', { duration: 3000 });
        this.fetchBookings();
      },
      error: (err) => this.snackBar.open(this.extractErrorMessage(err), 'Close', { duration: 5000 })
    });
  });
}

cancelBooking(booking: Booking): void {
  const confirmRef = this.dialog.open(ConfirmDialogComponent, {
    data: { title: 'Cancel Booking', message: `Cancel booking #${booking.id} for ${booking.guestName}?` }
  });
  confirmRef.afterClosed().pipe(takeUntilDestroyed(this.destroyRef)).subscribe(confirmed => {
    if (!confirmed) return;
    this.bookingApi.cancel(booking.id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.snackBar.open('Booking cancelled.', 'Close', { duration: 3000 });
        this.fetchBookings();
      },
      error: (err) => this.snackBar.open(this.extractErrorMessage(err), 'Close', { duration: 5000 })
    });
  });
}

extendStay(booking: Booking): void {
  const extendRef = this.dialog.open(ExtendStayDialogComponent, {
    data: { bookingId: booking.id, currentCheckOut: booking.checkOutDate }
  });
  extendRef.afterClosed().pipe(takeUntilDestroyed(this.destroyRef)).subscribe(result => {
    if (result) {
      this.snackBar.open('Stay extended.', 'Close', { duration: 3000 });
      this.fetchBookings();
    }
  });
}

checkOut(booking: Booking): void {
  const checkoutRef = this.dialog.open(CheckoutDialogComponent, {
    data: { bookingId: booking.id },
    width: '95vw',
    maxWidth: '600px',
    disableClose: true,
  });
  checkoutRef.afterClosed().pipe(takeUntilDestroyed(this.destroyRef)).subscribe(result => {
    if (result === true) {
      this.snackBar.open('Check‑out successful.', 'Close', { duration: 3000 });
      this.fetchBookings();
    }
  });
}

private extractErrorMessage(err: any): string {
  if (typeof err === 'string') return err;
  if (err?.error?.message) return err.error.message;
  if (err?.message) return err.message;
  return 'An unexpected error occurred.';
}
```

### 3.7 Styling (SCSS)

```scss
.guest-details {
  padding: 16px;
  .header {
    display: flex;
    align-items: center;
    gap: 8px;
    margin-bottom: 16px;
  }
  .tab-content {
    padding-top: 16px;
  }
  .booking-item {
    border: 1px solid #ddd;
    border-radius: 8px;
    padding: 12px;
    margin-bottom: 12px;
    .actions {
      display: flex;
      gap: 8px;
      flex-wrap: wrap;
    }
  }
}
```

## 4. Self‑Review Checklist (Part 2)

- [ ] Guest page loads by decoded email, shows all bookings.
- [ ] Overview tab displays overall status and summary cards.
- [ ] Bookings tab lists every booking with correct action buttons based on status.
- [ ] Check‑In, Cancel, Extend Stay, Check‑Out work and refresh the bookings list.
- [ ] Room Service tab appears only when an active booking exists and functions correctly.
- [ ] Billing tab displays folio and allows payment.
- [ ] Back button returns to dashboard without breaking state.
- [ ] No old modal logic remains in dashboard; no references to the removed MovementTableComponent.
- [ ] All subscriptions cleaned up.

---

