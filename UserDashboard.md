# /components/billing-dialog/billing-dialog.component.html

<h2 mat-dialog-title>Billing Folio – Booking #{{ bookingId }}</h2>
<mat-dialog-content class="billing-content">
  @if (loading()) {
    <div style="display: flex; justify-content: center; padding: 24px;">
      <mat-spinner diameter="40"></mat-spinner>
    </div>
  } @else if (error()) {
    <app-alert type="error" [message]="error()!" (closed)="error.set(null)">
      <button mat-button (click)="fetchBilling()">Retry</button>
    </app-alert>
  } @else if (folio()) {
    <div class="bill-row">
      <span class="bill-label">Guest:</span>
      <span class="bill-value">{{ folio()!.guestName }}</span>
    </div>
    <div class="bill-row">
      <span class="bill-label">Nights:</span>
      <span class="bill-value">{{ folio()!.nightsStayed }}</span>
    </div>
    <div class="bill-row">
      <span class="bill-label">Room Rate:</span>
      <span class="bill-value">{{ folio()!.roomBasePrice | currency }}/night</span>
    </div>
    <div class="bill-row">
      <span class="bill-label">Room Subtotal:</span>
      <span class="bill-value">{{ folio()!.roomTotal | currency }}</span>
    </div>
    <div class="bill-row">
      <span class="bill-label">Room Services / Food:</span>
      <span class="bill-value">{{ folio()!.foodTotal | currency }}</span>
    </div>
    <div class="bill-row">
      <span class="bill-label">Amenities Subtotal:</span>
      <span class="bill-value">{{ folio()!.amenityTotal | currency }}</span>
    </div>
    <div class="bill-row total-bill-row">
      <span>Total Bill:</span>
      <span>{{ folio()!.totalBill | currency }}</span>
    </div>
    <div class="bill-row" style="margin-top: 12px;">
      <span class="bill-label">Payment Status:</span>
      <span class="bill-value" [style.color]="folio()!.paymentStatus === 'Paid' ? 'green' : 'red'">
        {{ folio()!.paymentStatus }}
      </span>
    </div>

    @if (folio()!.foodItems && folio()!.foodItems.length > 0) {
      <h3>Room Service Orders</h3>
      <ul>
        @for (item of folio()!.foodItems; track item) {
          <li>{{ item }}</li>
        }
      </ul>
    }

    @if (folio()!.amenityItems && folio()!.amenityItems.length > 0) {
      <h3>Amenities Subscribed</h3>
      <ul>
        @for (item of folio()!.amenityItems; track item) {
          <li>{{ item }}</li>
        }
      </ul>
    }
  } @else {
    <p>No billing details found.</p>
  }
</mat-dialog-content>
<mat-dialog-actions align="end">
  <button mat-button mat-dialog-close>Close</button>
</mat-dialog-actions>


# /components/billing-dialog/billing-dialog.component.ts

import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { BillingApiService } from '../../services/billing-api.service';
import { BillingFolio } from '../../models/billing-folio.model';
import { AlertComponent } from '../../../../features/auth/components/alert.component';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'app-billing-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule, MatProgressSpinnerModule, AlertComponent],
  templateUrl: './billing-dialog.component.html',
  styles: [`
    .billing-content {
      min-width: 320px;
    }
    .bill-row {
      display: flex;
      justify-content: space-between;
      margin-bottom: 8px;
      font-size: 0.95rem;
    }
    .bill-label {
      font-weight: 500;
      color: rgba(0, 0, 0, 0.6);
    }
    .bill-value {
      font-weight: 600;
      color: rgba(0, 0, 0, 0.87);
    }
    .total-bill-row {
      border-top: 1px solid rgba(0,0,0,0.12);
      padding-top: 8px;
      margin-top: 8px;
      font-weight: bold;
      font-size: 1.1rem;
    }
    h3 {
      margin: 16px 0 8px 0;
      font-size: 1rem;
      border-bottom: 1px solid #f0f0f0;
      padding-bottom: 4px;
    }
    ul {
      margin: 0;
      padding-left: 20px;
      font-size: 0.9rem;
      color: rgba(0,0,0,0.7);
    }
  `]
})
export class BillingDialogComponent implements OnInit {
  readonly bookingId: number = inject(MAT_DIALOG_DATA);
  private readonly billingApi = inject(BillingApiService);

  folio = signal<BillingFolio | null>(null);
  loading = signal(false);
  error = signal<string | null>(null);

  ngOnInit(): void {
    this.fetchBilling();
  }

  fetchBilling(): void {
    this.loading.set(true);
    this.error.set(null);

    this.billingApi.getByBookingId(this.bookingId)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (data) => this.folio.set(data),
        error: (err) => {
          const message = err.error?.message || err.message || 'Could not fetch billing details.';
          this.error.set(message);
        }
      });
  }
}


# /components/booking-detail-dialog/booking-detail-dialog.component.html

<h2 mat-dialog-title>Booking Details – #{{ booking.id }}</h2>
<mat-dialog-content>
  <div class="detail-section">
    <div class="detail-row">
      <span class="detail-label">Status:</span>
      <span class="detail-value"><strong>{{ booking.bookingStatus }}</strong></span>
    </div>
    <div class="detail-row">
      <span class="detail-label">Guest Name:</span>
      <span class="detail-value">{{ booking.guestName }}</span>
    </div>
    <div class="detail-row">
      <span class="detail-label">Guest Email:</span>
      <span class="detail-value">{{ booking.guestEmail }}</span>
    </div>
    <div class="detail-row">
      <span class="detail-label">Guests:</span>
      <span class="detail-value">{{ booking.guestCount }}</span>
    </div>
    <div class="detail-row">
      <span class="detail-label">Check‑in:</span>
      <span class="detail-value">{{ booking.checkInDate }}</span>
    </div>
    <div class="detail-row">
      <span class="detail-label">Check‑out:</span>
      <span class="detail-value">{{ booking.checkOutDate }}</span>
    </div>
    <div class="detail-row">
      <span class="detail-label">Booked At:</span>
      <span class="detail-value">{{ booking.bookedAt | date:'medium' }}</span>
    </div>
  </div>

  <mat-divider></mat-divider>

  <div class="detail-section" style="margin-top: 16px;">
    <h3>Rooms Included</h3>
    @if (enrichedRooms().length > 0) {
      @for (room of enrichedRooms(); track room.id) {
        <div class="room-item" style="margin-bottom: 12px; padding: 8px; border-left: 3px solid #1976d2; background: #f9f9f9; border-radius: 0 4px 4px 0;">
          <p style="margin: 0 0 4px 0;"><strong>Room:</strong> {{ room.roomNumber ?? 'Unassigned' }}</p>
          <p style="margin: 0 0 4px 0;"><strong>Type:</strong> {{ room.roomTypeName }}</p>
          <p style="margin: 0;"><strong>Price:</strong> {{ room.lockedInPrice | currency }}</p>
        </div>
      }
    } @else if (booking.rooms && booking.rooms.length > 0) {
      <ul>
        @for (room of booking.rooms; track room.id) {
          <li>
            Room Number: {{ room.roomNumber || 'Pending Assignment' }}
            (Locked‑in Price: {{ room.lockedInPrice | currency }})
          </li>
        }
      </ul>
    } @else {
      <p>No rooms assigned.</p>
    }
  </div>

  <mat-divider></mat-divider>

  <div class="detail-section" style="margin-top: 16px;">
    <h3>Amenities Subscribed</h3>
    @if (booking.amenityIds && booking.amenityIds.length > 0) {
      <ul>
        @for (id of booking.amenityIds; track id) {
          <li>Amenity ID: {{ id }} (TODO: Resolve Amenity Names)</li>
        }
      </ul>
    } @else {
      <p>No amenities selected.</p>
    }
  </div>
</mat-dialog-content>
<mat-dialog-actions align="end">
  <button mat-button mat-dialog-close>Close</button>
</mat-dialog-actions>


# /components/booking-detail-dialog/booking-detail-dialog.component.ts

import { Component, inject, OnInit, signal, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { MatListModule } from '@angular/material/list';
import { Booking, BookingRoom } from '../../models/booking.model';
import { RoomTypeApiService } from '../../services/room-type-api.service';
import { forkJoin, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-booking-detail-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule, MatListModule, MatDividerModule],
  templateUrl: './booking-detail-dialog.component.html',
  styles: [`
    .detail-section {
      margin-bottom: 16px;
    }
    .detail-row {
      display: flex;
      margin-bottom: 8px;
      font-size: 0.95rem;
    }
    .detail-label {
      font-weight: 600;
      width: 140px;
      color: rgba(0, 0, 0, 0.6);
    }
    .detail-value {
      color: rgba(0, 0, 0, 0.87);
    }
    ul {
      margin: 4px 0 0 0;
      padding-left: 20px;
    }
  `]
})
export class BookingDetailDialogComponent implements OnInit {
  readonly booking: Booking = inject(MAT_DIALOG_DATA);
  private readonly roomTypeApi = inject(RoomTypeApiService);
  private readonly destroyRef = inject(DestroyRef);

  enrichedRooms = signal<(BookingRoom & { roomTypeName: string })[]>([]);

  ngOnInit(): void {
    this.enrichRooms();
  }

  private enrichRooms(): void {
    const rooms = this.booking.rooms ?? [];
    if (rooms.length === 0) return;

    const requests = rooms.map(room =>
      this.roomTypeApi.getById(room.roomTypeId).pipe(
        map(roomType => ({
          ...room,
          roomTypeName: roomType?.name ?? `Room Type ${room.roomTypeId}`
        })),
        catchError(() => of({
          ...room,
          roomTypeName: `Room Type ${room.roomTypeId}`
        }))
      )
    );

    forkJoin(requests).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(enriched => {
      this.enrichedRooms.set(enriched);
    });
  }
}


# /components/booking-history/booking-history.component.html

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
    <div style="display: flex; justify-content: center; padding: 24px;">
      <mat-spinner diameter="40"></mat-spinner>
    </div>
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
  }

  @if (bookings().length > 0 || loading()) {
    @if (loading()) {
      <mat-progress-bar mode="indeterminate"></mat-progress-bar>
    }
    <div class="table-container">
      <table
        mat-table
        [dataSource]="bookings()"
        matSort
        matSortDisableClear
        (matSortChange)="onSortChange($event)"
      >
        <!-- ID Column -->
        <ng-container matColumnDef="id">
          <th
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
          </td>
        </ng-container>

        <!-- Check-in Column -->
        <ng-container matColumnDef="checkIn">
          <th
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
          </td>
        </ng-container>

        <!-- Check-out Column -->
        <ng-container matColumnDef="checkOut">
          <th
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
          </td>
        </ng-container>

        <!-- Status Column -->
        <ng-container matColumnDef="status">
          <th
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
          </td>
        </ng-container>

        <!-- Rooms Column -->
        <ng-container matColumnDef="rooms">
          <th
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
          </td>
        </ng-container>

        <!-- Actions Column -->
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
              (click)="openDetail(b)"
              aria-label="View"
              matTooltip="View Details"
            >
              <mat-icon>visibility</mat-icon>
            </button>
            @if (b.bookingStatus === 'Booked') {
              <button
                mat-icon-button
                (click)="cancelBooking(b)"
                aria-label="Cancel"
                matTooltip="Cancel Booking"
              >
                <mat-icon>cancel</mat-icon>
              </button>
            }
            @if (b.bookingStatus === 'CheckedOut') {
              <button
                mat-icon-button
                (click)="openFeedback(b)"
                aria-label="Feedback"
                matTooltip="Leave Feedback"
              >
                <mat-icon>feedback</mat-icon>
              </button>
            }
            <button
              mat-icon-button
              (click)="openBilling(b)"
              aria-label="Billing"
              matTooltip="View Billing"
            >
              <mat-icon>receipt</mat-icon>
            </button>
          </td>
        </ng-container>

        <tr
          mat-header-row
          *matHeaderRowDef="displayedColumns"
        ></tr>
        <tr
          mat-row
          *matRowDef="let row; columns: displayedColumns"
          [class.highlight]="highlightRowId() === row.id"
        ></tr>
      </table>
    </div>

    <mat-paginator
      [length]="totalCount()"
      [pageSize]="pageSize"
      [pageIndex]="pageIndex"
      [pageSizeOptions]="[5, 10, 20]"
      (page)="onPageChange($event)"
      aria-label="Select page of bookings"
    ></mat-paginator>
  } @else if (!loading()) {
    <p class="no-bookings">No bookings found.</p>
  }
</div>


# /components/booking-history/booking-history.component.scss

.history-view {
  .controls {
    display: flex;
    gap: 16px;
    align-items: center;
    margin-bottom: 16px;
  }

  .table-container {
    overflow-x: auto;
    width: 100%;
    margin-bottom: 16px;
    border: 1px solid rgba(0, 0, 0, 0.12);
    border-radius: 4px;
  }

  table {
    width: 100%;
    border-collapse: collapse;

    tr.highlight {
      background-color: #e8eaf6 !important;
      animation: flash-highlight 2s ease-out;
    }
  }

  .no-bookings {
    padding: 24px;
    text-align: center;
    color: rgba(0, 0, 0, 0.54);
    font-size: 1.1rem;
  }
}

@keyframes flash-highlight {
  0% {
    background-color: #c5cae9;
  }
  100% {
    background-color: transparent;
  }
}


# /components/booking-history/booking-history.component.ts

import { Component, inject, signal, input, effect, ViewChild, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatSortModule, MatSort, Sort } from '@angular/material/sort';
import { MatPaginatorModule, MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { BookingApiService } from '../../services/booking-api.service';
import { Booking } from '../../../../features/admin/models/booking.model';
import { AlertComponent } from '../../../../features/auth/components/alert.component';
import { BookingDetailDialogComponent } from '../booking-detail-dialog/booking-detail-dialog.component';
import { BillingDialogComponent } from '../billing-dialog/billing-dialog.component';
import { FeedbackDialogComponent } from '../feedback-dialog/feedback-dialog.component';
import { ConfirmDialogComponent } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';
import { finalize } from 'rxjs/operators';

interface HistoryState {
  status: string;
  sortField: string;
  sortDescending: boolean;
  pageIndex: number;
  pageSize: number;
}

@Component({
  selector: 'app-booking-history',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatTableModule,
    MatSortModule,
    MatPaginatorModule,
    MatSelectModule,
    MatFormFieldModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatProgressBarModule,
    MatDialogModule,
    MatSnackBarModule,
    AlertComponent
  ],
  templateUrl: './booking-history.component.html',
  styleUrls: ['./booking-history.component.scss']
})
export class BookingHistoryComponent implements AfterViewInit {
  userEmail = input.required<string>();
  refresh = input(0);
  highlightBookingId = input<number | null>(null);

