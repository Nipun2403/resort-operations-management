# Specsheet: Customer Dashboard

## 1. Purpose
- Replace the `PlaceholderCustomerDashboardComponent` with the real Customer Dashboard.
- Display a personalised welcome message using the user’s first name from the JWT.
- Show current (CheckedIn) and upcoming (Booked) booking cards.
- Provide quick‑action buttons to request housekeeping or maintenance for the active booking.
- Responsive: cards stack on mobile, side‑by‑side on desktop.

## 2. Route & Navigation
- Path: `/user/dashboard` (lazy‑loaded in Customer Shell).
- **Overwrite** the placeholder file: `src/app/features/user/pages/dashboard.component.ts`.

## 3. Authorization
- Already protected by `customerGuard` from parent route.

## 4. Component API (CustomerDashboardComponent)
- **Selector**: `app-customer-dashboard` (exact placeholder match)
- **Standalone**: `true`
- **Imports**: `CommonModule`, `ReactiveFormsModule`, `MatCardModule`, `MatButtonModule`, `MatIconModule`, `MatProgressSpinnerModule`, `MatDividerModule`, `MatDialogModule`, `MatFormFieldModule`, `MatInputModule`, `MatSnackBarModule`, `AlertComponent`, `BookingApiService`, `HousekeepingApiService`, `MaintenanceApiService`, `AuthApiService`.
- **Exact import paths** (use verbatim):
  ```ts
  import { CommonModule } from '@angular/common';
  import { Component, inject, signal, computed } from '@angular/core';
  import { ReactiveFormsModule, FormControl, Validators } from '@angular/forms';
  import { MatCardModule } from '@angular/material/card';
  import { MatButtonModule } from '@angular/material/button';
  import { MatIconModule } from '@angular/material/icon';
  import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
  import { MatDividerModule } from '@angular/material/divider';
  import { MatDialogModule, MatDialog } from '@angular/material/dialog';
  import { MatFormFieldModule } from '@angular/material/form-field';
  import { MatInputModule } from '@angular/material/input';
  import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
  import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
  import { DestroyRef } from '@angular/core';
  import { finalize, forkJoin } from 'rxjs';
  import { AuthApiService } from '../../services/auth-api.service';
  import { BookingApiService } from '../../services/booking-api.service';
  import { HousekeepingApiService } from '../../services/housekeeping-api.service';
  import { MaintenanceApiService } from '../../services/maintenance-api.service';
  import { AuthMeResponse, Claim } from '../../models/auth-me-response.model';
  import { Booking } from '../../models/booking.model';
  import { AlertComponent } from '../../../../shared/components/alert/alert.component';
  ```

