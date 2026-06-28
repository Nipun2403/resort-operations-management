# Specsheet: Front Desk Dashboard – Part 3

## 1. Purpose

- Replace the placeholder `openBookingModal` in the dashboard with the **BookingActionModalComponent**.
- Displays all booking details and provides core actions: Check‑In, Cancel, Extend Stay.
- Actions are guarded by confirmation dialogs and call specific API endpoints.
- The modal closes with `true` after any successful mutating action, or `undefined` otherwise, so the dashboard can refresh the movement table and summary cards.
- The modal is structured with future extensibility in mind (tabs in Parts 4 & 5 will wrap the current content inside a `MatTabGroup` with minimal refactoring).

## 2. Files to Create / Modify

| File                                                                                                       | Action                                       |
| ---------------------------------------------------------------------------------------------------------- | -------------------------------------------- |
| **New:** `src/app/features/front-desk/components/booking-action-modal/booking-action-modal.component.ts`   | Modal component                              |
| **New:** `src/app/features/front-desk/components/booking-action-modal/booking-action-modal.component.html` | Modal template                               |
| **New:** `src/app/features/front-desk/components/booking-action-modal/booking-action-modal.component.scss` | Modal styles                                 |
| **New:** `src/app/features/front-desk/components/extend-stay-dialog/extend-stay-dialog.component.ts`       | Sub‑dialog for extending stay                |
| **New:** `src/app/features/front-desk/components/extend-stay-dialog/extend-stay-dialog.component.html`     | Sub‑dialog template                          |
| **Modify:** `src/app/features/front-desk/pages/dashboard.component.ts`                                     | Wire up `openBookingModal` and refresh logic |
| **Modify:** `src/app/features/front-desk/pages/dashboard.component.html`                                   | No change (already passes `bookingSelected`) |

## 3. BookingActionModalComponent

### 3.1 Selector & Dependencies

- **Selector**: `app-booking-action-modal`
- **Standalone**: `true`
- **Imports** (exact list):
  - `CommonModule`
  - `MatDialogModule`
  - `MatButtonModule`
  - `MatIconModule`
  - `MatDividerModule`
  - `MatChipsModule`
  - `MatProgressSpinnerModule`
  - `MatSnackBarModule`
  - `ConfirmDialogComponent` (from `src/app/shared/components/confirm-dialog/confirm-dialog.component`)
  - `AlertComponent` (from `src/app/shared/components/alert/alert.component`)
  - `ExtendStayDialogComponent` (from `./extend-stay-dialog/extend-stay-dialog.component`)
- **Injected Data**: via `MAT_DIALOG_DATA` – `{ booking: Booking }`

### 3.2 State (all signals)

```ts
booking = signal<Booking>(this.data.booking);
loading = signal(false); // disables action buttons during API call
error = signal<string | null>(null); // inline error message
```

### 3.3 Template (exact)

```html
<h2 mat-dialog-title>Booking #{{ booking().id }}</h2>
<mat-dialog-content>
  <!-- Wrap content in a neutral container for future tabs -->
  <div class="modal-content">
    <div class="booking-details">
      <h3>Guest Information</h3>
      <p><strong>Name:</strong> {{ booking().guestName ?? '—' }}</p>
      <p><strong>Email:</strong> {{ booking().guestEmail ?? '—' }}</p>
      <p><strong>Guest Count:</strong> {{ booking().guestCount }}</p>
      <p><strong>Origin:</strong> {{ booking().origin ?? '—' }}</p>

      <mat-divider></mat-divider>

      <h3>Booking Details</h3>
      <p>
        <strong>Status:</strong>
        <span
          class="status-chip"
          [class]="booking().bookingStatus"
          >{{ booking().bookingStatus }}</span
        >
      </p>
      <p><strong>Check‑In:</strong> {{ booking().checkInDate }}</p>
      <p><strong>Check‑Out:</strong> {{ booking().checkOutDate }}</p>
      <p>
        <strong>Booked At:</strong> {{ booking().bookedAt | date:'medium' }}
      </p>

      <mat-divider></mat-divider>

      <h3>Rooms</h3>
      @if (booking().rooms && booking().rooms.length > 0) {
      <div class="rooms-list">
        @for (room of booking().rooms; track room.id) {
        <div class="room-card">
          <p>
            <strong>Room Number:</strong> {{ room.roomNumber ?? 'Unassigned' }}
          </p>
          <p><strong>Room Type ID:</strong> {{ room.roomTypeId }}</p>
          <p>
            <strong>Locked Price:</strong> {{ room.lockedInPrice | currency }}
          </p>
        </div>
        }
      </div>
      } @else {
      <p>No rooms assigned.</p>
      }

      <mat-divider></mat-divider>

      <h3>Amenities</h3>
      @if (booking().amenityIds && booking().amenityIds.length > 0) {
      <mat-chip-listbox>
        @for (id of booking().amenityIds; track id) {
        <mat-chip-option>Amenity #{{ id }}</mat-chip-option>
        }
      </mat-chip-listbox>
      } @else {
      <p>No amenities.</p>
      }
    </div>

    <!-- Action buttons – separated for future tab integration -->
    <div class="actions">
      @if (booking().bookingStatus === 'Booked') {
      <button
        mat-raised-button
        color="primary"
        (click)="checkIn()"
        [disabled]="loading()"
      >
        <mat-icon>login</mat-icon> Check‑In
      </button>
      <button
        mat-raised-button
        color="warn"
        (click)="cancelBooking()"
        [disabled]="loading()"
      >
        <mat-icon>cancel</mat-icon> Cancel Booking
      </button>
      } @if (booking().bookingStatus === 'CheckedIn') {
      <button
        mat-raised-button
        (click)="extendStay()"
        [disabled]="loading()"
      >
        <mat-icon>edit_calendar</mat-icon> Extend Stay
      </button>
      }
    </div>

    @if (error()) {
    <app-alert
      type="error"
      [message]="error()!"
      (closed)="error.set(null)"
    ></app-alert>
    }
  </div>
</mat-dialog-content>
<mat-dialog-actions align="end">
  <button
    mat-button
    mat-dialog-close
  >
    Close
  </button>
</mat-dialog-actions>
```