  private readonly bookingApi = inject(BookingApiService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  bookings = signal<Booking[]>([]);
  totalCount = signal(0);
  loading = signal(false);
  error = signal<string | null>(null);
  highlightRowId = signal<number | null>(null);

  statusFilter = new FormControl<string>('', { nonNullable: true });
  displayedColumns = ['id', 'checkIn', 'checkOut', 'status', 'rooms', 'actions'];

  pageIndex = 0;
  pageSize = 10;
  sortField = 'id';
  sortDescending = true;

  @ViewChild(MatSort) sort!: MatSort;
  @ViewChild(MatPaginator) paginator!: MatPaginator;

  private readonly STORAGE_KEY = 'customerBookingsState';

  constructor() {
    this.loadState();

    // Effect to trigger load when email or refresh trigger changes
    effect(() => {
      const email = this.userEmail();
      const ref = this.refresh();
      if (email) {
        this.fetchData();
      }
    });

    // Effect to trigger row highlighting
    effect(() => {
      const highlightId = this.highlightBookingId();
      if (highlightId != null) {
        this.highlightRowId.set(highlightId);
        setTimeout(() => {
          this.highlightRowId.set(null);
          // Also try to scroll to the highlighted element
          const rowEl = document.querySelector(`.highlight`);
          if (rowEl) {
            rowEl.scrollIntoView({ behavior: 'smooth', block: 'center' });
          }
        }, 100);
      }
    });
  }

  ngAfterViewInit(): void {
    // Restore sort state visually — sort may be undefined on first render because
    // the table is wrapped in an @if block (no data yet), so guard before access.
    if (this.sort) {
      this.sort.active = this.sortField;
      this.sort.direction = this.sortDescending ? 'desc' : 'asc';
    }
  }

  fetchData(): void {
    this.loading.set(true);
    this.error.set(null);

    this.bookingApi.getAll({
      guestQuery: this.userEmail(),
      status: this.statusFilter.value || undefined,
      pageNumber: this.pageIndex + 1,
      pageSize: this.pageSize,
      sortBy: this.sortField,
      sortDescending: this.sortDescending
    }).pipe(
      finalize(() => this.loading.set(false))
    ).subscribe({
      next: (res) => {
        this.bookings.set(res.data);
        this.totalCount.set(res.totalCount);
        this.saveState();

        // If we are highlighting a row, let's schedule a scroll after DOM is updated
        const highlightId = this.highlightBookingId();
        if (highlightId != null) {
          setTimeout(() => {
            const rowEl = document.querySelector(`.highlight`);
            if (rowEl) {
              rowEl.scrollIntoView({ behavior: 'smooth', block: 'center' });
            }
          }, 300);
        }
      },
      error: (err) => {
        const message = err.error?.message || err.message || 'Failed to load bookings.';
        this.error.set(message);
      }
    });
  }

  onFilterChange(): void {
    this.pageIndex = 0;
    if (this.paginator) {
      this.paginator.pageIndex = 0;
    }
    this.fetchData();
  }

  clearFilter(): void {
    this.statusFilter.setValue('');
    this.onFilterChange();
  }

  onSortChange(sort: Sort): void {
    this.sortField = sort.active;
    this.sortDescending = sort.direction === 'desc';
    this.pageIndex = 0;
    if (this.paginator) {
      this.paginator.pageIndex = 0;
    }
    this.fetchData();
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.fetchData();
  }

  getRoomsSummary(booking: Booking): string {
    return booking.rooms
      .filter(r => r.roomNumber !== null)
      .map(r => r.roomNumber as string)
      .join(', ') || 'Pending Assignment';
  }

  openDetail(booking: Booking): void {
    this.dialog.open(BookingDetailDialogComponent, {
      data: booking,
      width: '500px'
    });
  }

  cancelBooking(booking: Booking): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Cancel Booking',
        message: `Are you sure you want to cancel booking #${booking.id}?`
      }
    });

    dialogRef.afterClosed().subscribe((confirmed: boolean) => {
      if (confirmed) {
        this.loading.set(true);
        this.bookingApi.cancel(booking.id)
          .pipe(finalize(() => this.loading.set(false)))
          .subscribe({
            next: () => {
              this.snackBar.open('Booking successfully cancelled.', 'Close', { duration: 4000 });
              this.fetchData();
            },
            error: (err) => {
              const message = err.error?.message || err.message || 'Failed to cancel booking.';
              this.snackBar.open(message, 'Close', { duration: 5000 });
            }
          });
      }
    });
  }

  openFeedback(booking: Booking): void {
    this.dialog.open(FeedbackDialogComponent, {
      data: booking.id,
      width: '450px'
    });
  }

  openBilling(booking: Booking): void {
    this.dialog.open(BillingDialogComponent, {
      data: booking.id,
      width: '500px'
    });
  }

  private loadState(): void {
    try {
      const stateStr = sessionStorage.getItem(this.STORAGE_KEY);
      if (stateStr) {
        const state: HistoryState = JSON.parse(stateStr);
        this.statusFilter.setValue(state.status);
        this.sortField = state.sortField;
        this.sortDescending = state.sortDescending;
        this.pageIndex = state.pageIndex;
        this.pageSize = state.pageSize;
      }
    } catch (e) {
      console.error('Error loading history state:', e);
    }
  }

  private saveState(): void {
    try {
      const state: HistoryState = {
        status: this.statusFilter.value,
        sortField: this.sortField,
        sortDescending: this.sortDescending,
        pageIndex: this.pageIndex,
        pageSize: this.pageSize
      };
      sessionStorage.setItem(this.STORAGE_KEY, JSON.stringify(state));
    } catch (e) {
      console.error('Error saving history state:', e);
    }
  }
}


# /components/booking-wizard/booking-wizard.component.html

<mat-stepper
  linear
  #stepper
  [orientation]="isMobile() ? 'vertical' : 'horizontal'"
  (selectionChange)="onStepChange($event)"
>
  <!-- Step 1: Dates & Guests -->
  <mat-step
    [stepControl]="datesForm"
    label="Dates & Guests"
  >
    <form [formGroup]="datesForm" class="stepper-form">
      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Check‑in</mat-label>
        <input
          matInput
          [matDatepicker]="cinPicker"
          formControlName="checkInDate"
          required
        />
        <mat-datepicker-toggle matIconSuffix [for]="cinPicker"></mat-datepicker-toggle>
        <mat-datepicker #cinPicker></mat-datepicker>
        @if (datesForm.get('checkInDate')?.touched && datesForm.get('checkInDate')?.invalid) {
          <mat-error>Check‑in date is required.</mat-error>
        }
      </mat-form-field>

      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Check‑out</mat-label>
        <input
          matInput
          [matDatepicker]="coutPicker"
          formControlName="checkOutDate"
          required
        />
        <mat-datepicker-toggle matIconSuffix [for]="coutPicker"></mat-datepicker-toggle>
        <mat-datepicker #coutPicker></mat-datepicker>
        @if (datesForm.get('checkOutDate')?.touched && datesForm.get('checkOutDate')?.invalid) {
          <mat-error>Check‑out date is required.</mat-error>
        }
      </mat-form-field>

      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Guests</mat-label>
        <input
          matInput
          type="number"
          formControlName="guestCount"
          min="1"
          max="20"
          required
        />
        @if (datesForm.get('guestCount')?.touched && datesForm.get('guestCount')?.invalid) {
          <mat-error>Guests count must be between 1 and 20.</mat-error>
        }
      </mat-form-field>

      @if (datesForm.touched && datesForm.errors) {
        <div class="form-error">
          @if (datesForm.errors['checkInInPast']) {
            <p>Check‑in date cannot be in the past.</p>
          }
          @if (datesForm.errors['checkOutBeforeCheckIn']) {
            <p>Check‑out date must be strictly after Check‑in date.</p>
          }
        </div>
      }

      <div class="actions">
        <button
          mat-raised-button
          color="primary"
          matStepperNext
          [disabled]="datesForm.invalid"
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
    @if (loading()) {
      <div style="display: flex; justify-content: center; padding: 24px;">
        <mat-spinner diameter="40"></mat-spinner>
      </div>
    } @else if (error()) {
      <app-alert type="error" [message]="error()!" (closed)="error.set(null)">
        <button mat-button (click)="loadRooms()">Retry</button>
      </app-alert>
    } @else {
      <form [formGroup]="roomsForm" class="stepper-form">
        <div class="room-list">
          @for (room of availableRooms(); track room.roomTypeId) {
            <div class="room-item">
              <div class="room-info">
                <h3>{{ room.name }}</h3>
                <p>{{ room.description || 'No description available.' }}</p>
                <div class="room-meta">
                  <span><strong>{{ room.basePrice | currency }}</strong>/night</span>
                  <span class="divider">|</span>
                  <span>Max guests: {{ room.maxOccupancy }}</span>
                  <span class="divider">|</span>
                  <span [style.color]="room.availableCount > 0 ? 'green' : 'red'">
                    Available: {{ room.availableCount }}
                  </span>
                </div>
              </div>
              <div class="quantity-selector">
                <button
                  mat-icon-button
                  type="button"
                  (click)="decrementRoom(room.roomTypeId)"
                  [disabled]="getRoomQuantity(room.roomTypeId) === 0"
                  aria-label="Remove room"
                >
                  <mat-icon>remove</mat-icon>
                </button>
                <span class="qty-display">{{ getRoomQuantity(room.roomTypeId) }}</span>
                <button
                  mat-icon-button
                  type="button"
                  (click)="incrementRoom(room.roomTypeId)"
                  [disabled]="getRoomQuantity(room.roomTypeId) >= room.availableCount"
                  aria-label="Add room"
                >
                  <mat-icon>add</mat-icon>
                </button>
              </div>
            </div>
          }
        </div>

        @if (capacityWarning()) {
          <p class="warning-text"><mat-icon>warning</mat-icon> {{ capacityWarning() }}</p>
        }

        <div class="actions" style="margin-top: 16px;">
          <button mat-button matStepperPrevious>Back</button>
          <button
            mat-raised-button
            color="primary"
            matStepperNext
            [disabled]="totalSelectedQuantity() === 0 || capacityWarning() !== null"
          >
            Next
          </button>
        </div>
      </form>
    }
  </mat-step>

  <!-- Step 3: Amenities -->
  <mat-step
    [stepControl]="amenitiesForm"
    label="Add Amenities"
  >
    @if (loading()) {
      <div style="display: flex; justify-content: center; padding: 24px;">
        <mat-spinner diameter="40"></mat-spinner>
      </div>
    } @else if (error()) {
      <app-alert type="error" [message]="error()!" (closed)="error.set(null)">
        <button mat-button (click)="loadAmenities()">Retry</button>
      </app-alert>
    } @else {
      <form [formGroup]="amenitiesForm" class="stepper-form">
        <div class="amenity-list">
          @for (amenity of availableAmenities(); track amenity.id; let i = $index) {
            <div class="amenity-item">
              <mat-checkbox [formControl]="getAmenityControl(i)">
                <div class="amenity-info">
                  <strong>{{ amenity.name }}</strong> – {{ amenity.price | currency }}
                  <p class="amenity-desc">{{ amenity.description }}</p>
                </div>
              </mat-checkbox>
            </div>
          }
        </div>
        <div class="actions" style="margin-top: 16px;">
          <button mat-button matStepperPrevious>Back</button>
          <button
            mat-raised-button
            color="primary"
            matStepperNext
          >
            Next
          </button>
        </div>
      </form>
    }
  </mat-step>

  <!-- Step 4: Review & Confirm -->
  <mat-step label="Review & Confirm">
    <div class="summary-container">
      <h3>Stay Overview</h3>
      <div class="overview-grid">
        <div><strong>Guest Name:</strong> {{ userProfile().firstName }} {{ userProfile().lastName }}</div>
        <div><strong>Email:</strong> {{ userProfile().email }}</div>
        <div><strong>Check‑in:</strong> {{ datesForm.value.checkInDate | date:'mediumDate' }}</div>
        <div><strong>Check‑out:</strong> {{ datesForm.value.checkOutDate | date:'mediumDate' }}</div>
        <div><strong>Nights Stayed:</strong> {{ nights() }}</div>
        <div><strong>Guest Count:</strong> {{ datesForm.value.guestCount }}</div>
      </div>

      <mat-divider></mat-divider>

      <h3>Rooms Selected</h3>
      @if (selectedRoomEntries().length > 0) {
        <ul class="summary-list">
          @for (item of selectedRoomEntries(); track item.roomTypeId) {
            <li>
              <div class="list-item-content">
                <span>{{ item.name }} x{{ item.quantity }}</span>
                <span>{{ item.quantity * item.basePrice * nights() | currency }}</span>
              </div>
            </li>
          }
        </ul>
      }

      <mat-divider></mat-divider>

      <h3>Amenities Selected</h3>
      @if (selectedAmenityEntries().length > 0) {
        <ul class="summary-list">
          @for (item of selectedAmenityEntries(); track item.id) {
            <li>
              <div class="list-item-content">
                <span>{{ item.name }}</span>
                <span>{{ item.price | currency }}</span>
              </div>
            </li>
          }
        </ul>
      } @else {
        <p class="empty-text">No amenities selected.</p>
      }

      <mat-divider></mat-divider>

      <div class="estimated-total-row">
        <span>Estimated Total:</span>
        <span class="total-price">{{ estimatedTotal() | currency }}</span>
      </div>

      @if (error()) {
        <app-alert type="error" [message]="error()!" (closed)="error.set(null)"></app-alert>
      }

      <div class="actions" style="margin-top: 24px;">
        <button mat-button matStepperPrevious [disabled]="loading()">Back</button>
        <button
          mat-raised-button
          color="primary"
          [disabled]="loading()"
          (click)="submitBooking()"
        >
          @if (loading()) {
            <mat-spinner diameter="18" style="display: inline-block; margin-right: 8px;"></mat-spinner>
          }
          Confirm Booking
        </button>
      </div>
    </div>
  </mat-step>
</mat-stepper>


# /components/booking-wizard/booking-wizard.component.scss

.stepper-form {
  padding: 8px 0;
  max-width: 600px;
}

.full-width {
  width: 100%;
}

.form-error {
  color: #f44336;
  font-size: 0.85rem;
  margin-bottom: 16px;
  p {
    margin: 4px 0;
  }
}

.room-list {
  display: flex;
  flex-direction: column;
  gap: 16px;
  margin-bottom: 16px;
}

.room-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px;
  border: 1px solid rgba(0, 0, 0, 0.12);
  border-radius: 8px;
  background-color: #fafafa;
}

.room-info {
  flex-grow: 1;
  padding-right: 16px;

  h3 {
    margin: 0 0 4px 0;
    font-size: 1.1rem;
    font-weight: 500;
  }

  p {
    margin: 0 0 8px 0;
    font-size: 0.875rem;
    color: rgba(0, 0, 0, 0.54);
    display: -webkit-box;
    -webkit-line-clamp: 2;
    -webkit-box-orient: vertical;
    overflow: hidden;
  }
}

.room-meta {
  display: flex;
  align-items: center;
  font-size: 0.85rem;
  color: rgba(0, 0, 0, 0.6);

  .divider {
    margin: 0 8px;
    color: rgba(0, 0, 0, 0.2);
  }
}

.quantity-selector {
  display: flex;
  align-items: center;
  gap: 8px;
  border: 1px solid rgba(0, 0, 0, 0.12);
  border-radius: 20px;
  padding: 2px;
  background: white;

  .qty-display {
    font-weight: 600;
    min-width: 24px;
    text-align: center;
  }
}

.warning-text {
  color: #ff9800;
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 0.9rem;
  margin-top: 12px;

  mat-icon {
    font-size: 20px;
    width: 20px;
    height: 20px;
  }
}

.amenity-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.amenity-item {
  border: 1px solid rgba(0, 0, 0, 0.08);
  border-radius: 6px;
  padding: 12px;
  background-color: #fafafa;

  mat-checkbox {
    width: 100%;
  }

  .amenity-info {
    display: flex;
    flex-direction: column;
    margin-left: 8px;
  }

  .amenity-desc {
    margin: 4px 0 0 0;
    font-size: 0.8rem;
    color: rgba(0, 0, 0, 0.54);
  }
}

