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
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { RouterModule } from '@angular/router';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { BookingApiService } from '../../services/booking-api.service';
import { BillingApiService } from '../../services/billing-api.service';
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
    MatTooltipModule,
    MatProgressSpinnerModule,
    MatProgressBarModule,
    MatDialogModule,
    MatSnackBarModule,
    AlertComponent,
    RouterModule,
  ],
  templateUrl: './booking-history.component.html',
  styleUrls: ['./booking-history.component.scss'],
})
export class BookingHistoryComponent implements AfterViewInit {
  userEmail = input.required<string>();
  refresh = input(0);
  highlightBookingId = input<number | null>(null);

  private readonly bookingApi = inject(BookingApiService);
  private readonly billingApi = inject(BillingApiService);
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

    this.bookingApi
      .getAll({
        guestQuery: this.userEmail(),
        status: this.statusFilter.value || undefined,
        pageNumber: this.pageIndex + 1,
        pageSize: this.pageSize,
        sortBy: this.sortField,
        sortDescending: this.sortDescending,
      })
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
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
        },
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
    const assignedRooms = booking.rooms
      .filter((r) => r.roomNumber !== null)
      .map((r) => r.roomNumber as string)
      .join(', ');

    if (assignedRooms) {
      return assignedRooms;
    }

    return booking.bookingStatus === 'Cancelled'
      ? 'Booking Cancelled'
      : booking.rooms[0]?.roomAssignmentText || 'Room Not Assigned Yet';
  }

  openDetail(booking: Booking): void {
    this.dialog.open(BookingDetailDialogComponent, {
      data: booking,
      width: '500px',
    });
  }

  cancelBooking(booking: Booking): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Cancel Booking',
        message: `Are you sure you want to cancel booking #${booking.id}?`,
      },
    });

    dialogRef.afterClosed().subscribe((confirmed: boolean) => {
      if (confirmed) {
        this.loading.set(true);
        this.bookingApi
          .cancel(booking.id)
          .pipe(finalize(() => this.loading.set(false)))
          .subscribe({
            next: () => {
              this.snackBar.open('Booking successfully cancelled.', 'Close', { duration: 4000 });
              this.fetchData();
            },
            error: (err) => {
              const message = err.error?.message || err.message || 'Failed to cancel booking.';
              this.snackBar.open(message, 'Close', { duration: 5000 });
            },
          });
      }
    });
  }

  openFeedback(booking: Booking): void {
    this.dialog.open(FeedbackDialogComponent, {
      data: booking.id,
      width: '450px',
    });
  }

  openBilling(booking: Booking): void {
    this.dialog.open(BillingDialogComponent, {
      data: booking.id,
      width: '500px',
    });
  }

  downloadFolioPdf(bookingId: number, event: Event): void {
    event.stopPropagation();
    this.billingApi.downloadFolioPdf(bookingId).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `Aetheris_Folio_BK-${bookingId}.pdf`;
        a.click();
        window.URL.revokeObjectURL(url);
      },
      error: () => console.error('Failed to download folio PDF for booking', bookingId),
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
        pageSize: this.pageSize,
      };
      sessionStorage.setItem(this.STORAGE_KEY, JSON.stringify(state));
    } catch (e) {
      console.error('Error saving history state:', e);
    }
  }
}
