import { Component, inject, signal, OnInit, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs/operators';

import { BillingApiService } from '../../../../user/services/billing-api.service';
import { BookingApiService } from '../../../../user/services/booking-api.service';
import { AlertComponent } from '../../../../auth/components/alert.component';
import { PaymentFormComponent } from '../payment-form/payment-form.component';
import { Booking } from '../../../../admin/models/booking.model';

@Component({
  selector: 'app-checkin-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatDividerModule,
    MatProgressSpinnerModule,
    MatIconModule,
    AlertComponent,
    PaymentFormComponent,
  ],
  templateUrl: './checkin-dialog.component.html',
  styleUrls: ['./checkin-dialog.component.scss'],
})
export class CheckinDialogComponent implements OnInit {
  data: { bookingId: number } = inject(MAT_DIALOG_DATA);
  private dialogRef = inject(MatDialogRef<CheckinDialogComponent>);
  private billingApi = inject(BillingApiService);
  private bookingApi = inject(BookingApiService);
  private destroyRef = inject(DestroyRef);

  step = signal<'folio' | 'payment' | 'confirm' | 'error'>('folio');
  billDetails = signal<any | null>(null);
  checkinResult = signal<Booking | null>(null);
  loading = signal(false);
  error = signal<string | null>(null);
  bookingId = signal<number>(this.data.bookingId);

  get roomPaymentPending(): boolean {
    const b = this.billDetails();
    return !!b && b.paymentStatus === 'Pending';
  }

  get roomAndAmenityTotal(): number {
    const b = this.billDetails();
    return b ? (b.roomTotal + b.amenityTotal) : 0;
  }

  ngOnInit(): void {
    this.loadBill();
  }

  loadBill(): void {
    this.loading.set(true);
    this.error.set(null);
    this.billingApi
      .getByBookingId(this.bookingId())
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.loading.set(false))
      )
      .subscribe({
        next: data => this.billDetails.set(data),
        error: (err: any) => this.error.set(this.extractErrorMessage(err)),
      });
  }

  onPaymentComplete(): void {
    this.step.set('folio');
    this.loadBill();
  }

  processCheckIn(): void {
    this.loading.set(true);
    this.error.set(null);
    this.bookingApi
      .checkIn(this.bookingId())
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.loading.set(false))
      )
      .subscribe({
        next: (result) => {
          this.checkinResult.set(result);
          this.step.set('confirm');
        },
        error: (err: any) => {
          this.step.set('error');
          this.error.set(this.extractErrorMessage(err));
        },
      });
  }

  private extractErrorMessage(err: any): string {
    if (typeof err === 'string') return err;
    if (err?.error?.message) return err.error.message;
    if (typeof err?.error === 'string') return err.error;
    if (err?.message) return err.message;
    return 'An unexpected error occurred.';
  }
}