.summary-container {
  max-width: 600px;
  padding: 16px;
  border: 1px solid rgba(0, 0, 0, 0.12);
  border-radius: 8px;

  h3 {
    margin: 16px 0 12px 0;
    font-size: 1.1rem;
    font-weight: 500;
    color: #3f51b5;

    &:first-of-type {
      margin-top: 0;
    }
  }

  .overview-grid {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 12px;
    margin-bottom: 16px;
    font-size: 0.9rem;
  }

  .summary-list {
    list-style: none;
    padding: 0;
    margin: 0 0 16px 0;

    li {
      padding: 8px 0;
      font-size: 0.95rem;
    }
  }

  .list-item-content {
    display: flex;
    justify-content: space-between;
  }

  .empty-text {
    font-style: italic;
    color: rgba(0,0,0,0.54);
    margin-bottom: 16px;
  }

  .estimated-total-row {
    display: flex;
    justify-content: space-between;
    align-items: center;
    font-size: 1.15rem;
    font-weight: 600;
    margin-top: 16px;

    .total-price {
      font-size: 1.3rem;
      color: #3f51b5;
    }
  }
}

@media (max-width: 767px) {
  .room-item {
    flex-direction: column;
    align-items: stretch;
    gap: 12px;
  }

  .room-info {
    padding-right: 0;
  }

  .quantity-selector {
    align-self: flex-end;
  }

  .summary-container .overview-grid {
    grid-template-columns: 1fr;
  }
}


# /components/booking-wizard/booking-wizard.component.ts

import { Component, inject, signal, computed, input, output, ChangeDetectorRef, DestroyRef, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormGroup, FormControl, FormArray, Validators, AbstractControl } from '@angular/forms';
import { MatStepperModule } from '@angular/material/stepper';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { ConfirmDialogComponent } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';
import { BreakpointObserver } from '@angular/cdk/layout';
import { map } from 'rxjs/operators';
import { toSignal } from '@angular/core/rxjs-interop';
import { RoomTypeApiService } from '../../services/room-type-api.service';
import { AmenityApiService } from '../../services/amenity-api.service';
import { BookingApiService } from '../../services/booking-api.service';
import { AvailableRoomType } from '../../models/available-room-type.model';
import { Amenity } from '../../../../features/admin/models/amenity.model';
import { AlertComponent } from '../../../../features/auth/components/alert.component';
import { MatDividerModule } from '@angular/material/divider';
import { finalize } from 'rxjs/operators';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-booking-wizard',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatStepperModule,
    MatInputModule,
    MatButtonModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatCheckboxModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatDividerModule,
    MatDialogModule,
    AlertComponent
  ],
  templateUrl: './booking-wizard.component.html',
  styleUrls: ['./booking-wizard.component.scss']
})
export class BookingWizardComponent implements OnInit {
  userProfile = input.required<{ firstName: string; lastName: string; email: string }>();
  bookingCreated = output<number>();

  initialCheckIn = input<Date | null>(null);
  initialCheckOut = input<Date | null>(null);
  initialGuests = input<number | null>(null);
  initialRoomTypeId = input<number | null>(null);

  private readonly roomTypeApi = inject(RoomTypeApiService);
  private readonly amenityApi = inject(AmenityApiService);
  private readonly bookingApi = inject(BookingApiService);
  private readonly breakpointObserver = inject(BreakpointObserver);
  private readonly dialog = inject(MatDialog);
  private readonly destroyRef = inject(DestroyRef);
  private readonly cdr = inject(ChangeDetectorRef);

  isMobile = toSignal(
    this.breakpointObserver.observe('(max-width: 767px)').pipe(map(r => r.matches)),
    { initialValue: false }
  );

  private initialRoomApplied = false;

  ngOnInit(): void {
    if (this.initialCheckIn() && this.initialCheckOut() && this.initialGuests()) {
      this.datesForm.patchValue({
        checkInDate: this.initialCheckIn(),
        checkOutDate: this.initialCheckOut(),
        guestCount: this.initialGuests() ?? 1
      });
      this.loadRooms();
    }
  }

  loading = signal(false);
  error = signal<string | null>(null);

  availableRooms = signal<AvailableRoomType[]>([]);
  availableAmenities = signal<Amenity[]>([]);
  selectedRoomQuantities = signal<Record<number, number>>({});

  // Forms definition
  datesForm = new FormGroup({
    checkInDate: new FormControl<Date | null>(null, { validators: [Validators.required] }),
    checkOutDate: new FormControl<Date | null>(null, { validators: [Validators.required] }),
    guestCount: new FormControl<number>(1, { validators: [Validators.required, Validators.min(1), Validators.max(20)], nonNullable: true })
  }, { validators: this.dateRangeValidator });

  roomsForm = new FormGroup({
    dummy: new FormControl<boolean>(false, { validators: [Validators.requiredTrue], nonNullable: true })
  });

  amenitiesForm = new FormGroup({
    selectedAmenities: new FormArray<FormControl<boolean>>([])
  });

  get amenityControls(): FormControl<boolean>[] {
    return (this.amenitiesForm.get('selectedAmenities') as FormArray).controls as FormControl<boolean>[];
  }

  getAmenityControl(index: number): FormControl<boolean> {
    return this.amenityControls[index];
  }

  // Convert form values to signals so computed reacts
  private datesValues = toSignal(this.datesForm.valueChanges, { initialValue: this.datesForm.value });
  private amenitiesValues = toSignal(this.amenitiesForm.valueChanges, { initialValue: this.amenitiesForm.value });

  // Computed signals
  nights = computed(() => {
    const dates = this.datesValues();
    if (!dates || !dates.checkInDate || !dates.checkOutDate) return 0;
    const cin = new Date(dates.checkInDate);
    const cout = new Date(dates.checkOutDate);
    return Math.max(0, Math.ceil((cout.getTime() - cin.getTime()) / (1000 * 3600 * 24)));
  });

  totalSelectedQuantity = computed(() => {
    return Object.values(this.selectedRoomQuantities()).reduce((a, b) => a + b, 0);
  });

  capacityWarning = computed(() => {
    const totalCap = this.availableRooms().reduce(
      (sum, r) => sum + (this.selectedRoomQuantities()[r.roomTypeId] || 0) * r.maxOccupancy,
      0
    );
    const dates = this.datesValues();
    const guests = dates?.guestCount ?? 0;
    if (this.totalSelectedQuantity() > 0 && totalCap < guests) {
      return `The selected rooms can only accommodate ${totalCap} guests. You need ${guests}.`;
    }
    return null;
  });

  selectedRoomEntries = computed(() => {
    const quantities = this.selectedRoomQuantities();
    return this.availableRooms()
      .filter(r => (quantities[r.roomTypeId] || 0) > 0)
      .map(r => ({
        roomTypeId: r.roomTypeId,
        name: r.name,
        basePrice: r.basePrice,
        maxOccupancy: r.maxOccupancy,
        quantity: quantities[r.roomTypeId]
      }));
  });

  selectedAmenityEntries = computed(() => {
    const list = this.availableAmenities();
    const amenitiesVal = this.amenitiesValues();
    const selectedList = amenitiesVal?.selectedAmenities || [];
    return list.filter((_, i) => selectedList[i] === true);
  });

  estimatedTotal = computed(() => {
    const amenitiesVal = this.amenitiesValues();
    const nights = this.nights();
    const roomCost = this.availableRooms().reduce(
      (sum, r) => sum + (this.selectedRoomQuantities()[r.roomTypeId] || 0) * r.basePrice * nights,
      0
    );
    const selectedList = amenitiesVal?.selectedAmenities || [];
    const amenityCost = this.availableAmenities().reduce(
      (sum, a, i) => sum + (selectedList[i] ? a.price : 0),
      0
    );
    return roomCost + amenityCost;
  });

  onStepChange(event: any): void {
    if (event.selectedIndex === 1) {
      this.loadRooms();
    } else if (event.selectedIndex === 2) {
      this.loadAmenities();
    }
  }

  loadRooms(): void {
    const cin = this.datesForm.value.checkInDate;
    const cout = this.datesForm.value.checkOutDate;
    if (!cin || !cout) return;

    this.loading.set(true);
    this.error.set(null);

    this.roomTypeApi.getAvailable(this.formatDate(cin), this.formatDate(cout))
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (res) => {
          this.availableRooms.set(res.data);
          // Pre-populate empty quantities
          const quantities: Record<number, number> = {};
          res.data.forEach(r => {
            quantities[r.roomTypeId] = 0;
          });
          this.selectedRoomQuantities.set(quantities);

          if (!this.initialRoomApplied && this.initialRoomTypeId()) {
            const room = res.data.find(r => r.roomTypeId === this.initialRoomTypeId());
            if (room && room.availableCount > 0) {
              this.selectedRoomQuantities.update(q => ({
                ...q,
                [room.roomTypeId]: 1
              }));
              this.initialRoomApplied = true;
            }
          }

          this.updateRoomsFormValidity();
        },
        error: (err) => {
          const message = err.error?.message || err.message || 'Failed to load available rooms.';
          this.error.set(message);
        }
      });
  }

  loadAmenities(): void {
    this.loading.set(true);
    this.error.set(null);

    this.amenityApi.getAll({ pageNumber: 1, pageSize: 100, isAvailable: true })
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (res) => {
          this.availableAmenities.set(res.data);
          const formArray = this.amenitiesForm.get('selectedAmenities') as FormArray;
          formArray.clear();
          res.data.forEach(() => {
            formArray.push(new FormControl<boolean>(false, { nonNullable: true }));
          });
          this.cdr.detectChanges();
        },
        error: (err) => {
          const message = err.error?.message || err.message || 'Failed to load amenities.';
          this.error.set(message);
        }
      });
  }

  incrementRoom(roomTypeId: number): void {
    const current = this.selectedRoomQuantities();
    const limit = this.availableRooms().find(r => r.roomTypeId === roomTypeId)?.availableCount ?? 0;
    const val = current[roomTypeId] || 0;
    if (val < limit) {
      this.selectedRoomQuantities.set({
        ...current,
        [roomTypeId]: val + 1
      });
      this.updateRoomsFormValidity();
    }
  }

  decrementRoom(roomTypeId: number): void {
    const current = this.selectedRoomQuantities();
    const val = current[roomTypeId] || 0;
    if (val > 0) {
      this.selectedRoomQuantities.set({
        ...current,
        [roomTypeId]: val - 1
      });
      this.updateRoomsFormValidity();
    }
  }

  getRoomQuantity(roomTypeId: number): number {
    return this.selectedRoomQuantities()[roomTypeId] || 0;
  }

  updateRoomsFormValidity(): void {
    const isValid = this.totalSelectedQuantity() > 0 && !this.capacityWarning();
    this.roomsForm.controls.dummy.setValue(isValid);
    this.roomsForm.updateValueAndValidity();
  }

  submitBooking(): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Confirm Booking',
        message: `Create this booking? Total estimated: $${this.estimatedTotal().toFixed(2)}`
      }
    });

    dialogRef.afterClosed().pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe((confirmed) => {
      if (confirmed) {
        this.performBooking();
      }
    });
  }

  private performBooking(): void {
    this.loading.set(true);
    this.error.set(null);

    const roomTypeIds: number[] = [];
    const quantities = this.selectedRoomQuantities();
    Object.keys(quantities).forEach(key => {
      const typeId = Number(key);
      const qty = quantities[typeId] || 0;
      for (let i = 0; i < qty; i++) {
        roomTypeIds.push(typeId);
      }
    });

    const amenityIds = this.selectedAmenityEntries().map(a => a.id);
    const profile = this.userProfile();

    const bookingDto = {
      roomTypeIds,
      guestCount: this.datesForm.value.guestCount!,
      checkInDate: this.datesForm.value.checkInDate!.toISOString(),
      checkOutDate: this.datesForm.value.checkOutDate!.toISOString(),
      guestName: `${profile.firstName} ${profile.lastName}`,
      guestEmail: profile.email,
      amenityIds
    };

    this.bookingApi.create(bookingDto)
      .pipe(
        finalize(() => this.loading.set(false)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: (response) => {
          this.bookingCreated.emit(response.id);
        },
        error: (err) => {
          const message = err.error?.message || err.message || 'Failed to confirm booking.';
          this.error.set(message);
        }
      });
  }

  formatDate(date: Date): string {
    const day = String(date.getDate()).padStart(2, '0');
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const year = date.getFullYear();
    return `${day}-${month}-${year}`;
  }

  private dateRangeValidator(control: AbstractControl): { [key: string]: boolean } | null {
    const cin = control.get('checkInDate')?.value as Date | null;
    const cout = control.get('checkOutDate')?.value as Date | null;
    if (!cin || !cout) return null;

    const today = new Date();
    today.setHours(0, 0, 0, 0);

    const cinDate = new Date(cin);
    cinDate.setHours(0, 0, 0, 0);

    if (cinDate < today) {
      return { checkInInPast: true };
    }

    const coutDate = new Date(cout);
    coutDate.setHours(0, 0, 0, 0);

    if (coutDate <= cinDate) {
      return { checkOutBeforeCheckIn: true };
    }

    return null;
  }
}


# /components/feedback-dialog/feedback-dialog.component.html

<h2 mat-dialog-title>Booking Feedback – Booking #{{ bookingId }}</h2>
<mat-dialog-content class="feedback-content">
  @if (loading()) {
    <div style="display: flex; justify-content: center; padding: 24px;">
      <mat-spinner diameter="40"></mat-spinner>
    </div>
  } @else if (error()) {
    <app-alert type="error" [message]="error()!" (closed)="error.set(null)">
      @if (!submitting()) {
        <button mat-button (click)="checkExistingFeedback()">Retry</button>
      }
    </app-alert>
  } @else if (existingFeedback()) {
    <div class="read-only-feedback">
      <p>Thank you for submitting feedback for this stay!</p>
      <p><strong>Rating:</strong> <span class="rating-value">{{ existingFeedback()!.rating }} / 5</span></p>
      <p><strong>Comments:</strong></p>
      <p style="white-space: pre-wrap; font-style: italic; background: #f9f9f9; padding: 12px; border-radius: 4px;">
        {{ existingFeedback()!.comments || 'No comments provided.' }}
      </p>
      <p style="font-size: 0.8rem; color: rgba(0,0,0,0.54);">Submitted on {{ existingFeedback()!.createdAt | date:'mediumDate' }}</p>
    </div>
  } @else {
    <form [formGroup]="feedbackForm">
      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Rating</mat-label>
        <mat-select formControlName="rating">
          <mat-option [value]="5">5 – Excellent</mat-option>
          <mat-option [value]="4">4 – Good</mat-option>
          <mat-option [value]="3">3 – Average</mat-option>
          <mat-option [value]="2">2 – Poor</mat-option>
          <mat-option [value]="1">1 – Terrible</mat-option>
        </mat-select>
      </mat-form-field>

      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Comments (Optional)</mat-label>
        <textarea
          matInput
          formControlName="comments"
          rows="4"
          placeholder="Tell us about your stay..."
          maxlength="500"
        ></textarea>
        <mat-hint align="end">{{ feedbackForm.value.comments?.length || 0 }}/500</mat-hint>
      </mat-form-field>
    </form>
  }
</mat-dialog-content>
<mat-dialog-actions align="end">
  <button mat-button mat-dialog-close>Cancel</button>
  @if (!existingFeedback() && !loading()) {
    <button
      mat-raised-button
      color="primary"
      [disabled]="feedbackForm.invalid || submitting()"
      (click)="submitFeedback()"
    >
      @if (submitting()) {
        <mat-spinner diameter="18" style="display: inline-block; margin-right: 8px;"></mat-spinner>
      }
      Submit
    </button>
  }
</mat-dialog-actions>


# /components/feedback-dialog/feedback-dialog.component.ts

