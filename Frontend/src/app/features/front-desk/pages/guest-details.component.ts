import { Component, inject, signal, computed, OnInit, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterModule, ActivatedRoute } from '@angular/router';
import { MatTabsModule } from '@angular/material/tabs';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs/operators';

import { BookingApiService } from '../../user/services/booking-api.service';
import { BillingApiService } from '../../user/services/billing-api.service';
import { GuestApiService } from '../services/guest-api.service';
import { GuestProfile } from '../models/guest-profile.model';
import { Booking } from '../../admin/models/booking.model';
import { AlertComponent } from '../../auth/components/alert.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { ExtendStayDialogComponent } from '../components/extend-stay-dialog/extend-stay-dialog.component';
import { CheckoutDialogComponent } from '../components/booking-action-modal/checkout-dialog/checkout-dialog.component';
import { FoodOrderPanelComponent } from '../components/booking-action-modal/food-order-panel/food-order-panel.component';
import { HousekeepingRequestPanelComponent } from '../components/booking-action-modal/housekeeping-request-panel/housekeeping-request-panel.component';
import { MaintenanceRequestPanelComponent } from '../components/booking-action-modal/maintenance-request-panel/maintenance-request-panel.component';
import { GuestBillingComponent } from '../components/guest-billing/guest-billing.component';

@Component({
  selector: 'app-guest-details',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    MatTabsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatDividerModule,
    MatTableModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatDialogModule,
    AlertComponent,
    FoodOrderPanelComponent,
    HousekeepingRequestPanelComponent,
    MaintenanceRequestPanelComponent,
    GuestBillingComponent,
  ],
  templateUrl: './guest-details.component.html',
  styleUrls: ['./guest-details.component.scss'],
})
export class GuestDetailsComponent implements OnInit {
  email = signal('');
  bookings = signal<Booking[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);
  guestProfile = signal<GuestProfile | null>(null);

  activeBooking = computed(() => {
    return (
      this.bookings().find(b => b.bookingStatus === 'CheckedIn') ||
      this.bookings()[0] ||
      null
    );
  });
  activeBookingId = computed(() => this.activeBooking()?.id ?? 0);

  private readonly route = inject(ActivatedRoute);
  private readonly bookingApi = inject(BookingApiService);
  private readonly billingApi = inject(BillingApiService);
  private readonly guestApi = inject(GuestApiService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);
  private readonly destroyRef = inject(DestroyRef);

  ngOnInit(): void {
    const encodedEmail = this.route.snapshot.paramMap.get('email') || '';
    const decodedEmail = decodeURIComponent(encodedEmail);
    this.email.set(decodedEmail);
    this.fetchBookings();
    this.guestApi.search(decodedEmail).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: res => {
        if (res.data && res.data.length > 0) {
          this.guestProfile.set(res.data[0]);
        }
      },
      error: (err: any) => console.error('Failed to load guest profile', err)
    });
  }

  fetchBookings(): void {
    this.loading.set(true);
    this.bookingApi.getAll({ guestQuery: this.email(), pageSize: 200 }).pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.loading.set(false))
    ).subscribe({
      next: res => this.bookings.set(res.data),
      error: (err: any) => this.error.set(this.extractErrorMessage(err))
    });
  }

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
      data: { title: 'Confirm Check-In', message: `Check in guest: ${booking.guestName}?` }
    });
    confirmRef.afterClosed().pipe(takeUntilDestroyed(this.destroyRef)).subscribe(confirmed => {
      if (!confirmed) return;
      this.bookingApi.checkIn(booking.id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
        next: (updated) => {
          this.snackBar.open(`Checked in. Room: ${updated.rooms?.[0]?.roomNumber || 'assigned'}`, 'Close', { duration: 3000 });
          this.fetchBookings();
        },
        error: (err: any) => this.snackBar.open(this.extractErrorMessage(err), 'Close', { duration: 5000 })
      });
    });
  }

  cancelBooking(booking: Booking): void {
    this.bookingApi.cancel(booking.id).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: () => {
        this.snackBar.open('Booking cancelled.', 'Close', { duration: 3000 });
        this.fetchBookings();
      },
      error: (err: any) => this.snackBar.open(this.extractErrorMessage(err), 'Close', { duration: 5000 })
    });
  }

  extendStay(booking: Booking): void {
    const extendRef = this.dialog.open(ExtendStayDialogComponent, {
      data: { bookingId: booking.id, currentCheckOut: booking.checkOutDate },
      width: '400px',
      maxWidth: '90vw',
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
        this.snackBar.open('Check-out successful.', 'Close', { duration: 3000 });
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
}
