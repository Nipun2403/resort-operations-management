import { CommonModule } from '@angular/common';
import { Component, inject, signal, OnInit } from '@angular/core';
import { ReactiveFormsModule, FormControl } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { MatChipsModule } from '@angular/material/chips';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DestroyRef } from '@angular/core';
import { debounceTime, distinctUntilChanged, finalize } from 'rxjs';

import { BookingApiService } from '../../services/booking-api.service';
import { BillingApiService } from '../../services/billing-api.service';
import { Booking } from '../../models/booking.model';
import { Receipt } from '../../models/receipt.model';
import { AlertComponent } from '../../../../features/auth/components/alert.component';
import { BookingDetailDialogComponent } from './booking-detail-dialog.component';
import { ReceiptDetailDialogComponent } from './receipt-detail-dialog.component';

type BookingSortField = 'id' | 'bookingStatus' | 'bookedAt';
type ReceiptSortField = 'id' | 'amountPaid' | 'paidAt';

@Component({
  selector: 'app-billing-receipts',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatTableModule,
    MatSortModule,
    MatPaginatorModule,
    MatButtonToggleModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatProgressSpinnerModule,
    MatProgressBarModule,
    MatDialogModule,
    MatCardModule,
    MatDividerModule,
    MatChipsModule,
    AlertComponent,
  ],
  templateUrl: './billing-receipts.component.html',
  styleUrls: ['./billing-receipts.component.scss'],
})
export class BillingReceiptsComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly bookingApi = inject(BookingApiService);
  private readonly billingApi = inject(BillingApiService);
  private readonly dialog = inject(MatDialog);

  private readonly STORAGE_KEY = 'billingReceiptsState';

  // Active view toggle
  activeView = new FormControl<'bookings' | 'receipts'>('bookings', { nonNullable: true });

  // Bookings state
  bookings = signal<Booking[]>([]);
  bookingsTotal = signal(0);
  bookingsLoading = signal(false);
  bookingsError = signal<string | null>(null);
  bookingPage = signal(0);
  bookingPageSize = signal(10);
  bookingSortField = signal<BookingSortField>('bookedAt');
  bookingSortDescending = signal(true);

  // Bookings filter controls
  bookingSearch = new FormControl('', { nonNullable: true });
  bookingStatus = new FormControl('', { nonNullable: true });

  // Receipts state
  receipts = signal<Receipt[]>([]);
  receiptsTotal = signal(0);
  receiptsLoading = signal(false);
  receiptsError = signal<string | null>(null);
  receiptPage = signal(0);
  receiptPageSize = signal(10);
  receiptSortField = signal<ReceiptSortField>('id');
  receiptSortDescending = signal(true);

  // Receipts filter controls
  receiptStartDate = new FormControl<Date | null>(null);
  receiptEndDate = new FormControl<Date | null>(null);

  ngOnInit(): void {
    this.restoreState();
    this.setupBookingSearchDebounce();
    this.setupBookingStatusListener();

    if (this.activeView.value === 'bookings') {
      this.fetchBookings();
    } else {
      this.fetchReceipts();
    }
  }

  private setupBookingSearchDebounce(): void {
    this.bookingSearch.valueChanges
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(() => {
        this.bookingPage.set(0);
        this.saveState();
        this.fetchBookings();
      });
  }

  private setupBookingStatusListener(): void {
    this.bookingStatus.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        this.bookingPage.set(0);
        this.saveState();
        this.fetchBookings();
      });
  }

  onBookingSearchDebounced(): void {
    // Subscribed via setupBookingSearchDebounce valueChanges.
  }

  onViewToggle(): void {
    this.saveState();
    if (this.activeView.value === 'bookings') {
      this.fetchBookings();
    } else {
      this.fetchReceipts();
    }
  }

  fetchBookings(): void {
    this.bookingsLoading.set(true);
    this.bookingsError.set(null);
    this.bookingApi
      .getAll({
        status: this.bookingStatus.value || undefined,
        guestQuery: this.bookingSearch.value || undefined,
        pageNumber: this.bookingPage() + 1,
        pageSize: this.bookingPageSize(),
        sortBy: this.bookingSortField(),
        sortDescending: this.bookingSortDescending(),
      })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.bookingsLoading.set(false)),
      )
      .subscribe({
        next: (res) => {
          this.bookings.set(res.data);
          this.bookingsTotal.set(res.totalCount);
          const maxPage = Math.max(0, Math.ceil(res.totalCount / this.bookingPageSize()) - 1);
          if (this.bookingPage() > maxPage) {
            this.bookingPage.set(maxPage);
            this.saveState();
          }
        },
        error: (err) => this.bookingsError.set(this.extractErrorMessage(err)),
      });
  }

  fetchReceipts(): void {
    this.receiptsLoading.set(true);
    this.receiptsError.set(null);
    const startStr = this.formatDate(this.receiptStartDate.value);
    const endStr = this.formatDate(this.receiptEndDate.value);
    this.billingApi
      .getReceipts({
        startDate: startStr,
        endDate: endStr,
        pageNumber: this.receiptPage() + 1,
        pageSize: this.receiptPageSize(),
        sortBy: this.receiptSortField(),
        sortDescending: this.receiptSortDescending(),
      })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.receiptsLoading.set(false)),
      )
      .subscribe({
        next: (res) => {
          this.receipts.set(res.data);
          this.receiptsTotal.set(res.totalCount);
          const maxPage = Math.max(0, Math.ceil(res.totalCount / this.receiptPageSize()) - 1);
          if (this.receiptPage() > maxPage) {
            this.receiptPage.set(maxPage);
            this.saveState();
          }
        },
        error: (err) => this.receiptsError.set(this.extractErrorMessage(err)),
      });
  }

  clearBookingFilters(): void {
    this.bookingSearch.setValue('');
    this.bookingStatus.setValue('');
    this.bookingPage.set(0);
    this.saveState();
    this.fetchBookings();
  }

  applyReceiptDateFilter(): void {
    this.receiptPage.set(0);
    this.saveState();
    this.fetchReceipts();
  }

  clearReceiptDateFilter(): void {
    this.receiptStartDate.setValue(null);
    this.receiptEndDate.setValue(null);
    this.receiptPage.set(0);
    this.saveState();
    this.fetchReceipts();
  }

  onBookingSort(event: Sort): void {
    if (!event.active || !event.direction) return;
    const field = event.active as BookingSortField;
    if (!['id', 'bookingStatus', 'bookedAt'].includes(field)) return;
    this.bookingSortField.set(field);
    this.bookingSortDescending.set(event.direction === 'desc');
    this.bookingPage.set(0);
    this.saveState();
    this.fetchBookings();
  }

  onReceiptSort(event: Sort): void {
    if (!event.active || !event.direction) return;
    const field = event.active as ReceiptSortField;
    if (!['id', 'amountPaid', 'paidAt'].includes(field)) return;
    this.receiptSortField.set(field);
    this.receiptSortDescending.set(event.direction === 'desc');
    this.receiptPage.set(0);
    this.saveState();
    this.fetchReceipts();
  }

  onBookingPage(event: any): void {
    this.bookingPage.set(event.pageIndex);
    this.bookingPageSize.set(event.pageSize);
    this.saveState();
    this.fetchBookings();
  }

  onReceiptPage(event: any): void {
    this.receiptPage.set(event.pageIndex);
    this.receiptPageSize.set(event.pageSize);
    this.saveState();
    this.fetchReceipts();
  }

  openBookingDetail(booking: Booking): void {
    this.dialog.open(BookingDetailDialogComponent, {
      data: booking,
      width: '500px',
    });
  }

  openReceiptDetail(receipt: Receipt): void {
    this.dialog.open(ReceiptDetailDialogComponent, {
      data: receipt,
      width: '400px',
    });
  }

  getRoomsSummary(booking: Booking): string {
    if (!booking.rooms || booking.rooms.length === 0) return '—';
    return booking.rooms
      .map((r) => r.roomNumber || `Room type #${r.roomTypeId}`)
      .join(', ');
  }

  private formatDate(date: Date | null): string | undefined {
    if (!date) return undefined;
    const d = new Date(date);
    const day = String(d.getDate()).padStart(2, '0');
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const year = d.getFullYear();
    return `${day}-${month}-${year}`;
  }

  private extractErrorMessage(err: any): string {
    if (typeof err === 'string') return err;
    if (err?.error?.message) return err.error.message;
    if (err?.message) return err.message;
    return 'An unexpected error occurred.';
  }

  private restoreState(): void {
    try {
      const stored = sessionStorage.getItem(this.STORAGE_KEY);
      if (!stored) return;
      const parsed = JSON.parse(stored);
      if (typeof parsed !== 'object' || parsed === null) return;

      if (parsed.activeView === 'bookings' || parsed.activeView === 'receipts')
        this.activeView.setValue(parsed.activeView);

      if (Number.isInteger(parsed.bookingPage) && parsed.bookingPage >= 0)
        this.bookingPage.set(parsed.bookingPage);
      if (Number.isInteger(parsed.bookingPageSize) && parsed.bookingPageSize > 0)
        this.bookingPageSize.set(parsed.bookingPageSize);
      if (parsed.bookingSortField === 'id' || parsed.bookingSortField === 'bookingStatus' || parsed.bookingSortField === 'bookedAt')
        this.bookingSortField.set(parsed.bookingSortField);
      if (typeof parsed.bookingSortDescending === 'boolean')
        this.bookingSortDescending.set(parsed.bookingSortDescending);
      if (typeof parsed.bookingSearch === 'string')
        this.bookingSearch.setValue(parsed.bookingSearch);
      if (typeof parsed.bookingStatus === 'string')
        this.bookingStatus.setValue(parsed.bookingStatus);

      if (Number.isInteger(parsed.receiptPage) && parsed.receiptPage >= 0)
        this.receiptPage.set(parsed.receiptPage);
      if (Number.isInteger(parsed.receiptPageSize) && parsed.receiptPageSize > 0)
        this.receiptPageSize.set(parsed.receiptPageSize);
      if (parsed.receiptSortField === 'id' || parsed.receiptSortField === 'amountPaid' || parsed.receiptSortField === 'paidAt')
        this.receiptSortField.set(parsed.receiptSortField);
      if (typeof parsed.receiptSortDescending === 'boolean')
        this.receiptSortDescending.set(parsed.receiptSortDescending);
      if (
        parsed.receiptStartDate === null ||
        (typeof parsed.receiptStartDate === 'string' && !isNaN(Date.parse(parsed.receiptStartDate)))
      )
        this.receiptStartDate.setValue(
          parsed.receiptStartDate ? new Date(parsed.receiptStartDate) : null,
        );
      if (
        parsed.receiptEndDate === null ||
        (typeof parsed.receiptEndDate === 'string' && !isNaN(Date.parse(parsed.receiptEndDate)))
      )
        this.receiptEndDate.setValue(
          parsed.receiptEndDate ? new Date(parsed.receiptEndDate) : null,
        );
    } catch {
      /* fallback silently */
    }
  }

  private saveState(): void {
    sessionStorage.setItem(
      this.STORAGE_KEY,
      JSON.stringify({
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
      }),
    );
  }
}