import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormGroup, FormControl, Validators } from '@angular/forms';
import { MatDialogModule, MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { FeedbackApiService } from '../../services/feedback-api.service';
import { Feedback, CreateFeedbackDTO } from '../../models/feedback.model';
import { AlertComponent } from '../../../../features/auth/components/alert.component';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'app-feedback-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatSelectModule,
    MatInputModule,
    MatFormFieldModule,
    MatProgressSpinnerModule,
    AlertComponent
  ],
  templateUrl: './feedback-dialog.component.html',
  styles: [`
    .feedback-content {
      min-width: 320px;
      max-width: 480px;
    }
    .read-only-feedback {
      font-size: 1rem;
      line-height: 1.5;
    }
    .rating-value {
      font-size: 1.2rem;
      font-weight: 600;
      color: #3f51b5;
    }
    .full-width {
      width: 100%;
    }
  `]
})
export class FeedbackDialogComponent implements OnInit {
  readonly bookingId: number = inject(MAT_DIALOG_DATA);
  private readonly feedbackApi = inject(FeedbackApiService);
  private readonly dialogRef = inject(MatDialogRef<FeedbackDialogComponent>);

  existingFeedback = signal<Feedback | null>(null);
  loading = signal(false);
  submitting = signal(false);
  error = signal<string | null>(null);

  feedbackForm = new FormGroup({
    rating: new FormControl<number>(5, { validators: [Validators.required, Validators.min(1), Validators.max(5)], nonNullable: true }),
    comments: new FormControl<string>('', { nonNullable: true })
  });

  ngOnInit(): void {
    this.checkExistingFeedback();
  }

  checkExistingFeedback(): void {
    this.loading.set(true);
    this.error.set(null);

    this.feedbackApi.getByBookingId(this.bookingId)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (data) => {
          // If the backend returns 204 or null or empty object, existingFeedback will be null.
          if (data && data.id) {
            this.existingFeedback.set(data);
          } else {
            this.existingFeedback.set(null);
          }
        },
        error: (err) => {
          // A 404 might mean no feedback exists yet. Let's inspect status
          if (err.status === 404) {
            this.existingFeedback.set(null);
          } else {
            const message = err.error?.message || err.message || 'Error checking existing feedback.';
            this.error.set(message);
          }
        }
      });
  }

  submitFeedback(): void {
    if (this.feedbackForm.invalid) {
      return;
    }

    this.submitting.set(true);
    this.error.set(null);

    const dto: CreateFeedbackDTO = {
      bookingId: this.bookingId,
      rating: this.feedbackForm.value.rating!,
      comments: this.feedbackForm.value.comments ?? ''
    };

    this.feedbackApi.submit(dto)
      .pipe(finalize(() => this.submitting.set(false)))
      .subscribe({
        next: (response) => {
          this.dialogRef.close(response);
        },
        error: (err) => {
          const message = err.error?.message || err.message || 'Failed to submit feedback.';
          this.error.set(message);
        }
      });
  }
}


# /components/food-order/cart-drawer.component.html

<div
  class="cart-drawer"
  [class.open]="isOpen()"
>
  <button
    mat-raised-button
    class="cart-toggle-btn"
    (click)="cartToggle.emit()"
    aria-label="Toggle shopping cart"
  >
    <mat-icon>shopping_cart</mat-icon>
    Cart ({{ itemCount() }}) – {{ subtotal() | currency }}
  </button>

  @if (isOpen()) {
    <div class="cart-panel">
      <div class="cart-header">
        <h3>Shopping Cart</h3>
        <button mat-icon-button (click)="cartToggle.emit()" aria-label="Close cart">
          <mat-icon>close</mat-icon>
        </button>
      </div>

      <div class="cart-items-list">
        @for (item of cartItems(); track item.menuItemId) {
          <div class="cart-item">
            <span class="item-name">{{ item.name }}</span>
            <div class="qty-controls">
              <button type="button" mat-icon-button (click)="decrementQty(item.menuItemId)" aria-label="Decrease quantity">
                <mat-icon>remove</mat-icon>
              </button>
              <span class="qty">{{ item.quantity }}</span>
              <button type="button" mat-icon-button (click)="incrementQty(item.menuItemId)" aria-label="Increase quantity">
                <mat-icon>add</mat-icon>
              </button>
            </div>
            <span class="item-price">{{ item.price * item.quantity | currency }}</span>
          </div>
        } @empty {
          <p class="empty-cart">Your cart is empty.</p>
        }
      </div>

      <div class="cart-footer">
        <div class="total-row">
          <span>Total:</span>
          <span class="total-price">{{ subtotal() | currency }}</span>
        </div>
        <button
          mat-raised-button
          color="primary"
          class="checkout-btn"
          (click)="checkout.emit()"
          [disabled]="cartItems().length === 0"
        >
          Place Order
        </button>
      </div>
    </div>
  }
</div>


# /components/food-order/cart-drawer.component.scss

.cart-drawer {
  position: relative;

  .cart-toggle-btn {
    width: 100%;
    height: 48px;
    font-weight: 500;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 8px;
    border-radius: 8px;
    background-color: #f5f5f5;
    color: #333;
  }

  .cart-panel {
    position: absolute;
    top: 56px;
    right: 0;
    width: 320px;
    background: white;
    border: 1px solid rgba(0, 0, 0, 0.12);
    border-radius: 8px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
    z-index: 100;
    display: flex;
    flex-direction: column;
    max-height: 400px;
  }

  .cart-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 12px 16px;
    border-bottom: 1px solid #f0f0f0;

    h3 {
      margin: 0;
      font-size: 1.1rem;
      font-weight: 500;
    }
  }

  .cart-items-list {
    flex-grow: 1;
    overflow-y: auto;
    padding: 16px;
    display: flex;
    flex-direction: column;
    gap: 12px;
  }

  .cart-item {
    display: flex;
    justify-content: space-between;
    align-items: center;
    font-size: 0.95rem;
    gap: 8px;

    .item-name {
      color: rgba(0, 0, 0, 0.87);
      flex: 1 1 auto;
      min-width: 0;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }
    .qty-controls {
      display: flex;
      align-items: center;
      gap: 4px;
      flex-shrink: 0;

      button {
        width: 28px;
        height: 28px;
        line-height: 28px;
        display: flex;
        align-items: center;
        justify-content: center;

        ::ng-deep .mat-mdc-button-touch-target {
          display: none;
        }

        mat-icon {
          font-size: 18px;
          width: 18px;
          height: 18px;
        }
      }

      .qty {
        font-weight: 500;
        min-width: 16px;
        text-align: center;
      }
    }
    .item-price {
      font-weight: 500;
      flex-shrink: 0;
      min-width: 60px;
      text-align: right;
    }
  }

  .empty-cart {
    text-align: center;
    color: rgba(0, 0, 0, 0.54);
    font-style: italic;
    margin: 16px 0;
  }

  .cart-footer {
    padding: 16px;
    border-top: 1px solid #f0f0f0;
    background: #fafafa;
    border-bottom-left-radius: 8px;
    border-bottom-right-radius: 8px;

    .total-row {
      display: flex;
      justify-content: space-between;
      align-items: center;
      font-weight: 600;
      font-size: 1.05rem;
      margin-bottom: 12px;

      .total-price {
        color: #3f51b5;
        font-size: 1.1rem;
      }
    }

    .checkout-btn {
      width: 100%;
    }
  }
}

// Mobile bottom sheet styles
@media (max-width: 767px) {
  .cart-drawer {
    position: fixed;
    bottom: 0;
    left: 0;
    right: 0;
    background: white;
    box-shadow: 0 -4px 16px rgba(0, 0, 0, 0.15);
    border-top-left-radius: 16px;
    border-top-right-radius: 16px;
    z-index: 1000;
    padding: 12px 16px;
    display: flex;
    flex-direction: column;

    .cart-toggle-btn {
      margin-bottom: 8px;
    }

    &.open {
      height: 70vh;
    }

    .cart-panel {
      position: static;
      width: 100%;
      box-shadow: none;
      border: none;
      flex-grow: 1;
      max-height: none;
      display: flex;
    }

    .cart-items-list {
      max-height: calc(70vh - 180px);
    }
  }
}


# /components/food-order/cart-drawer.component.ts

import { Component, input, output, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { OrderItem } from '../../models/order-item.model';

@Component({
  selector: 'app-cart-drawer',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule],
  templateUrl: './cart-drawer.component.html',
  styleUrls: ['./cart-drawer.component.scss']
})
export class CartDrawerComponent {
  cartItems = input.required<OrderItem[]>();
  isOpen = input.required<boolean>();

  cartToggle = output<void>();
  checkout = output<void>();
  updateQuantity = output<{ menuItemId: number; delta: number }>();

  itemCount = computed(() => this.cartItems().reduce((s, i) => s + i.quantity, 0));
  subtotal = computed(() => this.cartItems().reduce((s, i) => s + i.price * i.quantity, 0));

  incrementQty(menuItemId: number): void {
    this.updateQuantity.emit({ menuItemId, delta: 1 });
  }

  decrementQty(menuItemId: number): void {
    this.updateQuantity.emit({ menuItemId, delta: -1 });
  }
}


# /components/food-order/food-order.component.html

<div class="food-order-container">
  @if (loading()) {
    <div class="spinner-container">
      <mat-spinner diameter="40"></mat-spinner>
    </div>
  } @else if (error()) {
    <app-alert type="error" [message]="error()!" (closed)="error.set(null)">
      <button mat-button (click)="fetchMenuItems()">Retry</button>
    </app-alert>
  } @else {
    <div class="food-order-layout">
      <div class="menu-section">
        <mat-form-field appearance="outline" style="width: 100%; max-width: 300px; margin-bottom: 16px; display: block;">
          <mat-label>Deliver to Room</mat-label>
          <mat-select [formControl]="selectedRoomId">
            @for (room of validRooms(); track room.roomId) {
              <mat-option [value]="room.roomId">
                {{ room.roomNumber ?? 'Room ' + room.roomId }}
              </mat-option>
            }
          </mat-select>
          @if (selectedRoomId.invalid && selectedRoomId.touched) {
            <mat-error>Please select a room for delivery.</mat-error>
          }
        </mat-form-field>

        <app-menu-grid
          [menuItems]="menuItems()"
          [cartItems]="cartItems()"
          (addToCart)="onAddToCart($event)"
          (updateQuantity)="onUpdateCartQty($event)"
        />
      </div>

      <div class="cart-section">
        <app-cart-drawer
          [cartItems]="cartItems()"
          [isOpen]="cartOpen()"
          (cartToggle)="cartOpen.set(!cartOpen())"
          (checkout)="placeOrder()"
          (updateQuantity)="onUpdateCartQty($event)"
        />
      </div>
    </div>
  }
</div>


# /components/food-order/food-order.component.scss

.food-order-container {
  padding: 16px 0;

  .spinner-container {
    display: flex;
    justify-content: center;
    padding: 32px;
  }

  .food-order-layout {
    display: flex;
    gap: 24px;
    align-items: flex-start;
  }

  .menu-section {
    flex-grow: 1;
  }

  .cart-section {
    width: 320px;
    flex-shrink: 0;
    position: sticky;
    top: 24px;
  }
}

@media (max-width: 1024px) {
  .food-order-container {
    .food-order-layout {
      flex-direction: column;
      align-items: stretch;
    }

    .cart-section {
      width: 100%;
      position: static;
    }
  }
}

@media (max-width: 767px) {
  .food-order-container {
    padding-bottom: 72px; /* make space for fixed bottom sheet cart drawer */
  }
}


# /components/food-order/food-order.component.ts

import { Component, OnInit, inject, signal, computed, input, output, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl, Validators } from '@angular/forms';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { ConfirmDialogComponent } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MenuItemApiService } from '../../services/menu-item-api.service';
import { OrderApiService } from '../../services/order-api.service';
import { MenuGridComponent } from './menu-grid.component';
import { CartDrawerComponent } from './cart-drawer.component';
import { MenuItem } from '../../../../features/admin/models/menu-item.model';
import { BookingRoom } from '../../../../features/admin/models/booking.model';
import { OrderItem } from '../../models/order-item.model';
import { finalize } from 'rxjs/operators';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AlertComponent } from '../../../../features/auth/components/alert.component';

@Component({
  selector: 'app-food-order',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    MatDialogModule,
    MatFormFieldModule,
    MatSelectModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MenuGridComponent,
    CartDrawerComponent,
    AlertComponent
  ],
  templateUrl: './food-order.component.html',
  styleUrls: ['./food-order.component.scss']
})
export class FoodOrderComponent implements OnInit {
  activeBookingId = input.required<number>();
  rooms = input.required<BookingRoom[]>();
  orderPlaced = output<void>();

  private readonly menuApi = inject(MenuItemApiService);
  private readonly orderApi = inject(OrderApiService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly dialog = inject(MatDialog);
  private readonly destroyRef = inject(DestroyRef);

  selectedRoomId = new FormControl<number>(0, { nonNullable: true, validators: Validators.required });

  menuItems = signal<MenuItem[]>([]);
  cartItems = signal<OrderItem[]>([]);
  cartOpen = signal(false);

  loading = signal(false);
  error = signal<string | null>(null);
  submitting = signal(false);

  validRooms = computed(() => this.rooms().filter((r): r is typeof r & { roomId: number } => r.roomId !== null));
  canCheckout = computed(() => this.cartItems().length > 0);
  subtotal = computed(() => this.cartItems().reduce((s, i) => s + i.price * i.quantity, 0));

  ngOnInit(): void {
    this.fetchMenuItems();
    const roomsList = this.validRooms();
    if (roomsList.length > 0) {
      this.selectedRoomId.setValue(roomsList[0].roomId);
    }
  }

  fetchMenuItems(): void {
    this.loading.set(true);
    this.error.set(null);

    this.menuApi.getAll({ isAvailable: true, pageSize: 200 })
      .pipe(
        finalize(() => this.loading.set(false)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: (res) => this.menuItems.set(res.data),
        error: (err) => {
          const msg = err.error?.message || err.message || 'Failed to load menu items.';
          this.error.set(msg);
        }
      });
  }

  onAddToCart(item: MenuItem): void {
    this.cartItems.update((items) => {
      const idx = items.findIndex((i) => i.menuItemId === item.id);
      if (idx > -1) {
        const updated = [...items];
        updated[idx] = {
          ...updated[idx],
          quantity: updated[idx].quantity + 1
        };
        return updated;
      } else {
        return [...items, { menuItemId: item.id, name: item.name, price: item.price, quantity: 1 }];
      }
    });

    const snackRef = this.snackBar.open(`Added ${item.name} to cart.`, 'View Cart', {
      duration: 4000
    });

    snackRef.onAction().subscribe(() => {
      this.cartOpen.set(true);
    });
  }

  onUpdateCartQty(event: { menuItemId: number; delta: number }): void {
    this.cartItems.update(items => {
      const index = items.findIndex(i => i.menuItemId === event.menuItemId);
      if (index === -1) return items;
      const newQty = items[index].quantity + event.delta;
      if (newQty <= 0) {
        return items.filter(i => i.menuItemId !== event.menuItemId);
      }
      return items.map(i => i.menuItemId === event.menuItemId ? { ...i, quantity: newQty } : i);
    });
  }

  placeOrder(): void {
    if (!this.canCheckout() || this.submitting()) {
      return;
    }
    if (this.selectedRoomId.invalid) {
      this.selectedRoomId.markAsTouched();
      return;
    }

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Confirm Order',
        message: `Place this order? Total: $${this.subtotal().toFixed(2)}`
      }
    });

    dialogRef.afterClosed().pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe((confirmed) => {
      if (confirmed) {
        this.submitOrder();
      }
    });
  }

  private submitOrder(): void {
    if (this.selectedRoomId.invalid) {
      this.selectedRoomId.markAsTouched();
      return;
    }
    this.submitting.set(true);
    const dto = {
      bookingId: this.activeBookingId(),
      roomId: this.selectedRoomId.value,
      items: this.cartItems().map((i) => ({
        menuItemId: i.menuItemId,
        quantity: i.quantity
      }))
    };

    this.orderApi.create(dto)
      .pipe(
        finalize(() => this.submitting.set(false)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: () => {
          this.snackBar.open('Order placed successfully!', 'Close', { duration: 4000 });
          this.cartItems.set([]);
          this.cartOpen.set(false);
          this.orderPlaced.emit();
        },
        error: (err) => {
          const msg = typeof err.error === 'string' ? err.error : (err.error?.message || 'Failed to place order.');
          this.snackBar.open(msg, 'Close', { duration: 5000 });
        }
      });
  }
}