### 3.4 Logic (exact TypeScript)

```ts
import { Component, inject, signal } from "@angular/core";
import { MAT_DIALOG_DATA, MatDialogRef } from "@angular/material/dialog";
import { MatSnackBar } from "@angular/material/snack-bar";
import { MatDialog } from "@angular/material/dialog";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { DestroyRef } from "@angular/core";
import { finalize } from "rxjs";
import { BookingApiService } from "../../../services/booking-api.service";
import { Booking } from "../../../models/booking.model";
import { ConfirmDialogComponent } from "../../../../shared/components/confirm-dialog/confirm-dialog.component";
import { ExtendStayDialogComponent } from "../extend-stay-dialog/extend-stay-dialog.component";

@Component({
  selector: "app-booking-action-modal",
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatDividerModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    ConfirmDialogComponent,
    AlertComponent,
    ExtendStayDialogComponent,
  ],
  templateUrl: "./booking-action-modal.component.html",
  styleUrls: ["./booking-action-modal.component.scss"],
})
export class BookingActionModalComponent {
  data: { booking: Booking } = inject(MAT_DIALOG_DATA);
  private dialogRef = inject(MatDialogRef<BookingActionModalComponent>);
  private bookingApi = inject(BookingApiService);
  private snackBar = inject(MatSnackBar);
  private dialog = inject(MatDialog);
  private destroyRef = inject(DestroyRef);

  booking = signal<Booking>(this.data.booking);
  loading = signal(false);
  error = signal<string | null>(null);

  // ── Check‑In ────────────────────────────────────
  checkIn(): void {
    if (this.loading()) return;
    const confirmRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: "Confirm Check‑In",
        message: `Check in guest: ${this.booking().guestName}?`,
      },
    });
    confirmRef
      .afterClosed()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((confirmed) => {
        if (!confirmed) return;
        this.loading.set(true);
        this.error.set(null);
        this.bookingApi
          .checkIn(this.booking().id)
          .pipe(
            takeUntilDestroyed(this.destroyRef),
            finalize(() => this.loading.set(false)),
          )
          .subscribe({
            next: (updatedBooking) => {
              const roomNumber =
                updatedBooking.rooms?.[0]?.roomNumber || "assigned";
              this.snackBar.open(
                `Checked in successfully. Room: ${roomNumber}`,
                "Close",
                { duration: 3000 },
              );
              this.dialogRef.close(true); // signal parent to refresh
            },
            error: (err: any) => this.error.set(this.extractErrorMessage(err)),
          });
      });
  }

  // ── Cancel Booking ──────────────────────────────
  cancelBooking(): void {
    if (this.loading()) return;
    const confirmRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: "Cancel Booking",
        message: `Are you sure you want to cancel booking #${this.booking().id} for ${this.booking().guestName}? This cannot be undone.`,
      },
    });
    confirmRef
      .afterClosed()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((confirmed) => {
        if (!confirmed) return;
        this.loading.set(true);
        this.error.set(null);
        this.bookingApi
          .cancel(this.booking().id)
          .pipe(
            takeUntilDestroyed(this.destroyRef),
            finalize(() => this.loading.set(false)),
          )
          .subscribe({
            next: () => {
              this.snackBar.open("Booking cancelled.", "Close", {
                duration: 3000,
              });
              this.dialogRef.close(true);
            },
            error: (err: any) => this.error.set(this.extractErrorMessage(err)),
          });
      });
  }

  // ── Extend Stay ─────────────────────────────────
  extendStay(): void {
    if (this.loading()) return;
    const extendRef = this.dialog.open(ExtendStayDialogComponent, {
      data: {
        bookingId: this.booking().id,
        currentCheckOut: this.booking().checkOutDate,
      },
    });
    extendRef
      .afterClosed()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((result) => {
        if (result) {
          this.snackBar.open("Stay extended successfully.", "Close", {
            duration: 3000,
          });
          this.dialogRef.close(true);
        }
      });
  }

  private extractErrorMessage(err: any): string {
    if (typeof err === "string") return err;
    if (err?.error?.message) return err.error.message;
    if (err?.message) return err.message;
    return "An unexpected error occurred.";
  }
}
```

**API Methods Reference (to be implemented in `BookingApiService`):**

- `checkIn(id: number): Observable<Booking>` – `POST /api/v1/bookings/{id}/checkin`
- `cancel(id: number): Observable<void>` – `DELETE /api/v1/bookings/{id}/cancel` (as per Swagger)
- `extendStay(id: number, dto: { checkOutDate: string }): Observable<void>` – `PATCH /api/v1/bookings/{id}/extend-stay`

## 4. ExtendStayDialogComponent

### 4.1 API

- **Selector**: `app-extend-stay-dialog`
- **Standalone**: `true`
- **Imports**: `CommonModule`, `ReactiveFormsModule`, `MatDialogModule`, `MatButtonModule`, `MatDatepickerModule`, `MatNativeDateModule`, `MatFormFieldModule`, `MatInputModule`, `MatProgressSpinnerModule`, `AlertComponent`.
- **Injected Data**: `{ bookingId: number; currentCheckOut: string }` via `MAT_DIALOG_DATA`.

### 4.2 State

```ts
data: { bookingId: number; currentCheckOut: string } = inject(MAT_DIALOG_DATA);
private dialogRef = inject(MatDialogRef<ExtendStayDialogComponent>);
private bookingApi = inject(BookingApiService);
private destroyRef = inject(DestroyRef);

