# Specsheet: Billing & Receipts Page

## 1. Purpose

- Replace the `PlaceholderBillingReceiptsComponent` with the full Billing & Receipts page.
- Provides a toggle between two read‑only tables: **Bookings** and **Receipts**.
- Each table supports search, filters, sorting (server‑side), and pagination.
- Clicking a row opens a detail modal showing all available information for that record.
- No create, edit, or payment actions – purely informational.

## 2. Route & Navigation

- Path: `/operations/admin/oversight/billings-receipts` (lazy‑loaded in Admin Shell).
- **Overwrite** the placeholder file: `src/app/features/admin/pages/oversight/billing-receipts.component.ts`.

## 3. Authorization

- Inherits `adminGuard` from parent route.

## 4. Component API (BillingReceiptsComponent)

- **Selector**: `app-billing-receipts` (exact placeholder match)
- **Standalone**: `true`
- **Imports**: `CommonModule`, `ReactiveFormsModule`, `MatTableModule`, `MatSortModule`, `MatPaginatorModule`, `MatButtonToggleModule`, `MatButtonModule`, `MatIconModule`, `MatFormFieldModule`, `MatInputModule`, `MatSelectModule`, `MatDatepickerModule`, `MatNativeDateModule`, `MatProgressSpinnerModule`, `MatDialogModule`, `MatCardModule`, `MatDividerModule`, `MatChipsModule`, `AlertComponent`.
- **Exact import paths** (use these verbatim):

  ```ts
  import { CommonModule } from "@angular/common";
  import { Component, inject, signal } from "@angular/core";
  import { ReactiveFormsModule, FormControl } from "@angular/forms";
  import { MatTableModule } from "@angular/material/table";
  import { MatSortModule, MatSort, Sort } from "@angular/material/sort";
  import {
    MatPaginatorModule,
    MatPaginator,
    PageEvent,
  } from "@angular/material/paginator";
  import { MatButtonToggleModule } from "@angular/material/button-toggle";
  import { MatButtonModule } from "@angular/material/button";
  import { MatIconModule } from "@angular/material/icon";
  import { MatFormFieldModule } from "@angular/material/form-field";
  import { MatInputModule } from "@angular/material/input";
  import { MatSelectModule } from "@angular/material/select";
  import { MatDatepickerModule } from "@angular/material/datepicker";
  import { MatNativeDateModule } from "@angular/material/core";
  import { MatProgressSpinnerModule } from "@angular/material/progress-spinner";
  import { MatDialogModule, MatDialog } from "@angular/material/dialog";
  import { MatCardModule } from "@angular/material/card";
  import { MatDividerModule } from "@angular/material/divider";
  import { MatChipsModule } from "@angular/material/chips";
  import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
  import { DestroyRef } from "@angular/core";
  import { debounceTime, distinctUntilChanged, finalize } from "rxjs";
  import { BookingApiService } from "../../services/booking-api.service";
  import { BillingApiService } from "../../services/billing-api.service";
  import { Booking, BookingRoom } from "../../models/booking.model";
  import { Receipt } from "../../models/receipt.model";
  import { AlertComponent } from "../../../../shared/components/alert/alert.component";
  import { BookingDetailDialogComponent } from "./booking-detail-dialog.component";
  import { ReceiptDetailDialogComponent } from "./receipt-detail-dialog.component";
  ```

- **Template** (full – using ONLY Angular 18 control flow, no `*ngIf`):