# /components/food-order/menu-grid.component.html

<div class="menu-categories">
  <div class="filter-row">
    <mat-form-field appearance="outline" class="category-select">
      <mat-label>Category</mat-label>
      <mat-select [formControl]="categoryFilter">
        <mat-option value="All">All</mat-option>
        @for (cat of categories(); track cat) {
          <mat-option [value]="cat">{{ cat }}</mat-option>
        }
      </mat-select>
    </mat-form-field>
  </div>

  @for (group of filteredGroups(); track group.category) {
    <div class="category-section">
      <h3 class="category-title">{{ group.category }}</h3>
      <div class="menu-grid">
        @for (item of group.items; track item.id) {
          <mat-card class="menu-item-card">
            <mat-card-header>
              <mat-card-title>{{ item.name }}</mat-card-title>
            </mat-card-header>
            <mat-card-content>
              <div class="price-row">
                <span class="price">{{ item.price | currency }}</span>
              </div>
            </mat-card-content>
            <mat-card-actions>
              @if (getQuantity(item.id) === 0) {
                <button
                  mat-raised-button
                  color="primary"
                  (click)="increment(item)"
                  aria-label="Add to cart"
                  class="action-btn"
                >
                  <mat-icon>add_shopping_cart</mat-icon> Add to Cart
                </button>
              } @else {
                <div class="inline-qty-controls">
                  <button
                    type="button"
                    mat-icon-button
                    (click)="decrement(item)"
                    aria-label="Decrease quantity"
                  >
                    <mat-icon>remove</mat-icon>
                  </button>
                  <span class="qty-display">{{ getQuantity(item.id) }}</span>
                  <button
                    type="button"
                    mat-icon-button
                    (click)="increment(item)"
                    aria-label="Increase quantity"
                  >
                    <mat-icon>add</mat-icon>
                  </button>
                </div>
              }
            </mat-card-actions>
          </mat-card>
        }
      </div>
    </div>
  } @empty {
    <p class="no-items">No menu items available at the moment.</p>
  }
</div>


# /components/food-order/menu-grid.component.scss

.menu-categories {
  .category-section {
    margin-bottom: 32px;
  }

  .category-title {
    font-size: 1.4rem;
    font-weight: 500;
    margin: 16px 0 12px;
    padding-bottom: 8px;
    border-bottom: 2px solid #e0e0e0;
    color: rgba(0, 0, 0, 0.87);
  }

  .filter-row {
    margin-bottom: 24px;
    
    .category-select {
      width: 200px;
      @media (max-width: 599px) {
        width: 100%;
      }
    }
  }

  .menu-grid {
    display: grid;
    grid-template-columns: repeat(3, 1fr);
    gap: 16px;
    padding: 8px 0;

    .menu-item-card {
      display: flex;
      flex-direction: column;
      justify-content: space-between;
      height: 100%;

      mat-card-header {
        margin-bottom: 8px;
      }

      .price-row {
        font-size: 1.2rem;
        font-weight: 600;
        color: #3f51b5;
      }

      mat-card-actions {
        padding: 8px 16px 16px 16px;

        .action-btn {
          width: 100%;
        }

        .inline-qty-controls {
          display: flex;
          align-items: center;
          justify-content: space-between;
          width: 100%;
          border: 1px solid rgba(0, 0, 0, 0.12);
          border-radius: 4px;
          height: 36px;
          box-sizing: border-box;

          button {
            width: 36px;
            height: 36px;
            display: flex;
            align-items: center;
            justify-content: center;
            line-height: 36px;
            border-radius: 0;

            ::ng-deep .mat-mdc-button-touch-target {
              display: none;
            }
          }

          .qty-display {
            font-weight: 600;
            font-size: 1rem;
            color: rgba(0, 0, 0, 0.87);
          }
        }
      }
    }
  }

  .no-items {
    text-align: center;
    padding: 32px;
    color: rgba(0, 0, 0, 0.54);
    font-style: italic;
  }
}

@media (max-width: 1024px) {
  .menu-categories {
    .menu-grid {
      grid-template-columns: repeat(2, 1fr);
    }
  }
}

@media (max-width: 767px) {
  .menu-categories {
    .menu-grid {
      grid-template-columns: 1fr;
    }
  }
}


# /components/food-order/menu-grid.component.ts

import { Component, input, output, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl } from '@angular/forms';
import { toSignal } from '@angular/core/rxjs-interop';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MenuItem } from '../../../../features/admin/models/menu-item.model';
import { OrderItem } from '../../models/order-item.model';

@Component({
  selector: 'app-menu-grid',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatSelectModule,
    MatFormFieldModule
  ],
  templateUrl: './menu-grid.component.html',
  styleUrls: ['./menu-grid.component.scss']
})
export class MenuGridComponent {
  menuItems = input.required<MenuItem[]>();
  cartItems = input<OrderItem[]>([]);
  addToCart = output<MenuItem>();
  updateQuantity = output<{ menuItemId: number; delta: number }>();

  categoryFilter = new FormControl('All', { nonNullable: true });
  private categoryFilterSignal = toSignal(this.categoryFilter.valueChanges, { initialValue: this.categoryFilter.value });

  cartMap = computed(() => {
    const map: Record<number, number> = {};
    const items = this.cartItems() || [];
    for (const item of items) {
      map[item.menuItemId] = item.quantity;
    }
    return map;
  });

  getQuantity(menuItemId: number): number {
    return this.cartMap()[menuItemId] || 0;
  }

  increment(item: MenuItem): void {
    const current = this.getQuantity(item.id);
    if (current === 0) {
      this.addToCart.emit(item);
    } else {
      this.updateQuantity.emit({ menuItemId: item.id, delta: 1 });
    }
  }

  decrement(item: MenuItem): void {
    const current = this.getQuantity(item.id);
    if (current > 0) {
      this.updateQuantity.emit({ menuItemId: item.id, delta: -1 });
    }
  }

  categories = computed(() => {
    const cats = new Set(this.menuItems().map(i => i.category || 'Other'));
    return Array.from(cats).sort();
  });

  filteredGroups = computed(() => {
    const selected = this.categoryFilterSignal() ?? 'All';
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
}


# /components/my-requests/my-requests.component.html

<div class="my-requests">
  @if (loading() && requests().length === 0) {
    <div class="spinner-container">
      <mat-spinner diameter="30"></mat-spinner>
    </div>
  } @else if (error()) {
    <app-alert
      type="error"
      [message]="error()!"
      (closed)="error.set(null)"
    >
      <button
        mat-button
        (click)="fetchRequests()"
      >
        Retry
      </button>
    </app-alert>
  }

  @if (requests().length > 0) {
    <div class="table-container">
      <table
        mat-table
        [dataSource]="requests()"
        matSort
        matSortDisableClear
      >
        <!-- Type Column -->
        <ng-container matColumnDef="type">
          <th
            mat-header-cell
            *matHeaderCellDef
          >
            Type
          </th>
          <td
            mat-cell
            *matCellDef="let r"
          >
            {{ r.type }}
          </td>
        </ng-container>

        <!-- Room Column -->
        <ng-container matColumnDef="room">
          <th
            mat-header-cell
            *matHeaderCellDef
          >
            Room
          </th>
          <td
            mat-cell
            *matCellDef="let r"
          >
            {{ r.roomNumber }}
          </td>
        </ng-container>

        <!-- Description Column -->
        <ng-container matColumnDef="description">
          <th
            mat-header-cell
            *matHeaderCellDef
          >
            Description
          </th>
          <td
            mat-cell
            *matCellDef="let r"
          >
            {{ r.description }}
          </td>
        </ng-container>

        <!-- Status Column -->
        <ng-container matColumnDef="status">
          <th
            mat-header-cell
            *matHeaderCellDef
          >
            Status
          </th>
          <td
            mat-cell
            *matCellDef="let r"
          >
            <span class="status-badge" [class]="r.status.toLowerCase()">
              {{ r.status }}
            </span>
          </td>
        </ng-container>

        <!-- Created Column -->
        <ng-container matColumnDef="createdAt">
          <th
            mat-header-cell
            *matHeaderCellDef
          >
            Created
          </th>
          <td
            mat-cell
            *matCellDef="let r"
          >
            {{ r.createdAt | date:'short' }}
          </td>
        </ng-container>

        <tr
          mat-header-row
          *matHeaderRowDef="displayedColumns"
        ></tr>
        <tr
          mat-row
          *matRowDef="let row; columns: displayedColumns"
        ></tr>
      </table>
    </div>
  } @else if (!loading()) {
    <p class="no-requests">No housekeeping, maintenance, or food order requests found.</p>
  }
</div>


# /components/my-requests/my-requests.component.scss

.my-requests {
  padding: 16px 0;

  .spinner-container {
    display: flex;
    justify-content: center;
    padding: 24px;
  }

  .table-container {
    overflow-x: auto;
    width: 100%;
    border: 1px solid rgba(0, 0, 0, 0.12);
    border-radius: 4px;
  }

  table {
    width: 100%;
    border-collapse: collapse;

    .clickable-row {
      cursor: pointer;
      transition: background-color 0.2s ease;
      &:hover {
        background-color: rgba(0, 0, 0, 0.04);
      }
    }
  }

  .status-badge {
    display: inline-block;
    padding: 4px 8px;
    border-radius: 4px;
    font-size: 0.85rem;
    font-weight: 500;

    &.pending {
      background-color: #fff3e0;
      color: #e65100;
    }

    &.inprogress {
      background-color: #e8eaf6;
      color: #1a237e;
    }

    &.completed {
      background-color: #e8f5e9;
      color: #1b5e20;
    }
  }

  .no-requests {
    padding: 24px;
    text-align: center;
    color: rgba(0, 0, 0, 0.54);
    font-size: 1.1rem;
    font-style: italic;
  }
}

@media (max-width: 599px) {
  .my-requests {
    overflow-x: auto;
    table {
      min-width: 600px;
    }
    .mat-mdc-cell, .mat-mdc-header-cell {
      padding: 8px 4px;
      font-size: 0.85rem;
    }
  }
}


# /components/my-requests/my-requests.component.ts

import { Component, input, effect, inject, signal, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatSortModule } from '@angular/material/sort';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonModule } from '@angular/material/button';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { forkJoin, Observable, of } from 'rxjs';
import { map, finalize, catchError, switchMap } from 'rxjs/operators';
import { HousekeepingApiService } from '../../services/housekeeping-api.service';
import { MaintenanceApiService } from '../../services/maintenance-api.service';
import { OrderApiService } from '../../services/order-api.service';
import { CustomerRequest } from '../../models/customer-request.model';
import { AlertComponent } from '../../../../features/auth/components/alert.component';

@Component({
  selector: 'app-my-requests',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatSortModule,
    MatProgressSpinnerModule,
    MatButtonModule,
    AlertComponent
  ],
  templateUrl: './my-requests.component.html',
  styleUrls: ['./my-requests.component.scss']
})
export class MyRequestsComponent {
  roomIds = input.required<number[]>();
  bookingId = input<number | null>(null);
  refresh = input(0);

  private readonly housekeepingApi = inject(HousekeepingApiService);
  private readonly maintenanceApi = inject(MaintenanceApiService);
  private readonly orderApi = inject(OrderApiService);
  private readonly destroyRef = inject(DestroyRef);

  requests = signal<CustomerRequest[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);
  displayedColumns = ['type', 'room', 'description', 'status', 'createdAt'];

  constructor() {
    effect(() => {
      // Trigger fetch when roomIds or refresh trigger changes
      const ids = this.roomIds();
      const ref = this.refresh();
      if (ids && ids.length > 0) {
        this.fetchRequests();
      } else {
        this.requests.set([]);
      }
    });
  }

  fetchRequests(): void {
    const ids = this.roomIds();
    if (ids.length === 0) {
      this.requests.set([]);
      return;
    }

    this.loading.set(true);
    this.error.set(null);

    const obsList: Observable<CustomerRequest[]>[] = [];
    ids.forEach((roomId) => {
      obsList.push(
        this.housekeepingApi.getAll({ pageSize: 100 }).pipe(
          map((res) =>
            res.data
              .filter((hk) => hk.roomId === roomId)
              .map((hk) => ({
                id: hk.id,
                type: 'Housekeeping' as const,
                roomId: hk.roomId,
                roomNumber: hk.location ?? `Room ${hk.roomId}`,
                description: hk.description ?? '',
                status: hk.status,
                createdAt: hk.createdAt
              }))
          ),
          catchError(() => of([]))
        )
      );
      obsList.push(
        this.maintenanceApi.getAll({ pageSize: 100 }).pipe(
          map((res) =>
            res.data
              .filter((m) => m.roomId === roomId)
              .map((m) => ({
                id: m.id,
                type: 'Maintenance' as const,
                roomId: m.roomId,
                roomNumber: m.location ?? `Room ${m.roomId}`,
                description: m.description ?? '',
                status: m.status,
                createdAt: m.createdAt
              }))
          ),
          catchError(() => of([]))
        )
      );
    });

    // Add food orders if bookingId is available
    const bId = this.bookingId();
    if (bId != null) {
      const food$ = this.orderApi.getAll({ status: 'Pending', pageSize: 50 }).pipe(
        switchMap((res: any) =>
          this.orderApi.getAll({ status: 'Preparing', pageSize: 50 }).pipe(
            map((res2: any) =>
              [...res.data, ...res2.data]
                .filter((o: any) => o.bookingId === bId)
                .map((o: any) => ({
                  id: o.id,
                  type: 'Food Order' as const,
                  roomId: o.roomId ?? 0,
                  roomNumber: o.roomNumber ?? 'N/A',
                  description: `Order #${o.id}`,
                  status: o.orderStatus ?? 'Pending',
                  createdAt: o.generatedAt ?? new Date().toISOString()
                }))
            )
          )
        ),
        catchError(() => of([]))
      );
      obsList.push(food$);
    }

    forkJoin(obsList)
      .pipe(
        finalize(() => this.loading.set(false)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: (results) => {
          const merged = results.reduce((acc, curr) => acc.concat(curr), []);
          // Sort by createdAt descending
          merged.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
          this.requests.set(merged);
        },
        error: (err) => {
          const msg = err.error?.message || err.message || 'Failed to fetch requests.';
          this.error.set(msg);
        }
      });
  }
}


# /components/request-service-dialog.component.html

