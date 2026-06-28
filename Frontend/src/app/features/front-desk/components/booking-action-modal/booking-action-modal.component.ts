import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DestroyRef } from '@angular/core';
import { finalize } from 'rxjs';

import { BookingApiService } from '../../../user/services/booking-api.service';
import { Booking } from '../../../admin/models/booking.model';
import { ConfirmDialogComponent } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';
import { AlertComponent } from '../../../auth/components/alert.component';
import { ExtendStayDialogComponent } from '../extend-stay-dialog/extend-stay-dialog.component';

import { MatTabsModule } from '@angular/material/tabs';
import { RoomServiceTabComponent } from './room-service-tab/room-service-tab.component';
import { BillingTabComponent } from './billing-tab/billing-tab.component';
import { CheckoutDialogComponent } from './checkout-dialog/checkout-dialog.component';

@Component({
  selector: 'app-booking-action-modal',
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
    AlertComponent,
    MatTabsModule,
    RoomServiceTabComponent,
    BillingTabComponent,
  ],
  templateUrl: './booking-action-modal.component.html',
  styleUrls: ['./booking-action-modal.component.scss'],
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
        title: 'Confirm Check‑In',
        message: `Check in guest: ${this.booking().guestName}?`,
      },
    });
    confirmRef
      .afterClosed()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(confirmed => {
        if (!confirmed) return;
        this.loading.set(true);
        this.error.set(null);
        this.bookingApi
          .checkIn(this.booking().id)
          .pipe(
            takeUntilDestroyed(this.destroyRef),
            finalize(() => this.loading.set(false))
          )
          .subscribe({
            next: (updatedBooking: Booking) => {
              const roomNumber = updatedBooking.rooms?.[0]?.roomNumber || 'assigned';
              this.snackBar.open(`Checked in successfully. Room: ${roomNumber}`, 'Close', { duration: 3000 });
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
        title: 'Cancel Booking',
        message: `Are you sure you want to cancel booking #${this.booking().id} for ${this.booking().guestName}? This cannot be undone.`,
      },
    });
    confirmRef
      .afterClosed()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(confirmed => {
        if (!confirmed) return;
        this.loading.set(true);
        this.error.set(null);
        this.bookingApi
          .cancel(this.booking().id)
          .pipe(
            takeUntilDestroyed(this.destroyRef),
            finalize(() => this.loading.set(false))
          )
          .subscribe({
            next: () => {
              this.snackBar.open('Booking cancelled.', 'Close', { duration: 3000 });
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
      .subscribe(result => {
        if (result === true) {
          this.snackBar.open('Stay extended successfully.', 'Close', { duration: 3000 });
          this.dialogRef.close(true);
        }
      });
  }

  // ── Check‑Out ───────────────────────────────────
  checkOut(): void {
    if (this.loading()) return;
    const checkoutRef = this.dialog.open(CheckoutDialogComponent, {
      data: { bookingId: this.booking().id },
      width: '95vw',
      maxWidth: '600px',
      disableClose: true,
    });
    checkoutRef
      .afterClosed()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(result => {
        if (result === true) {
          this.snackBar.open('Check‑out successful.', 'Close', { duration: 3000 });
          this.dialogRef.close(true);
        }
      });
  }

  private extractErrorMessage(err: any): string {
    if (typeof err === 'string') return err;
    if (err?.error?.message) return err.error.message;
    if (err?.message) return err.message;
    return 'An unexpected error occurred.';
  }
}