```html
<div class="billing-receipts">
  <!-- Toggle -->
  <div class="toggle-row">
    <mat-button-toggle-group
      [formControl]="activeView"
      (change)="onViewToggle()"
      aria-label="View"
    >
      <mat-button-toggle value="bookings">Bookings</mat-button-toggle>
      <mat-button-toggle value="receipts">Receipts</mat-button-toggle>
    </mat-button-toggle-group>
  </div>

  <!-- Bookings View -->
  @if (activeView.value === 'bookings') {
  <div class="bookings-view">
    <!-- Search & Filters -->
    <div class="controls">
      <mat-form-field
        appearance="outline"
        class="search"
      >
        <mat-label>Search guest name or email</mat-label>
        <input
          matInput
          [formControl]="bookingSearch"
          (keyup)="onBookingSearchDebounced()"
        />
        <mat-icon matSuffix>search</mat-icon>
      </mat-form-field>
      <mat-form-field appearance="outline">
        <mat-label>Status</mat-label>
        <mat-select [formControl]="bookingStatus">
          <mat-option value="">All</mat-option>
          <mat-option value="Booked">Booked</mat-option>
          <mat-option value="CheckedIn">Checked In</mat-option>
          <mat-option value="CheckedOut">Checked Out</mat-option>
          <mat-option value="Cancelled">Cancelled</mat-option>
        </mat-select>
      </mat-form-field>
      @if (bookingStatus.value || bookingSearch.value) {
      <button
        mat-button
        (click)="clearBookingFilters()"
      >
        Clear Filters
      </button>
      }
    </div>

    <!-- Loading / Error / Content -->
    @if (bookingsLoading() && bookings().length === 0) {
    <div class="loading"><mat-spinner diameter="40"></mat-spinner></div>
    } @else if (bookingsError()) {
    <app-alert
      type="error"
      [message]="bookingsError()!"
      (closed)="bookingsError.set(null)"
    >
      <button
        mat-button
        (click)="fetchBookings()"
      >
        Retry
      </button>
    </app-alert>
    } @if (bookings().length > 0 || bookingsLoading()) { @if (bookingsLoading())
    {
    <mat-progress-bar mode="indeterminate"></mat-progress-bar>
    }
    <table
      mat-table
      [dataSource]="bookings()"
      matSort
      (matSortChange)="onBookingSort($event)"
      aria-label="Bookings"
    >
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
      <ng-container matColumnDef="guestName"
        ><th
          mat-header-cell
          *matHeaderCellDef
        >
          Guest
        </th>
        <td
          mat-cell
          *matCellDef="let b"
        >
          {{ b.guestName }}
        </td></ng-container
      >
      <ng-container matColumnDef="checkIn"
        ><th
          mat-header-cell
          *matHeaderCellDef
        >
          Check-In
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
          Check-Out
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
          <span
            class="status-chip"
            [class]="b.bookingStatus"
            >{{ b.bookingStatus }}</span
          >
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
            (click)="openBookingDetail(b)"
            aria-label="View booking"
          >
            <mat-icon>visibility</mat-icon>
          </button>
        </td></ng-container
      >
      <tr
        mat-header-row
        *matHeaderRowDef="['id','guestName','checkIn','checkOut','status','rooms','actions']"
      ></tr>
      <tr
        mat-row
        *matRowDef="let row; columns: ['id','guestName','checkIn','checkOut','status','rooms','actions']"
        (click)="openBookingDetail(row)"
        class="clickable-row"
      ></tr>
    </table>
    <mat-paginator
      [length]="bookingsTotal()"
      [pageIndex]="bookingPage()"
      [pageSize]="bookingPageSize()"
      [pageSizeOptions]="[10,25,50]"
      (page)="onBookingPage($event)"
    ></mat-paginator>
    } @else {
    <div class="empty-state"><p>No bookings found.</p></div>
    }
  </div>
  }

  <!-- Receipts View -->
  @if (activeView.value === 'receipts') {
  <div class="receipts-view">
    <!-- Date Filters -->
    <div class="controls">
      <mat-form-field appearance="outline">
        <mat-label>Start date</mat-label>
        <input
          matInput
          [matDatepicker]="recStartPicker"
          [formControl]="receiptStartDate"
        />
        <mat-datepicker-toggle
          matSuffix
          [for]="recStartPicker"
        ></mat-datepicker-toggle>
        <mat-datepicker #recStartPicker></mat-datepicker>
      </mat-form-field>
      <mat-form-field appearance="outline">
        <mat-label>End date</mat-label>
        <input
          matInput
          [matDatepicker]="recEndPicker"
          [formControl]="receiptEndDate"
        />
        <mat-datepicker-toggle
          matSuffix
          [for]="recEndPicker"
        ></mat-datepicker-toggle>
        <mat-datepicker #recEndPicker></mat-datepicker>
      </mat-form-field>
      <button
        mat-raised-button
        color="primary"
        (click)="applyReceiptDateFilter()"
      >
        Apply
      </button>
      <button
        mat-button
        (click)="clearReceiptDateFilter()"
      >
        Clear
      </button>
    </div>

    @if (receiptsLoading() && receipts().length === 0) {
    <div class="loading"><mat-spinner diameter="40"></mat-spinner></div>
    } @else if (receiptsError()) {
    <app-alert
      type="error"
      [message]="receiptsError()!"
      (closed)="receiptsError.set(null)"
    >
      <button
        mat-button
        (click)="fetchReceipts()"
      >
        Retry
      </button>
    </app-alert>
    } @if (receipts().length > 0 || receiptsLoading()) { @if (receiptsLoading())
    {
    <mat-progress-bar mode="indeterminate"></mat-progress-bar>
    }
    <table
      mat-table
      [dataSource]="receipts()"
      matSort
      (matSortChange)="onReceiptSort($event)"
      aria-label="Receipts"
    >
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
          *matCellDef="let r"
        >
          {{ r.id }}
        </td></ng-container
      >
      <ng-container matColumnDef="bookingId"
        ><th
          mat-header-cell
          *matHeaderCellDef
        >
          Booking ID
        </th>
        <td
          mat-cell
          *matCellDef="let r"
        >
          {{ r.bookingId }}
        </td></ng-container
      >
      <ng-container matColumnDef="amountPaid"
        ><th
          mat-header-cell
          *matHeaderCellDef
          mat-sort-header="amountPaid"
        >
          Amount
        </th>
        <td
          mat-cell
          *matCellDef="let r"
        >
          {{ r.amountPaid | currency }}
        </td></ng-container
      >
      <ng-container matColumnDef="paymentMethod"
        ><th
          mat-header-cell
          *matHeaderCellDef
        >
          Payment Method
        </th>
        <td
          mat-cell
          *matCellDef="let r"
        >
          {{ r.paymentMethod }}
        </td></ng-container
      >
      <ng-container matColumnDef="paidAt"
        ><th
          mat-header-cell
          *matHeaderCellDef
          mat-sort-header="paidAt"
        >
          Paid At
        </th>
        <td
          mat-cell
          *matCellDef="let r"
        >
          {{ r.paidAt | date:'medium' }}
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
          *matCellDef="let r"
        >
          <button
            mat-icon-button
            (click)="openReceiptDetail(r)"
            aria-label="View receipt"
          >
            <mat-icon>visibility</mat-icon>
          </button>
        </td></ng-container
      >
      <tr
        mat-header-row
        *matHeaderRowDef="['id','bookingId','amountPaid','paymentMethod','paidAt','actions']"
      ></tr>
      <tr
        mat-row
        *matRowDef="let row; columns: ['id','bookingId','amountPaid','paymentMethod','paidAt','actions']"
        (click)="openReceiptDetail(row)"
        class="clickable-row"
      ></tr>
    </table>
    <mat-paginator
      [length]="receiptsTotal()"
      [pageIndex]="receiptPage()"
      [pageSize]="receiptPageSize()"
      [pageSizeOptions]="[10,25,50]"
      (page)="onReceiptPage($event)"
    ></mat-paginator>
    } @else {
    <div class="empty-state"><p>No receipts found.</p></div>
    }
  </div>
  }
</div>
```