<div class="dialog-container">
  <h2 mat-dialog-title>
    {{ data.type === 'housekeeping' ? 'Request Housekeeping' : 'Request Maintenance' }}
  </h2>
  <mat-dialog-content>
    <mat-form-field appearance="outline" class="full-width">
      <mat-label>Room</mat-label>
      <input matInput [value]="data.roomNumber" readonly aria-label="Room number (read only)" />
    </mat-form-field>
    <mat-form-field appearance="outline" class="full-width">
      <mat-label>Description</mat-label>
      <textarea
        matInput
        [formControl]="descriptionControl"
        rows="4"
        placeholder="Describe your request..."
        aria-label="Request description"
      ></textarea>
      @if (descriptionControl.invalid && descriptionControl.touched) {
        <mat-error>Description is required.</mat-error>
      }
    </mat-form-field>
  </mat-dialog-content>
  <mat-dialog-actions align="end">
    <button mat-button mat-dialog-close aria-label="Cancel request">Cancel</button>
    <button mat-raised-button color="primary" (click)="submit()" aria-label="Submit request">Submit</button>
  </mat-dialog-actions>
</div>


# /components/request-service-dialog.component.ts

import { Component, inject } from '@angular/core';
import { ReactiveFormsModule, FormControl, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

export interface RequestServiceDialogData {
  roomNumber: string;
  roomId: number;
  type: 'housekeeping' | 'maintenance';
}

export interface RequestServiceDialogResult {
  description: string;
}

@Component({
  selector: 'app-request-service-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
  ],
  templateUrl: './request-service-dialog.component.html',
})
export class RequestServiceDialogComponent {
  readonly data: RequestServiceDialogData = inject(MAT_DIALOG_DATA);
  private readonly dialogRef = inject(MatDialogRef<RequestServiceDialogComponent>);

  descriptionControl = new FormControl<string>('', { validators: [Validators.required], nonNullable: true });

  submit(): void {
    this.descriptionControl.markAsTouched();
    if (this.descriptionControl.invalid) {
      return;
    }
    const result: RequestServiceDialogResult = { description: this.descriptionControl.value };
    this.dialogRef.close(result);
  }
}


# /components/request-service/request-service.component.html

<div class="request-service">
  <mat-card class="request-card">
    <mat-card-header>
      <mat-card-title>Request Housekeeping or Maintenance</mat-card-title>
    </mat-card-header>
    <mat-card-content class="form-container">
      <div class="form-row">
        @if (isMobile()) {
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>Service Type</mat-label>
            <mat-select [formControl]="requestType">
              <mat-option value="housekeeping">Housekeeping</mat-option>
              <mat-option value="maintenance">Maintenance</mat-option>
            </mat-select>
          </mat-form-field>
        } @else {
          <mat-button-toggle-group
            [formControl]="requestType"
            aria-label="Service type"
            class="type-toggle-group"
          >
            <mat-button-toggle value="housekeeping">
              <mat-icon>cleaning_services</mat-icon> Housekeeping
            </mat-button-toggle>
            <mat-button-toggle value="maintenance">
              <mat-icon>build</mat-icon> Maintenance
            </mat-button-toggle>
          </mat-button-toggle-group>
        }
      </div>

      <div class="form-row">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Room</mat-label>
          <mat-select [formControl]="selectedRoomId">
            @for (room of activeBooking().rooms; track room.roomId) {
              <mat-option [value]="room.roomId">
                {{ room.roomNumber ?? 'Room ' + room.roomId }}
              </mat-option>
            }
          </mat-select>
        </mat-form-field>
      </div>

      <div class="form-row">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Description</mat-label>
          <textarea
            matInput
            [formControl]="description"
            rows="3"
            placeholder="Please detail your request..."
          ></textarea>
          @if (description.invalid && description.touched) {
            <mat-error>Description is required (minimum 5 characters).</mat-error>
          }
        </mat-form-field>
      </div>
    </mat-card-content>
    <mat-card-actions>
      <button
        mat-raised-button
        color="primary"
        (click)="submitRequest()"
        [disabled]="description.invalid || submitting()"
      >
        @if (submitting()) {
          <mat-spinner diameter="20" style="display: inline-block; margin-right: 8px;"></mat-spinner>
        }
        Submit Request
      </button>
    </mat-card-actions>
  </mat-card>
</div>


# /components/request-service/request-service.component.scss

.request-service {
  padding: 16px 0;
  max-width: 600px;
  margin: 0 auto;

  .request-card {
    padding: 16px;
  }

  .form-container {
    display: flex;
    flex-direction: column;
    gap: 16px;
    margin-top: 16px;
  }

  .type-toggle-group {
    width: 100%;
    display: flex;

    mat-button-toggle {
      flex: 1;
      text-align: center;
    }
  }

  .full-width {
    width: 100%;
  }

  mat-card-actions {
    justify-content: flex-end;
    padding: 8px 16px 16px 16px;
  }
}

@media (max-width: 599px) {
  .request-service {
    mat-card {
      margin: 8px;
      padding: 12px;
    }
    mat-form-field {
      width: 100%;
    }
    mat-button-toggle-group {
      width: 100%;
      display: flex;
      mat-button-toggle {
        flex: 1 1 50%;
      }
    }
  }
}


# /components/request-service/request-service.component.ts

import { Component, OnInit, inject, signal, input, output, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { BreakpointObserver } from '@angular/cdk/layout';
import { toSignal } from '@angular/core/rxjs-interop';
import { map } from 'rxjs/operators';
import { ConfirmDialogComponent } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';
import { HousekeepingApiService } from '../../services/housekeeping-api.service';
import { MaintenanceApiService } from '../../services/maintenance-api.service';
import { Booking } from '../../../../features/admin/models/booking.model';
import { finalize } from 'rxjs/operators';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-request-service',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonToggleModule,
    MatSelectModule,
    MatInputModule,
    MatFormFieldModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatDialogModule
  ],
  templateUrl: './request-service.component.html',
  styleUrls: ['./request-service.component.scss']
})
export class RequestServiceComponent implements OnInit {
  activeBooking = input.required<Booking>();
  requestCreated = output<void>();

  private readonly housekeepingApi = inject(HousekeepingApiService);
  private readonly maintenanceApi = inject(MaintenanceApiService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly dialog = inject(MatDialog);
  private readonly breakpointObserver = inject(BreakpointObserver);
  private readonly destroyRef = inject(DestroyRef);

  isMobile = toSignal(
    this.breakpointObserver.observe('(max-width: 599px)').pipe(map(r => r.matches)),
    { initialValue: false }
  );

  requestType = new FormControl<'housekeeping' | 'maintenance'>('housekeeping', { nonNullable: true });
  selectedRoomId = new FormControl<number>(0, { nonNullable: true, validators: [Validators.required] });
  description = new FormControl<string>('', {
    nonNullable: true,
    validators: [Validators.required, Validators.minLength(5)]
  });

  submitting = signal(false);

  ngOnInit(): void {
    const rooms = this.activeBooking().rooms || [];
    if (rooms.length > 0 && rooms[0].roomId != null) {
      this.selectedRoomId.setValue(rooms[0].roomId);
    }
  }

  submitRequest(): void {
    if (this.description.invalid || this.submitting()) {
      this.description.markAsTouched();
      return;
    }

    const roomId = this.selectedRoomId.value;
    if (!roomId) return;

    const room = this.activeBooking().rooms.find(r => r.roomId === roomId);
    const roomLabel = room?.roomNumber ?? 'selected room';
    const typeLabel = this.requestType.value === 'housekeeping' ? 'Housekeeping' : 'Maintenance';

    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Confirm Service Request',
        message: `Send a ${typeLabel} request for ${roomLabel}?`
      }
    });

    dialogRef.afterClosed().pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe((confirmed) => {
      if (confirmed) {
        this.performSubmit(roomId);
      }
    });
  }

  private performSubmit(roomId: number): void {
    this.submitting.set(true);
    const type = this.requestType.value;
    const desc = this.description.value;

    const request$ = type === 'housekeeping'
      ? this.housekeepingApi.trigger(roomId, { description: desc })
      : this.maintenanceApi.trigger(roomId, { description: desc });

    request$.pipe(
      finalize(() => this.submitting.set(false)),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: () => {
        this.snackBar.open(`${type === 'housekeeping' ? 'Housekeeping' : 'Maintenance'} request submitted successfully.`, 'Close', {
          duration: 4000
        });
        this.description.reset('');
        this.requestCreated.emit();
      },
      error: (err) => {
        const msg = err.error?.message || err.message || 'Failed to submit request.';
        this.snackBar.open(msg, 'Close', { duration: 5000 });
      }
    });
  }
}


# /facades/customer-booking.facade.ts

import { Injectable, inject } from '@angular/core';
import { Observable, of } from 'rxjs';
import { map, switchMap } from 'rxjs/operators';
import { AuthApiService } from '../../../core/services/auth-api.service';
import { BookingApiService } from '../services/booking-api.service';
import { Booking } from '../../../features/admin/models/booking.model';

export interface CustomerProfile {
  firstName: string;
  lastName: string;
  email: string;
}

@Injectable({ providedIn: 'root' })
export class CustomerBookingFacade {
  private readonly authApi = inject(AuthApiService);
  private readonly bookingApi = inject(BookingApiService);