// Convert current check‑out string to Date for min validation
minDate = new Date(this.data.currentCheckOut.split('-').reverse().join('-')); // assumes dd-MM-yyyy format; safer: parse with dayjs or just use new Date() if ISO, but we'll handle generically.
// More robust: create a Date from the string using manual parsing.
minDate = this.parseDate(this.data.currentCheckOut);

private parseDate(dateStr: string): Date {
  const parts = dateStr.split('-');
  if (parts.length === 3) {
    // dd-MM-yyyy
    return new Date(+parts[2], +parts[1] - 1, +parts[0]);
  }
  // fallback
  return new Date(dateStr);
}

newCheckOut = new FormControl<Date | null>(null, { validators: Validators.required });
submitting = signal(false);
error = signal<string | null>(null);
```

### 4.3 Template

```html
<h2 mat-dialog-title>Extend Stay</h2>
<mat-dialog-content>
  <p>Current check‑out: {{ data.currentCheckOut }}</p>
  <mat-form-field appearance="outline">
    <mat-label>New check‑out date</mat-label>
    <input
      matInput
      [matDatepicker]="picker"
      [formControl]="newCheckOut"
      [min]="minDate"
    />
    <mat-datepicker-toggle
      matSuffix
      [for]="picker"
    ></mat-datepicker-toggle>
    <mat-datepicker #picker></mat-datepicker>
    @if (newCheckOut.invalid && newCheckOut.touched) {
    <mat-error
      >Please select a future date after the current check‑out.</mat-error
    >
    }
  </mat-form-field>
  @if (error()) {
  <app-alert
    type="error"
    [message]="error()!"
    (closed)="error.set(null)"
  ></app-alert>
  }
</mat-dialog-content>
<mat-dialog-actions align="end">
  <button
    mat-button
    mat-dialog-close
  >
    Cancel
  </button>
  <button
    mat-raised-button
    color="primary"
    (click)="submit()"
    [disabled]="newCheckOut.invalid || submitting()"
  >
    @if (submitting()) { <mat-spinner diameter="20"></mat-spinner> } Extend Stay
  </button>