## 5. State Management (All Signals)

**Rule:** Signals are the canonical state. `FormControl` instances are only for UI input; changes are consumed via value changes and written to signals.

```ts
// Active view toggle (UI input)
activeView = new FormControl<'bookings' | 'receipts'>('bookings', { nonNullable: true });

// Bookings state (canonical signals)
bookings = signal<Booking[]>([]);
bookingsTotal = signal(0);
bookingsLoading = signal(false);
bookingsError = signal<string | null>(null);
bookingPage = signal(0);
bookingPageSize = signal(10);
bookingSortField = signal('bookedAt');
bookingSortDescending = signal(true);
// UI inputs
bookingSearch = new FormControl('', { nonNullable: true });
bookingStatus = new FormControl('', { nonNullable: true });
// debounce subject
private bookingSearchSub: any;

// Receipts state (canonical signals)
receipts = signal<Receipt[]>([]);
receiptsTotal = signal(0);
receiptsLoading = signal(false);
receiptsError = signal<string | null>(null);
receiptPage = signal(0);
receiptPageSize = signal(10);
receiptSortField = signal('id');
receiptSortDescending = signal(true);
// UI inputs
receiptStartDate = new FormControl<Date | null>(null);
receiptEndDate = new FormControl<Date | null>(null);

private readonly STORAGE_KEY = 'billingReceiptsState';
```

## 6. Data Flow & API Calls

### Services

- `BookingApiService` – `getAll(params: { status?, guestQuery?, pageNumber, pageSize, sortBy, sortDescending }): Observable<{ totalCount: number, data: Booking[] }>`
- `BillingApiService` – `getReceipts(params: { startDate?, endDate?, pageNumber, pageSize, sortBy, sortDescending }): Observable<{ totalCount: number, data: Receipt[] }>`

### DTOs / Models