  getActiveBooking(): Observable<Booking | null> {
    return this.authApi.getMe().pipe(
      switchMap((me) => {
        const email =
          me.claims?.find((c) => c.type === 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name')?.value ?? '';
        if (!email) return of(null);
        return this.bookingApi
          .getAll({
            guestQuery: email,
            status: 'CheckedIn',
            pageNumber: 1,
            pageSize: 1,
            sortBy: 'bookedAt',
            sortDescending: true
          })
          .pipe(map((res) => (res.data.length > 0 ? res.data[0] : null)));
      })
    );
  }

  getCurrentCustomerProfile(): Observable<CustomerProfile> {
    return this.authApi.getMe().pipe(
      map((me) => ({
        firstName:
          me.claims?.find((c) => c.type === 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname')?.value ??
          '',
        lastName:
          me.claims?.find((c) => c.type === 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname')?.value ?? '',
        email:
          me.claims?.find((c) => c.type === 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name')?.value ?? '',
      }))
    );
  }
}


# /models/auth-me-response.model.ts

export type { Claim, AuthMeResponse } from '../../../core/models/auth-me-response.model';


# /models/available-room-type.model.ts

export interface AvailableRoomType {
  roomTypeId: number;
  name: string;
  basePrice: number;
  maxOccupancy: number;
  description: string | null;
  imageUrls: string[] | null;
  squareFootage: number | null;
  bedConfiguration: Record<string, number> | null;
  availableCount: number;
}


# /models/billing-folio.model.ts

export interface BillingFolio {
  bookingId: number;
  guestName: string;
  nightsStayed: number;
  roomBasePrice: number;
  roomTotal: number;
  foodTotal: number;
  amenityTotal: number;
  totalBill: number;
  paymentStatus: string;
  foodItems: string[];
  amenityItems: string[];
}


# /models/booking.model.ts

export type { Booking, BookingRoom } from '../../../features/admin/models/booking.model';


# /models/customer-request.model.ts

export interface CustomerRequest {
  id: number;
  type: 'Housekeeping' | 'Maintenance' | 'Food Order';
  roomId: number;
  roomNumber: string;
  description: string;
  status: string;
  createdAt: string;
}


# /models/feedback.model.ts

export type { Feedback } from '../../../features/admin/models/feedback.model';

export interface CreateFeedbackDTO {
  bookingId: number;
  rating: number;
  comments: string;
}


# /models/order-item.model.ts

export interface OrderItem {
  menuItemId: number;
  name: string;
  price: number;
  quantity: number;
}


# /pages/bookings.component.html

<div class="bookings-page">
  <div class="toggle-row">
    <mat-button-toggle-group
      [formControl]="viewMode"
      aria-label="View"
    >
      <mat-button-toggle value="new">New Booking</mat-button-toggle>
      <mat-button-toggle value="history">My Bookings</mat-button-toggle>
    </mat-button-toggle-group>
  </div>

  @if (userProfile()) {
    @if (viewMode.value === 'history') {
      <app-booking-history
        [userEmail]="userEmail()"
        [refresh]="refreshTrigger()"
        [highlightBookingId]="newBookingId()"
      />
    } @if (viewMode.value === 'new') {
      <app-booking-wizard
        [userProfile]="userProfile()!"
        [initialCheckIn]="initialCheckIn"
        [initialCheckOut]="initialCheckOut"
        [initialGuests]="initialGuests"
        [initialRoomTypeId]="initialRoomTypeId"
        (bookingCreated)="onBookingCreated($event)"
      />
    }
  } @else {
    <div style="display: flex; justify-content: center; padding: 24px;">
      <mat-spinner diameter="40"></mat-spinner>
    </div>
  }
</div>


# /pages/bookings.component.scss

.bookings-page {
  padding: 24px;

  .toggle-row {
    margin-bottom: 24px;

    mat-button-toggle-group {
      border-radius: 24px;
      overflow: hidden;
      border: 1px solid rgba(0, 0, 0, 0.12);

      .mat-button-toggle {
        border-radius: 0;
        border: none;
      }

      ::ng-deep .mat-button-toggle-checked {
        background-color: #1976d2 !important; // primary color
        color: white !important;
      }
    }
  }
}


# /pages/bookings.component.ts

import { Component, inject, signal, OnInit, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AuthApiService } from '../../../core/services/auth-api.service';
import { BookingHistoryComponent } from '../components/booking-history/booking-history.component';
import { BookingWizardComponent } from '../components/booking-wizard/booking-wizard.component';

@Component({
  selector: 'app-customer-bookings',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatButtonToggleModule,
    MatProgressSpinnerModule,
    BookingHistoryComponent,
    BookingWizardComponent
  ],
  templateUrl: './bookings.component.html',
  styleUrls: ['./bookings.component.scss']
})
export class BookingsComponent implements OnInit {
  viewMode = new FormControl<'history' | 'new'>('new', { nonNullable: true });
  userEmail = signal('');
  userProfile = signal<{ firstName: string; lastName: string; email: string } | null>(null);

  refreshTrigger = signal(0);
  newBookingId = signal<number | null>(null);

  initialCheckIn: Date | null = null;
  initialCheckOut: Date | null = null;
  initialGuests: number | null = null;
  initialRoomTypeId: number | null = null;

  private readonly authApi = inject(AuthApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);

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

    this.authApi.getMe().pipe(takeUntilDestroyed(this.destroyRef)).subscribe(me => {
      const given = me.claims?.find(c => c.type === 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname')?.value ?? '';
      const surname = me.claims?.find(c => c.type === 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname')?.value ?? '';
      const email = me.claims?.find(c => c.type === 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name')?.value ?? '';
      this.userEmail.set(email);
      this.userProfile.set({ firstName: given, lastName: surname, email });
    });
  }

  onBookingCreated(bookingId: number): void {
    this.newBookingId.set(bookingId);
    this.refreshTrigger.update(n => n + 1);
    this.viewMode.setValue('history');
  }
}


# /pages/dashboard.component.html

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
            <p><strong>Check&#8209;in:</strong> {{ currentBooking()!.checkInDate }}</p>
            <p><strong>Check&#8209;out:</strong> {{ currentBooking()!.checkOutDate }}</p>
            <p><strong>Status:</strong> {{ currentBooking()!.bookingStatus }}</p>
          </mat-card-content>
          <mat-card-actions>
            <button mat-raised-button color="accent" (click)="openServiceRequest('housekeeping')" aria-label="Request housekeeping">
              <mat-icon>cleaning_services</mat-icon> Request Housekeeping
            </button>
            <button mat-raised-button color="warn" (click)="openServiceRequest('maintenance')" aria-label="Request maintenance">
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
            <p><strong>Check&#8209;in:</strong> {{ upcomingBooking()!.checkInDate }}</p>
            <p><strong>Check&#8209;out:</strong> {{ upcomingBooking()!.checkOutDate }}</p>
            <p><strong>Status:</strong> {{ upcomingBooking()!.bookingStatus }}</p>
            @if (upcomingRoomTypes().length > 0) {
              <p><strong>Room Type(s):</strong> {{ upcomingRoomTypes().join(', ') }}</p>
            }
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

    @if (currentBooking()) {
      <div class="room-service-status">
        <h2>Room Service Status</h2>
        <div class="status-grid">
          <!-- Housekeeping -->
          <mat-card class="status-card">
            <mat-card-header>
              <mat-card-title>Housekeeping</mat-card-title>
              <mat-card-subtitle>{{ pendingHousekeeping().length }} pending / in-progress</mat-card-subtitle>
            </mat-card-header>
            <mat-card-content>
              @for (item of pendingHousekeeping(); track item.id) {
                <div class="status-item">
                  <p class="status-line">
                    <span>{{ item.description || 'No description' }}</span>
                    <span class="badge" [class]="item.status.toLowerCase()">{{ item.status }}</span>
                  </p>
                </div>
              } @empty {
                <p class="no-status-items">No pending requests.</p>
              }
            </mat-card-content>
          </mat-card>

          <!-- Maintenance -->
          <mat-card class="status-card">
            <mat-card-header>
              <mat-card-title>Maintenance</mat-card-title>
              <mat-card-subtitle>{{ pendingMaintenance().length }} pending / in-progress</mat-card-subtitle>
            </mat-card-header>
            <mat-card-content>
              @for (item of pendingMaintenance(); track item.id) {
                <div class="status-item">
                  <p class="status-line">
                    <span>{{ item.description || 'No description' }}</span>
                    <span class="badge" [class]="item.status.toLowerCase()">{{ item.status }}</span>
                  </p>
                </div>
              } @empty {
                <p class="no-status-items">No pending requests.</p>
              }
            </mat-card-content>
          </mat-card>

          <!-- Food Orders -->
          <mat-card class="status-card">
            <mat-card-header>
              <mat-card-title>Food Orders</mat-card-title>
              <mat-card-subtitle>{{ pendingFoodOrders().length }} preparing / pending</mat-card-subtitle>
            </mat-card-header>
            <mat-card-content>
              @for (order of pendingFoodOrders(); track order.id) {
                <div class="status-item">
                  <p class="status-line">
                    <span>Order #{{ order.id }}</span>
                    <span class="badge" [class]="(order.orderStatus || 'pending').toLowerCase()">{{ order.orderStatus || 'Pending' }}</span>
                  </p>
                </div>
              } @empty {
                <p class="no-status-items">No pending orders.</p>
              }
            </mat-card-content>
          </mat-card>
        </div>
      </div>
    }
  }
</div>


# /pages/dashboard.component.scss

.dashboard {
  padding: 24px;

  h1 {
    margin-bottom: 24px;
    font-size: 1.75rem;
    font-weight: 500;
  }

  .booking-cards {
    display: flex;
    flex-wrap: wrap;
    gap: 16px;

    .booking-card {
      flex: 1 1 300px;

      mat-card-actions {
        display: flex;
        flex-wrap: wrap;
        gap: 8px;
        padding: 8px 16px 16px;
      }
    }
  }

  .room-service-status {
    margin-top: 32px;

    h2 {
      font-size: 1.4rem;
      font-weight: 500;
      margin-bottom: 16px;
    }

    .status-grid {
      display: flex;
      flex-wrap: wrap;
      gap: 16px;

      .status-card {
        flex: 1 1 300px;
        max-width: 100%;
        box-shadow: 0 2px 4px rgba(0,0,0,0.05);

        mat-card-header {
          margin-bottom: 12px;
          border-bottom: 1px solid #f0f0f0;
          padding-bottom: 8px;
        }

        .status-item {
          padding: 8px 0;
          border-bottom: 1px dashed #f0f0f0;
          &:last-child {
            border-bottom: none;
          }

          p {
            margin: 0;
          }

          .status-line {
            display: flex;
            justify-content: space-between;
            align-items: center;
            font-size: 0.95rem;
          }

          .description {
            font-size: 0.85rem;
            color: rgba(0, 0, 0, 0.54);
            margin-top: 4px;
            white-space: nowrap;
            overflow: hidden;
            text-overflow: ellipsis;
          }

          .badge {
            display: inline-block;
            padding: 2px 6px;
            border-radius: 4px;
            font-size: 0.8rem;
            font-weight: 500;

            &.pending {
              background-color: #fff3e0;
              color: #e65100;
            }
            &.inprogress, &.preparing {
              background-color: #e8eaf6;
              color: #1a237e;
            }
            &.completed {
              background-color: #e8f5e9;
              color: #1b5e20;
            }
          }
        }

        .no-status-items {
          text-align: center;
          color: rgba(0, 0, 0, 0.54);
          font-style: italic;
          margin: 16px 0 8px;
        }
      }
    }
  }
}

@media (max-width: 599px) {
  .dashboard {
    .room-service-status {
      .status-grid {
        flex-direction: column;
      }
    }
  }
}


# /pages/dashboard.component.ts

import { CommonModule } from '@angular/common';
import { Component, inject, signal, OnInit } from '@angular/core';
import { Router } from '@angular/router';
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
import { AuthApiService } from '../../../core/services/auth-api.service';
import { BookingApiService } from '../services/booking-api.service';
import { HousekeepingApiService } from '../services/housekeeping-api.service';
import { MaintenanceApiService } from '../services/maintenance-api.service';
import { CustomerBookingFacade } from '../facades/customer-booking.facade';
import { of, switchMap } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { Booking } from '../../admin/models/booking.model';
import { AlertComponent } from '../../auth/components/alert.component';
import { RequestServiceDialogComponent, RequestServiceDialogData, RequestServiceDialogResult } from '../components/request-service-dialog.component';
import { RoomTypeApiService } from '../services/room-type-api.service';
import { OrderApiService } from '../services/order-api.service';
import { CustomerRequest } from '../models/customer-request.model';

@Component({
  selector: 'app-customer-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatDividerModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSnackBarModule,
    AlertComponent,
  ],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss'],
})
export class PlaceholderCustomerDashboardComponent implements OnInit {
  firstName = signal('');
  loading = signal(false);
  error = signal<string | null>(null);
  currentBooking = signal<Booking | null>(null);
  upcomingBooking = signal<Booking | null>(null);
  upcomingRoomTypes = signal<string[]>([]);
  pendingHousekeeping = signal<CustomerRequest[]>([]);
  pendingMaintenance = signal<CustomerRequest[]>([]);
  pendingFoodOrders = signal<any[]>([]);

  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);
  private readonly authApi = inject(AuthApiService);
  private readonly bookingApi = inject(BookingApiService);
  private readonly housekeepingApi = inject(HousekeepingApiService);
  private readonly maintenanceApi = inject(MaintenanceApiService);
  private readonly roomTypeApi = inject(RoomTypeApiService);
  private readonly orderApi = inject(OrderApiService);
  private readonly bookingFacade = inject(CustomerBookingFacade);
  private readonly destroyRef = inject(DestroyRef);
  private readonly router = inject(Router);

  ngOnInit(): void {
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
    this.loadDashboard();
  }

  loadDashboard(): void {
    this.loading.set(true);
    this.error.set(null);

    this.bookingFacade.getCurrentCustomerProfile().pipe(
      takeUntilDestroyed(this.destroyRef),
      switchMap((profile) => {
        this.firstName.set(profile.firstName);
        if (!profile.email) {
          return forkJoin({
            active: of(null),
            upcoming: of({ data: [] as Booking[] })
          });
        }
        return forkJoin({
          active: this.bookingFacade.getActiveBooking(),
          upcoming: this.bookingApi.getAll({
            guestQuery: profile.email,
            status: 'Booked',
            pageNumber: 1,
            pageSize: 1,
            sortBy: 'checkInDate',
            sortDescending: false
          })
        });
      }),
      finalize(() => this.loading.set(false))
    ).subscribe({
      next: ({ active, upcoming }) => {
        this.currentBooking.set(active);
        this.upcomingBooking.set(upcoming.data.length > 0 ? upcoming.data[0] : null);
        if (active) {
          this.loadRoomServiceStatus();
        } else {
          this.pendingHousekeeping.set([]);
          this.pendingMaintenance.set([]);
          this.pendingFoodOrders.set([]);
        }
        if (upcoming.data.length > 0) {
          this.loadUpcomingRoomTypes(upcoming.data[0]);
        } else {
          this.upcomingRoomTypes.set([]);
        }
      },
      error: (err: unknown) => {
        this.error.set(this.extractErrorMessage(err));
      }
    });
  }

  private loadUpcomingRoomTypes(booking: Booking): void {
    if (!booking.rooms || booking.rooms.length === 0) {
      this.upcomingRoomTypes.set([]);
      return;
    }
    const ids = [...new Set(booking.rooms.map(r => r.roomTypeId))];
    const requests = ids.map(id =>
      this.roomTypeApi.getById(id).pipe(
        catchError(() => of(null)),
        map(rt => rt?.name ?? `Room Type ${id}`)
      )
    );
    forkJoin(requests).pipe(takeUntilDestroyed(this.destroyRef)).subscribe(names => {
      this.upcomingRoomTypes.set(names);
    });
  }

  private loadRoomServiceStatus(): void {
    const booking = this.currentBooking();
    if (!booking) return;
    const roomIds = booking.rooms.map(r => r.roomId).filter(id => id != null) as number[];
    if (roomIds.length === 0) return;

    // Helper to fetch housekeeping/maintenance for a single status
    const fetchHousekeeping = (status: string) =>
      forkJoin(roomIds.map(roomId =>
        this.housekeepingApi.getAll({ roomId, status, pageSize: 20 }).pipe(
          map(res => res.data.map(hk => ({
            ...hk,
            type: 'Housekeeping' as const,
            roomNumber: hk.location ?? `Room ${hk.roomId}`,
            description: hk.description ?? ''
          }))),
          catchError(() => of([]))
        )
      )).pipe(map(results => results.flat()));

    const fetchMaintenance = (status: string) =>
      forkJoin(roomIds.map(roomId =>
        this.maintenanceApi.getAll({ roomId, status, pageSize: 20 }).pipe(
          map(res => res.data.map(mt => ({
            ...mt,
            type: 'Maintenance' as const,
            roomNumber: mt.location ?? `Room ${mt.roomId}`,
            description: mt.description ?? ''
          }))),
          catchError(() => of([]))
        )
      )).pipe(map(results => results.flat()));

    // Fetch both statuses for housekeeping and maintenance
    forkJoin({
      hkPending: fetchHousekeeping('Pending'),
      hkInProgress: fetchHousekeeping('InProgress'),
      mtPending: fetchMaintenance('Pending'),
      mtInProgress: fetchMaintenance('InProgress'),
      food: this.orderApi.getAll({ status: 'Pending', pageSize: 50 }).pipe(
        switchMap((res: any) => {
          return this.orderApi.getAll({ status: 'Preparing', pageSize: 50 }).pipe(
            map((res2: any) => [...res.data, ...res2.data].filter(o => o.bookingId === booking.id))
          );
        }),
        catchError(() => of([]))
      )
    }).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: ({ hkPending, hkInProgress, mtPending, mtInProgress, food }) => {
        this.pendingHousekeeping.set([...hkPending, ...hkInProgress]);
        this.pendingMaintenance.set([...mtPending, ...mtInProgress]);
        // Normalize status field (API may return 'status' or 'orderStatus')
        this.pendingFoodOrders.set(
          (food as any[]).map((o: any) => ({
            ...o,
            orderStatus: o.orderStatus ?? o.status ?? 'Pending'
          }))
        );
      },
      error: (err: any) => this.snackBar.open(this.extractErrorMessage(err), 'Close', { duration: 5000 })
    });
  }

  openServiceRequest(type: 'housekeeping' | 'maintenance'): void {
    const booking = this.currentBooking();
    if (!booking || !booking.rooms.length || booking.rooms[0].roomId === null) {
      return;
    }

    const roomId = booking.rooms[0].roomId as number;
    const roomNumber = booking.rooms[0].roomNumber ?? roomId.toString();

    const data: RequestServiceDialogData = { roomNumber, roomId, type };
    const dialogRef = this.dialog.open<RequestServiceDialogComponent, RequestServiceDialogData, RequestServiceDialogResult>(
      RequestServiceDialogComponent,
      { data, width: '420px' }
    );

    dialogRef.afterClosed().pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe((result: RequestServiceDialogResult | undefined) => {
      if (!result) return;

      const api$ = type === 'housekeeping'
        ? this.housekeepingApi.trigger(roomId, { description: result.description })
        : this.maintenanceApi.trigger(roomId, { description: result.description });

      api$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
        next: () => {
          this.snackBar.open(
            type === 'housekeeping' ? 'Housekeeping request submitted.' : 'Maintenance request submitted.',
            'Close',
            { duration: 4000 }
          );
        },
        error: (err: unknown) => {
          this.snackBar.open(this.extractErrorMessage(err), 'Close', { duration: 5000 });
        }
      });
    });
  }

  getRoomNumbers(booking: Booking): string {
    return booking.rooms
      .filter(r => r.roomNumber !== null)
      .map(r => r.roomNumber as string)
      .join(', ') || '—';
  }

  private extractErrorMessage(err: unknown): string {
    if (typeof err === 'string') return err;
    const e = err as { error?: { message?: string }; message?: string };
    if (e?.error?.message) return e.error.message;
    if (e?.message) return e.message;
    return 'An unexpected error occurred.';
  }
}


# /pages/room-service.component.html

<div class="room-service">
  @if (loadingActiveBooking()) {
    <div style="display: flex; justify-content: center; padding: 32px;">
      <mat-spinner diameter="40"></mat-spinner>
    </div>
  } @else if (activeBookingError()) {
    <app-alert
      type="error"
      [message]="activeBookingError()!"
      (closed)="activeBookingError.set(null)"
    >
      <button
        mat-button
        (click)="loadActiveBooking()"
      >
        Retry
      </button>
    </app-alert>
  } @else if (activeBooking(); as booking) {
    <mat-tab-group>
      <mat-tab label="Food Order">
        <app-food-order
          [activeBookingId]="booking.id"
          [rooms]="booking.rooms"
          (orderPlaced)="onOrderPlaced()"
        />
      </mat-tab>
      <mat-tab label="Request Service">
        <app-request-service
          [activeBooking]="booking"
          (requestCreated)="onRequestCreated()"
        />
      </mat-tab>
      <mat-tab label="My Requests">
        <app-my-requests
          [roomIds]="roomIds()"
          [bookingId]="booking.id"
          [refresh]="refreshRequests()"
        />
      </mat-tab>
    </mat-tab-group>
  } @else {
    <mat-card class="no-booking-card">
      <mat-card-content class="no-booking-content">
        <mat-icon class="info-icon">info</mat-icon>
        <p>You need an active stay (Checked In) to use room service.</p>
        <p>Please visit <a routerLink="/user/bookings">My Bookings</a>.</p>
      </mat-card-content>
    </mat-card>
  }
</div>


# /pages/room-service.component.scss

.room-service {
  padding: 24px;

  .no-booking-card {
    max-width: 480px;
    margin: 40px auto;
    text-align: center;
    padding: 24px;
    border-radius: 8px;
    border: 1px solid rgba(0, 0, 0, 0.12);
  }

  .no-booking-content {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 12px;

    .info-icon {
      font-size: 40px;
      width: 40px;
      height: 40px;
      color: #3f51b5;
    }

    p {
      margin: 0;
      font-size: 1.1rem;
      color: rgba(0, 0, 0, 0.87);

      a {
        color: #3f51b5;
        text-decoration: none;
        font-weight: 500;
        &:hover {
          text-decoration: underline;
        }
      }
    }
  }
}


