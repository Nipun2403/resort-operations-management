import { Component, input, inject, signal, OnInit, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonModule } from '@angular/material/button';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs/operators';

import { BillingApiService } from '../../../../user/services/billing-api.service';
import { AlertComponent } from '../../../../auth/components/alert.component';
import { PaymentFormComponent } from '../payment-form/payment-form.component';

@Component({
  selector: 'app-billing-tab',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatDividerModule,
    MatProgressSpinnerModule,
    MatButtonModule,
    AlertComponent,
    PaymentFormComponent,
  ],
  templateUrl: './billing-tab.component.html',
  styleUrls: ['./billing-tab.component.scss'],
})
export class BillingTabComponent implements OnInit {
  bookingId = input.required<number>();

  billDetails = signal<any | null>(null);
  billLoading = signal(false);
  billError = signal<string | null>(null);
  showPayment = signal(false);

  private billingApi = inject(BillingApiService);
  private destroyRef = inject(DestroyRef);

  ngOnInit(): void {
    this.fetchBill();
  }

  fetchBill(): void {
    this.billLoading.set(true);
    this.billError.set(null);
    this.billingApi
      .getByBookingId(this.bookingId())
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.billLoading.set(false))
      )
      .subscribe({
        next: (data: any) => this.billDetails.set(data),
        error: (err: any) => this.billError.set(this.extractErrorMessage(err)),
      });
  }

  onPaymentComplete(): void {
    this.showPayment.set(false);
    this.fetchBill();
  }

  private extractErrorMessage(err: any): string {
    if (typeof err === 'string') return err;
    if (err?.error?.message) return err.error.message;
    if (err?.message) return err.message;
    return 'An unexpected error occurred.';
  }
}