```ts
// booking.model.ts (exact)
export interface BookingRoom {
  id: number;
  bookingId: number;
  roomTypeId: number;
  roomId: number | null;
  roomNumber: string | null;
  lockedInPrice: number;
}
export interface Booking {
  id: number;
  guestCount: number;
  rooms: BookingRoom[];
  guestName: string;
  guestEmail: string;
  checkInDate: string; // "dd-MM-yyyy"
  checkOutDate: string; // "dd-MM-yyyy"
  bookingStatus: "Booked" | "CheckedIn" | "CheckedOut" | "Cancelled";
  userId: number | null;
  origin: "WalkIn" | "RegisteredUser" | "Guest";
  bookedAt: string; // ISO 8601
  amenityIds: number[];
}

// receipt.model.ts (exact)
export interface Receipt {
  id: number;
  bookingId: number;
  amountPaid: number;
  paymentMethod: string;
  transactionId: string;
  paidAt: string; // ISO 8601
}
```

### API Error Handling

**Exact error shape from ASP.NET Core:**

```json
{ "message": "...", "errors": { ... } }
```

**Error extraction helper (used in every subscription):**

```ts
private extractErrorMessage(err: any): string {
  if (typeof err === 'string') return err;
  if (err?.error?.message) return err.error.message;
  if (err?.message) return err.message;
  return 'An unexpected error occurred.';
}
```

**Example usage:**

```ts
error: (err: any) => this.bookingsError.set(this.extractErrorMessage(err));
```

### Component Methods (exact code)

We only show key differences from previous version; full code must be generated by agent.

- `setupBookingSearchDebounce`: uses `bookingSearch.valueChanges.pipe(debounceTime(300), distinctUntilChanged(), takeUntilDestroyed(this.destroyRef)).subscribe(...)`.
- `onBookingSort(event: Sort)`:
  ```ts
  if (!event.active || !event.direction) return;
  this.bookingSortField.set(event.active);
  this.bookingSortDescending.set(event.direction === "desc");
  this.bookingPage.set(0);
  this.saveState();
  this.fetchBookings();
  ```
- `applyReceiptDateFilter`: formats start/end dates to `dd-MM-yyyy` strings, then calls `fetchReceipts()`.
- `clearBookingFilters`: set form controls to default, update signals, fetch.

**Important:** Sorting is server‑side; `matSort` directive only emits events, we do not use `MatTableDataSource`.

## 7. Detail Modal Components

### BookingDetailDialogComponent

- **Selector**: `app-booking-detail-dialog`
- **Standalone**: `true`
- **Imports**: `CommonModule`, `MatDialogModule`, `MatButtonModule`, `MatIconModule`, `MatCardModule`, `MatDividerModule`, `MatChipsModule`, `MatListModule`.
- **Template**: same as before (using `@if`, `@for`), no old control flow. All data comes from `MAT_DIALOG_DATA` injected as `Booking`.

### ReceiptDetailDialogComponent

- **Selector**: `app-receipt-detail-dialog`
- **Standalone**: `true`
- **Imports**: `CommonModule`, `MatDialogModule`, `MatButtonModule`, `MatIconModule`, `MatCardModule`, `MatDividerModule`.
- **Template**: as before, purely presentational.

## 8. Session Storage – Deterministic Implementation

**Schema:**

```json
{
  "activeView": "bookings",
  "bookingPage": 0,
  "bookingPageSize": 10,
  "bookingSortField": "bookedAt",
  "bookingSortDescending": true,
  "bookingSearch": "",
  "bookingStatus": "",
  "receiptPage": 0,
  "receiptPageSize": 10,
  "receiptSortField": "id",
  "receiptSortDescending": true,
  "receiptStartDate": null,
  "receiptEndDate": null
}
```

**Exact restore logic (full code – no omissions):**