- **Template** (exact):
  ```html
  <div class="dashboard">
    <!-- Welcome message -->
    @if (loading()) {
      <mat-spinner diameter="40"></mat-spinner>
    } @else if (error()) {
      <app-alert type="error" [message]="error()!" (closed)="error.set(null)">
        <button mat-button (click)="loadDashboard()">Retry</button>
      </app-alert>
    } @else {
      <h1>Welcome back, Mr {{ firstName() }}</h1>

      <div class="booking-cards">
        <!-- Current Booking (CheckedIn) -->
        @if (currentBooking()) {
          <mat-card class="booking-card current">
            <mat-card-header>
              <mat-card-title>Current Stay</mat-card-title>
              <mat-card-subtitle>Room: {{ getRoomNumbers(currentBooking()!) }}</mat-card-subtitle>
            </mat-card-header>
            <mat-card-content>
              <p><strong>Check‑in:</strong> {{ currentBooking()!.checkInDate }}</p>
              <p><strong>Check‑out:</strong> {{ currentBooking()!.checkOutDate }}</p>
              <p><strong>Status:</strong> {{ currentBooking()!.bookingStatus }}</p>
            </mat-card-content>
            <mat-card-actions>
              <button mat-raised-button color="accent" (click)="openServiceRequest('housekeeping')">
                <mat-icon>cleaning_services</mat-icon> Request Housekeeping
              </button>
              <button mat-raised-button color="warn" (click)="openServiceRequest('maintenance')">
                <mat-icon>build</mat-icon> Request Maintenance
              </button>
            </mat-card-actions>
          </mat-card>
        } @else {
          <mat-card class="booking-card no-booking">
            <mat-card-content>
              <p>No active stay right now.</p>
            </mat-card-content>
          </mat-card>
        }

        <!-- Upcoming Booking (Booked) -->
        @if (upcomingBooking()) {
          <mat-card class="booking-card upcoming">
            <mat-card-header>
              <mat-card-title>Upcoming Stay</mat-card-title>
            </mat-card-header>
            <mat-card-content>
              <p><strong>Check‑in:</strong> {{ upcomingBooking()!.checkInDate }}</p>
              <p><strong>Check‑out:</strong> {{ upcomingBooking()!.checkOutDate }}</p>
              <p><strong>Status:</strong> {{ upcomingBooking()!.bookingStatus }}</p>
            </mat-card-content>
          </mat-card>
        } @else {
          <mat-card class="booking-card no-booking">
            <mat-card-content>
              <p>No upcoming bookings.</p>
            </mat-card-content>
          </mat-card>
        }
      </div>
    }
  </div>
  ```

## 5. State Management (All Signals)
```ts
firstName = signal('');
loading = signal(false);
error = signal<string | null>(null);
currentBooking = signal<Booking | null>(null);
upcomingBooking = signal<Booking | null>(null);

// Service requests
private dialog = inject(MatDialog);
private snackBar = inject(MatSnackBar);
private authApi = inject(AuthApiService);
private bookingApi = inject(BookingApiService);
private housekeepingApi = inject(HousekeepingApiService);
private maintenanceApi = inject(MaintenanceApiService);
private destroyRef = inject(DestroyRef);
```

## 6. Data Flow & API Calls

### 6.1 Extract User Details
Call `GET /auth/me`, parse claims to get first name and email. Store `firstName` signal.

```ts
ngOnInit(): void {
  this.loadDashboard();
}

loadDashboard(): void {
  this.loading.set(true);
  this.error.set(null);

  // 1. Get user info
  this.authApi.getMe().pipe(
    takeUntilDestroyed(this.destroyRef),
  ).subscribe({
    next: (me: AuthMeResponse) => {
      const firstNameClaim = me.claims?.find(c => c.type === 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname')?.value ?? '';
      const emailClaim = me.claims?.find(c => c.type === 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name')?.value ?? '';

      this.firstName.set(firstNameClaim);

      // 2. Fetch current and upcoming bookings using the email
      if (emailClaim) {
        this.fetchBookings(emailClaim);
      } else {
        this.loading.set(false);
        // no email, but we can still show no bookings
      }
    },
    error: (err: any) => {
      this.error.set(this.extractErrorMessage(err));
      this.loading.set(false);
    }
  });
}

private fetchBookings(email: string): void {
  const current$ = this.bookingApi.getAll({ guestQuery: email, status: 'CheckedIn', pageNumber: 1, pageSize: 1, sortBy: 'bookedAt', sortDescending: true });
  const upcoming$ = this.bookingApi.getAll({ guestQuery: email, status: 'Booked', pageNumber: 1, pageSize: 1, sortBy: 'checkInDate', sortDescending: false });

  forkJoin([current$, upcoming$]).pipe(
    takeUntilDestroyed(this.destroyRef),
    finalize(() => this.loading.set(false))
  ).subscribe({
    next: ([currentRes, upcomingRes]) => {
      this.currentBooking.set(currentRes.data.length > 0 ? currentRes.data[0] : null);
      this.upcomingBooking.set(upcomingRes.data.length > 0 ? upcomingRes.data[0] : null);
    },
    error: (err: any) => this.error.set(this.extractErrorMessage(err))
  });
}
```