# /pages/room-service.component.ts

import { Component, inject, signal, computed, OnInit, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatTabsModule } from '@angular/material/tabs';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs/operators';
import { CustomerBookingFacade } from '../facades/customer-booking.facade';
import { Booking } from '../../admin/models/booking.model';
import { AlertComponent } from '../../../features/auth/components/alert.component';
import { FoodOrderComponent } from '../components/food-order/food-order.component';
import { RequestServiceComponent } from '../components/request-service/request-service.component';
import { MyRequestsComponent } from '../components/my-requests/my-requests.component';

@Component({
  selector: 'app-customer-room-service',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatTabsModule,
    MatProgressSpinnerModule,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    AlertComponent,
    FoodOrderComponent,
    RequestServiceComponent,
    MyRequestsComponent
  ],
  templateUrl: './room-service.component.html',
  styleUrls: ['./room-service.component.scss']
})
export class RoomServiceComponent implements OnInit {
  private readonly facade = inject(CustomerBookingFacade);
  private readonly destroyRef = inject(DestroyRef);

  activeBooking = signal<Booking | null>(null);
  loadingActiveBooking = signal(false);
  activeBookingError = signal<string | null>(null);
  refreshRequests = signal(0);

  roomIds = computed(() => {
    const booking = this.activeBooking();
    return booking ? booking.rooms.map(r => r.roomId).filter(id => id != null) as number[] : [];
  });

  ngOnInit(): void {
    this.loadActiveBooking();
  }

  loadActiveBooking(): void {
    this.loadingActiveBooking.set(true);
    this.activeBookingError.set(null);

    this.facade.getActiveBooking().pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.loadingActiveBooking.set(false))
    ).subscribe({
      next: booking => this.activeBooking.set(booking),
      error: (err: any) => this.activeBookingError.set(this.extractErrorMessage(err))
    });
  }

  onOrderPlaced(): void {
    // Only show a snackbar or log – no need to refresh My Requests tab.
  }

  onRequestCreated(): void {
    this.refreshRequests.update(n => n + 1);
  }

  private extractErrorMessage(err: any): string {
    return err.error?.message || err.message || 'An unexpected error occurred.';
  }
}


# /services/amenity-api.service.ts

import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Amenity } from '../../../features/admin/models/amenity.model';
import { PaginatedResponse } from '../../../core/models/paginated-response.model';

@Injectable({ providedIn: 'root' })
export class AmenityApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.baseUrl}/amenities`;

  getAll(params: {
    pageNumber: number;
    pageSize: number;
    isAvailable?: boolean;
  }): Observable<PaginatedResponse<Amenity>> {
    let httpParams = new HttpParams()
      .set('pageNumber', params.pageNumber.toString())
      .set('pageSize', params.pageSize.toString());

    if (params.isAvailable !== undefined) {
      httpParams = httpParams.set('isAvailable', params.isAvailable.toString());
    }

    return this.http.get<PaginatedResponse<Amenity>>(this.baseUrl, { params: httpParams });
  }
}


# /services/auth-api.service.ts

export { AuthApiService } from '../../../core/services/auth-api.service';


# /services/billing-api.service.ts

import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { BillingFolio } from '../models/billing-folio.model';

@Injectable({ providedIn: 'root' })
export class BillingApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.baseUrl}/billing`;

  getByBookingId(bookingId: number): Observable<BillingFolio> {
    return this.http.get<BillingFolio>(`${this.baseUrl}/${bookingId}`);
  }

  pay(bookingId: number, dto: { amount: number; paymentMethod: string; transactionId: string }): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${bookingId}/pay`, dto);
  }
}


# /services/booking-api.service.ts

import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Booking } from '../../../features/admin/models/booking.model';
import { PaginatedResponse } from '../../../core/models/paginated-response.model';

@Injectable({ providedIn: 'root' })
export class BookingApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.baseUrl}/bookings`;

  getAll(params: {
    status?: string;
    bookingStatus?: string;
    guestQuery?: string;
    movementStatus?: string;
    pageNumber?: number;
    pageSize?: number;
    sortBy?: string;
    sortDescending?: boolean;
  }): Observable<PaginatedResponse<Booking>> {
    let httpParams = new HttpParams()
      .set('pageNumber', (params.pageNumber ?? 1).toString())
      .set('pageSize', (params.pageSize ?? 10).toString())
      .set('sortBy', params.sortBy ?? 'id')
      .set('sortDescending', (params.sortDescending ?? false).toString());

    if (params.status) {
      httpParams = httpParams.set('bookingStatus', params.status);
    }
    if (params.bookingStatus) {
      httpParams = httpParams.set('bookingStatus', params.bookingStatus);
    }
    if (params.guestQuery) {
      httpParams = httpParams.set('guestQuery', params.guestQuery);
    }
    if (params.movementStatus) {
      httpParams = httpParams.set('movementStatus', params.movementStatus);
    }

    return this.http.get<PaginatedResponse<Booking>>(this.baseUrl, { params: httpParams });
  }

  create(booking: {
    roomTypeIds: number[];
    guestCount: number;
    checkInDate: string;
    checkOutDate: string;
    guestName?: string;
    guestEmail?: string;
    amenityIds?: number[];
  }): Observable<Booking> {
    return this.http.post<Booking>(this.baseUrl, booking);
  }

  cancel(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}/cancel`);
  }

  checkIn(id: number): Observable<Booking> {
    return this.http.post<Booking>(`${this.baseUrl}/${id}/checkin`, {});
  }

  extendStay(id: number, dto: { checkOutDate: string }): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/${id}/extend-stay`, dto);
  }

  checkOut(id: number): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/checkout`, {});
  }
}


# /services/feedback-api.service.ts

import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Feedback, CreateFeedbackDTO } from '../models/feedback.model';

@Injectable({ providedIn: 'root' })
export class FeedbackApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.baseUrl}/feedback`;

  getByBookingId(bookingId: number): Observable<Feedback | null> {
    return this.http.get<Feedback | null>(`${this.baseUrl}/booking/${bookingId}`);
  }

  submit(dto: CreateFeedbackDTO): Observable<Feedback> {
    return this.http.post<Feedback>(this.baseUrl, dto);
  }
}


# /services/housekeeping-api.service.ts

import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { HousekeepingTask } from '../../admin/models/housekeeping-task.model';
import { PaginatedResponse } from '../../../core/models/paginated-response.model';

@Injectable({ providedIn: 'root' })
export class HousekeepingApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.baseUrl}/housekeeping`;

  trigger(roomId: number, body: { description: string }): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/trigger/${roomId}`, body);
  }

  getAll(params?: {
    pageNumber?: number;
    pageSize?: number;
    status?: string;
    roomId?: number;
    sortBy?: string;
    sortDescending?: boolean;
  }): Observable<PaginatedResponse<HousekeepingTask>> {
    let httpParams = new HttpParams();
    if (params) {
      if (params.pageNumber) httpParams = httpParams.set('pageNumber', params.pageNumber.toString());
      if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());
      if (params.status) httpParams = httpParams.set('status', params.status);
      if (params.roomId) httpParams = httpParams.set('roomId', params.roomId.toString());
      if (params.sortBy) httpParams = httpParams.set('sortBy', params.sortBy);
      if (params.sortDescending !== undefined) httpParams = httpParams.set('sortDescending', params.sortDescending.toString());
    }
    return this.http.get<PaginatedResponse<HousekeepingTask>>(this.baseUrl, { params: httpParams });
  }

  createInternal(body: { location: string; description: string }): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/internal`, body);
  }

  updateStatus(id: number, dto: { status: string }): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/${id}/status`, dto);
  }
}


# /services/maintenance-api.service.ts

import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { MaintenanceTask } from '../../admin/models/maintenance-task.model';
import { PaginatedResponse } from '../../../core/models/paginated-response.model';

@Injectable({ providedIn: 'root' })
export class MaintenanceApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.baseUrl}/maintenance`;

  trigger(roomId: number, body: { description: string }): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/trigger/${roomId}`, body);
  }

  getAll(params?: {
    pageNumber?: number;
    pageSize?: number;
    status?: string;
    roomId?: number;
    sortBy?: string;
    sortDescending?: boolean;
  }): Observable<PaginatedResponse<MaintenanceTask>> {
    let httpParams = new HttpParams();
    if (params) {
      if (params.pageNumber) httpParams = httpParams.set('pageNumber', params.pageNumber.toString());
      if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());
      if (params.status) httpParams = httpParams.set('status', params.status);
      if (params.roomId) httpParams = httpParams.set('roomId', params.roomId.toString());
      if (params.sortBy) httpParams = httpParams.set('sortBy', params.sortBy);
      if (params.sortDescending !== undefined) httpParams = httpParams.set('sortDescending', params.sortDescending.toString());
    }
    return this.http.get<PaginatedResponse<MaintenanceTask>>(this.baseUrl, { params: httpParams });
  }

  createInternal(body: { location: string; description: string }): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/internal`, body);
  }

  updateStatus(id: number, dto: { status: string }): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/${id}/status`, dto);
  }
}


# /services/menu-item-api.service.ts

import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { MenuItem } from '../../../features/admin/models/menu-item.model';
import { PaginatedResponse } from '../../../core/models/paginated-response.model';

@Injectable({ providedIn: 'root' })
export class MenuItemApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.baseUrl}/menu-items`;

  getAll(params?: {
    isAvailable?: boolean;
    pageSize?: number;
    pageNumber?: number;
  }): Observable<PaginatedResponse<MenuItem>> {
    let httpParams = new HttpParams();
    if (params) {
      if (params.isAvailable !== undefined) {
        httpParams = httpParams.set('isAvailable', params.isAvailable.toString());
      }
      if (params.pageSize) {
        httpParams = httpParams.set('pageSize', params.pageSize.toString());
      }
      if (params.pageNumber) {
        httpParams = httpParams.set('pageNumber', params.pageNumber.toString());
      }
    }
    return this.http.get<PaginatedResponse<MenuItem>>(this.baseUrl, { params: httpParams });
  }
}


# /services/order-api.service.ts

import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

export interface CreateFoodOrderDTO {
  bookingId: number;
  roomId: number;
  items: { menuItemId: number; quantity: number }[];
}

@Injectable({ providedIn: 'root' })
export class OrderApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.baseUrl}/orders`;

  create(dto: CreateFoodOrderDTO): Observable<any> {
    return this.http.post<any>(this.baseUrl, dto);
  }

  getAll(params?: any): Observable<any> {
    return this.http.get<any>(this.baseUrl, { params });
  }

  updateStatus(id: number, dto: { status: string }): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/${id}`, dto);
  }
}


# /services/room-type-api.service.ts

import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { AvailableRoomType } from '../models/available-room-type.model';
import { PaginatedResponse } from '../../../core/models/paginated-response.model';

@Injectable({ providedIn: 'root' })
export class RoomTypeApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.baseUrl}/room-types`;

  getAvailable(
    checkIn: string,
    checkOut: string,
    pageNumber: number = 1,
    pageSize: number = 100
  ): Observable<PaginatedResponse<AvailableRoomType>> {
    const params = new HttpParams()
      .set('checkIn', checkIn)
      .set('checkOut', checkOut)
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<PaginatedResponse<AvailableRoomType>>(`${this.baseUrl}/availability`, { params });
  }

  getById(id: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/${id}`);
  }
}


# /user-shell.component.html

<mat-sidenav-container>
  <!-- SIDEBAR -->
  <mat-sidenav
    #sidenav
    [mode]="isMobile() ? 'over' : 'side'"
    [opened]="isMobile() ? sidebarOpen() : true"
    aria-label="Customer navigation">
    <mat-toolbar color="primary">Hotel</mat-toolbar>
    <mat-nav-list>
      <a mat-list-item routerLink="/user/dashboard" routerLinkActive="active" (click)="onNavClick()">
        <mat-icon matListItemIcon aria-hidden="true">dashboard</mat-icon>
        <span matListItemTitle>Dashboard</span>
      </a>
      <a mat-list-item routerLink="/user/bookings" routerLinkActive="active" (click)="onNavClick()">
        <mat-icon matListItemIcon aria-hidden="true">book_online</mat-icon>
        <span matListItemTitle>My Bookings</span>
      </a>
      <a mat-list-item routerLink="/user/room-service" routerLinkActive="active" (click)="onNavClick()">
        <mat-icon matListItemIcon aria-hidden="true">room_service</mat-icon>
        <span matListItemTitle>Room Service</span>
      </a>
    </mat-nav-list>
  </mat-sidenav>

  <!-- MAIN CONTENT -->
  <mat-sidenav-content>
    <mat-toolbar color="primary">
      @if (isMobile()) {
        <button mat-icon-button (click)="sidebarOpen.set(!sidebarOpen())">
          <mat-icon aria-hidden="true">menu</mat-icon>
        </button>
      }
      <span>Hotel</span>
      <span class="spacer"></span>
      <button mat-icon-button [matMenuTriggerFor]="userMenu" aria-label="Open user menu">
        <mat-icon aria-hidden="true">account_circle</mat-icon>
      </button>
      <mat-menu #userMenu="matMenu">
        <button mat-menu-item routerLink="/user/profile">
          <mat-icon aria-hidden="true">manage_accounts</mat-icon> Profile
        </button>
        <button mat-menu-item (click)="logout()">
          <mat-icon aria-hidden="true">logout</mat-icon> Logout
        </button>
      </mat-menu>
    </mat-toolbar>

    <!-- ROUTER OUTLET -->
    <div class="content">
      <router-outlet></router-outlet>
    </div>
  </mat-sidenav-content>
</mat-sidenav-container>


# /user-shell.component.scss

mat-sidenav-container {
  height: 100vh;
  width: 100%;
}

mat-sidenav {
  width: 250px;
  border-right: 1px solid rgba(0, 0, 0, 0.12);

  mat-toolbar {
    position: sticky;
    top: 0;
    z-index: 2;
  }
}

mat-sidenav-content {
  display: flex;
  flex-direction: column;
  height: 100%;

  mat-toolbar {
    position: sticky;
    top: 0;
    z-index: 2;
  }
}

.spacer {
  flex: 1 1 auto;
}

.content {
  padding: 24px;
  flex-grow: 1;
  overflow-y: auto;
  box-sizing: border-box;
}

.active {
  background-color: rgba(63, 81, 181, 0.08);
  color: #3f51b5 !important;
  font-weight: 500;

  mat-icon {
    color: #3f51b5;
  }
}

@media (max-width: 1024px) {
  .content {
    padding: 16px;
  }
}


# /user-shell.component.ts

import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { RouterModule, Router } from '@angular/router';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatDividerModule } from '@angular/material/divider';
import { BreakpointObserver } from '@angular/cdk/layout';
import { map } from 'rxjs/operators';
import { toSignal } from '@angular/core/rxjs-interop';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-user-shell',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatSidenavModule,
    MatToolbarModule,
    MatListModule,
    MatIconModule,
    MatButtonModule,
    MatMenuModule,
    MatDividerModule,
  ],
  templateUrl: './user-shell.component.html',
  styleUrls: ['./user-shell.component.scss'],
})
export class UserShellComponent {
  private breakpointObserver = inject(BreakpointObserver);
  private authService = inject(AuthService);
  private router = inject(Router);

  isMobile = toSignal(
    this.breakpointObserver.observe('(max-width: 1024px)').pipe(map(r => r.matches)),
    { initialValue: false }
  );

  sidebarOpen = signal(false);

  onNavClick(): void {
    if (this.isMobile()) {
      this.sidebarOpen.set(false);
    }
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/auth']);
  }
}