```ts
private restoreState(): void {
  try {
    const stored = sessionStorage.getItem(this.STORAGE_KEY);
    if (!stored) return;
    const parsed = JSON.parse(stored);
    if (typeof parsed !== 'object' || parsed === null) return;

    if (parsed.activeView === 'bookings' || parsed.activeView === 'receipts')
      this.activeView.setValue(parsed.activeView);

    if (Number.isInteger(parsed.bookingPage) && parsed.bookingPage >= 0) this.bookingPage.set(parsed.bookingPage);
    if (Number.isInteger(parsed.bookingPageSize) && parsed.bookingPageSize > 0) this.bookingPageSize.set(parsed.bookingPageSize);
    if (typeof parsed.bookingSortField === 'string') this.bookingSortField.set(parsed.bookingSortField);
    if (typeof parsed.bookingSortDescending === 'boolean') this.bookingSortDescending.set(parsed.bookingSortDescending);
    if (typeof parsed.bookingSearch === 'string') this.bookingSearch.setValue(parsed.bookingSearch);
    if (typeof parsed.bookingStatus === 'string') this.bookingStatus.setValue(parsed.bookingStatus);

    if (Number.isInteger(parsed.receiptPage) && parsed.receiptPage >= 0) this.receiptPage.set(parsed.receiptPage);
    if (Number.isInteger(parsed.receiptPageSize) && parsed.receiptPageSize > 0) this.receiptPageSize.set(parsed.receiptPageSize);
    if (typeof parsed.receiptSortField === 'string') this.receiptSortField.set(parsed.receiptSortField);
    if (typeof parsed.receiptSortDescending === 'boolean') this.receiptSortDescending.set(parsed.receiptSortDescending);
    if (parsed.receiptStartDate === null || (typeof parsed.receiptStartDate === 'string' && !isNaN(Date.parse(parsed.receiptStartDate))))
      this.receiptStartDate.setValue(parsed.receiptStartDate ? new Date(parsed.receiptStartDate) : null);
    if (parsed.receiptEndDate === null || (typeof parsed.receiptEndDate === 'string' && !isNaN(Date.parse(parsed.receiptEndDate))))
      this.receiptEndDate.setValue(parsed.receiptEndDate ? new Date(parsed.receiptEndDate) : null);
  } catch { /* fallback silently */ }
}

private saveState(): void {
  sessionStorage.setItem(this.STORAGE_KEY, JSON.stringify({
    activeView: this.activeView.value,
    bookingPage: this.bookingPage(),
    bookingPageSize: this.bookingPageSize(),
    bookingSortField: this.bookingSortField(),
    bookingSortDescending: this.bookingSortDescending(),
    bookingSearch: this.bookingSearch.value,
    bookingStatus: this.bookingStatus.value,
    receiptPage: this.receiptPage(),
    receiptPageSize: this.receiptPageSize(),
    receiptSortField: this.receiptSortField(),
    receiptSortDescending: this.receiptSortDescending(),
    receiptStartDate: this.receiptStartDate.value?.toISOString() ?? null,
    receiptEndDate: this.receiptEndDate.value?.toISOString() ?? null,
  }));
}
```

## 9. UI States

- **Initial load**: full‑page spinner (bookings or receipts) only when data array is empty and loading true.
- **Refetch**: `mat-progress-bar` shown while loading existing data.
- **Error**: `app-alert` with retry button.
- **Empty**: “No bookings/receipts found.” with suggestion to adjust filters.

## 10. Responsive Behaviour

- Tables scroll horizontally on mobile.
- Controls stack vertically.
- Detail modals use 90% width on mobile.

## 11. Accessibility

- Tables have `aria-label`.
- Clickable rows; icon buttons have `aria-label`.
- Dialogs trap focus.

## 12. Integration Notes

- **Overwrite** placeholder: `src/app/features/admin/pages/oversight/billing-receipts.component.ts`.
- Create `BookingApiService`, `BillingApiService`, and model files.
- Create the two detail dialog components as standalone.
- Date format for receipts API is `dd-MM-yyyy`; conversion occurs in `applyReceiptDateFilter()`.
- No `MatTableDataSource` is used – raw array is passed to `[dataSource]`.
- No `*ngIf` / `*ngFor` anywhere – only `@if`, `@for`.

## 13. File Structure

```
src/app/features/admin/
  pages/oversight/
    billing-receipts.component.ts   (overwrite)
    billing-receipts.component.html
    billing-receipts.component.scss
    booking-detail-dialog.component.ts
    booking-detail-dialog.component.html
    receipt-detail-dialog.component.ts
    receipt-detail-dialog.component.html
  services/
    booking-api.service.ts
    billing-api.service.ts
  models/
    booking.model.ts
    receipt.model.ts
```

## 14. Self‑Review Checklist

- [ ] Toggle switches views; each table loads with correct data.
- [ ] Bookings: status filter, guest search, server‑side sort/pagination all work.
- [ ] Clicking booking row opens detail modal with full info.
- [ ] Receipts: date filters, sort/pagination work.
- [ ] Clicking receipt row opens detail modal.
- [ ] Loading spinners/progress bar behave correctly (initial vs refresh).
- [ ] Session storage fully restores state; invalid data falls back to defaults.
- [ ] Error messages extracted correctly from backend errors.
- [ ] No old control flow directives (`*ngIf`, `*ngFor`) exist.
- [ ] No `MatTableDataSource` used; plain array works with server‑side sort.
- [ ] All subscriptions use `takeUntilDestroyed`.

## 15. Implementation Constraints

- Angular 18 control flow (`@if`, `@for`) ONLY.
- Standalone components, signals, `takeUntilDestroyed`.
- Overwrite placeholder; do not rename.
- Raw array as `[dataSource]` for tables; `matSort` used for sort change events only.
- `extractErrorMessage` helper must be used in all API error handling.
- All validations and session storage code must be copied verbatim.