</mat-dialog-actions>
```

### 4.4 Logic

```ts
submit(): void {
  if (this.submitting() || this.newCheckOut.invalid) return;
  this.submitting.set(true);
  this.error.set(null);
  const newDate = this.newCheckOut.value!;
  // Format as ISO string (backend expects ISO 8601 with time, but the PATCH endpoint from Swagger accepts a date-time string; we'll send ISO)
  const dto = { checkOutDate: newDate.toISOString() };
  this.bookingApi.extendStay(this.data.bookingId, dto).pipe(
    takeUntilDestroyed(this.destroyRef),
    finalize(() => this.submitting.set(false))
  ).subscribe({
    next: () => this.dialogRef.close(true),
    error: (err: any) => this.error.set(err.error?.message || err.message || 'Extend stay failed.')
  });
}
```

## 5. Dashboard Integration (final)

In `dashboard.component.ts` (already has `refreshTable` and `loadSummary`):

```ts
openBookingModal(booking: Booking): void {
  const dialogRef = this.dialog.open(BookingActionModalComponent, {
    data: { booking },
    width: '95vw',
    maxWidth: '700px',
    panelClass: 'booking-action-modal',
  });
  dialogRef.afterClosed().pipe(takeUntilDestroyed(this.destroyRef)).subscribe(result => {
    if (result === true) {
      this.refreshTable.update(n => n + 1);
      this.loadSummary();
    }
  });
}
```

## 6. Responsive & Styling (SCSS)

Provide basic structure in `booking-action-modal.component.scss`:

```scss
.modal-content {
  max-height: 70vh;
  overflow-y: auto;
}
.booking-details {
  h3 {
    margin-top: 16px;
    font-size: 1.1rem;
  }
  p {
    margin: 8px 0;
  }
}
.rooms-list {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
}
.room-card {
  flex: 1 1 200px;
  border: 1px solid #ddd;
  border-radius: 8px;
  padding: 12px;
  background: #fafafa;
}
.actions {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
  margin-top: 24px;
  button {
    min-width: 140px;
  }
}
.status-chip {
  padding: 2px 8px;
  border-radius: 12px;
  font-size: 0.85rem;
  &.CheckedIn {
    background: #c8e6c9;
    color: #2e7d32;
  }
  &.CheckedOut {
    background: #ffe0b2;
    color: #e65100;
  }
  &.Booked {
    background: #b3e5fc;
    color: #0277bd;
  }
  &.Cancelled {
    background: #ffcdd2;
    color: #c62828;
  }
}
```

## 7. Error Handling Strategy (explicit)

- **Recoverable API failures** are shown in an inline `<app-alert>` (dismissible) so the modal stays open.
- **Success messages** use `MatSnackBar` (automatic dismiss) so the user sees a quick confirmation.
- Each action has its own error signal, preventing cross‑action contamination.

## 8. Prevent Duplicate Submissions

- The `loading` signal disables all action buttons.
- Methods guard with `if (this.loading()) return;` to prevent accidental double calls from programmatic triggers.

## 9. Dialog Result Contract

- The modal closes with `true` after a successful mutation (check‑in, cancel, extend).
- The modal closes with `undefined` when the user clicks the close button or cancels a confirmation.
- The dashboard uses this exact contract to trigger refreshes.

## 10. Self‑Review Checklist (Part 3)

- [ ] All imports match template usage (finalize, MatChips, AlertComponent, etc.).
- [ ] Date formatting is deterministic (ISO string used for API call).
- [ ] Defensive rendering `?? '—'` for potentially nullable fields.
- [ ] Extend stay date picker disallows past dates and current check‑out.
- [ ] Check‑In shows room number in snackbar and closes modal.
- [ ] Cancel shows confirmation, calls DELETE endpoint, and refreshes.
- [ ] Extend stay successfully updates the stay and refreshes.
- [ ] Error alerts appear for API failures.
- [ ] Dashboard refresh logic works as specified.
- [ ] Modal layout is styled and responsive.
- [ ] No unused subscriptions; `takeUntilDestroyed` used consistently.

## 11. Integration Notes for Future Parts

- The `modal-content` wrapper and separation of `booking-details` and `actions` makes it easy to wrap them in a `MatTabGroup` later without rewriting the whole template.
- The `booking` signal remains the single source of truth; later parts can add additional signals for billing/room service without conflict.
- The `close(true)` contract will be extended in Part 5 when the checkout flow is added.