### 6.2 Quick Service Request
Method `openServiceRequest(type: 'housekeeping' | 'maintenance')`:
- If no current booking or current booking has no room, do nothing (button disabled logic in template).
- Open a dialog with a form: description (required), location pre‑filled with room number (readonly). Room ID is taken from `currentBooking.rooms[0].roomId` (must exist).
- On submit, call the respective endpoint:
  - Housekeeping: `POST /api/v1/housekeeping/trigger/{roomId}` with `{ description }`
  - Maintenance: `POST /api/v1/maintenance/trigger/{roomId}` with `{ description }`
- Show success snackbar on success, error on failure.

We'll create a simple inline component for the dialog, or use `MatDialog` with a generic `RequestServiceDialogComponent`. To keep it contained, we can define a standalone dialog component in the same file (or in the same folder). We'll outline the dialog component.

**RequestServiceDialogComponent** (standalone):
- **Selector**: `app-request-service-dialog`
- **Template**: form with description textarea, cancel/submit buttons.
- **Data**: injected via `MAT_DIALOG_DATA` containing `roomNumber: string`, `roomId: number`, `type: 'housekeeping' | 'maintenance'`.
- On submit, validate description not empty, then close dialog with `{ description }`.

In the dashboard component, after closing the dialog, if result is not null, call API.

## 7. UI States
- **Loading**: spinner.
- **Error**: alert with retry.
- **Success**: dashboard content as described.
- **No bookings**: cards show appropriate empty messages.
- **Service request button**: Only enabled if currentBooking exists and contains a room with roomId. If not, buttons can be hidden or disabled. We'll show buttons only when currentBooking exists.

## 8. Responsive Behaviour
- `.booking-cards` use flexbox with `flex-wrap: wrap; gap: 16px;`.
- Each card `flex: 1 1 300px;` so they stack on narrow screens.
- Welcome message uses `h1` styled appropriately.

## 9. Accessibility
- Cards have proper headings.
- Buttons have aria labels.
- Service request dialog traps focus.

## 10. Integration Notes
- **Overwrite** placeholder: `src/app/features/user/pages/dashboard.component.ts`.
- `AuthApiService` must exist (reuse from admin or create). It should have `getMe(): Observable<AuthMeResponse>`.
- `BookingApiService` must have `getAll(params)` method (same as admin).
- `HousekeepingApiService` and `MaintenanceApiService` must have `trigger(roomId, body)` methods (already used in admin).
- The `RequestServiceDialogComponent` should be created as a standalone component in the same folder or a shared location. For simplicity, create it in `src/app/features/user/components/request-service-dialog.component.ts`.

## 11. File Structure
```
src/app/features/user/
  pages/
    dashboard.component.ts   (overwrite)
    dashboard.component.html
    dashboard.component.scss
  components/
    request-service-dialog.component.ts
    request-service-dialog.component.html
  services/   (if not already imported from admin, but can reuse)
```

## 12. Self‑Review Checklist
- [ ] Dashboard loads, shows welcome message with first name.
- [ ] Current booking card appears if user has a CheckedIn booking, with room numbers, dates, and action buttons.
- [ ] Upcoming booking card appears if user has a Booked booking.
- [ ] Clicking "Request Housekeeping" opens a dialog with pre‑filled room number; submitting calls the correct API and shows success/error snackbar.
- [ ] Clicking "Request Maintenance" works similarly.
- [ ] If no current booking, the "No active stay" message is shown and no buttons appear.
- [ ] Responsive: cards stack on mobile.
- [ ] Loading/error states function correctly.
- [ ] No console errors, subscriptions cleaned.

## 13. Implementation Constraints
- Angular 18 control flow, standalone components, signals, `takeUntilDestroyed`.
- Overwrite placeholder; do not rename.
- Use existing API services if available; otherwise create minimal ones.
- The dialog component must be standalone and use `MAT_DIALOG_DATA`.
- All API calls must use `extractErrorMessage` helper for error handling.