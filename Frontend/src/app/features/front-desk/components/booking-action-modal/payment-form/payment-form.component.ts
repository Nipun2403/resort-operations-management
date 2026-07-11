import { Component, input, output, inject, signal, OnInit, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormGroup, FormControl, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs/operators';

import { BillingApiService } from '../../../../user/services/billing-api.service';
import { AlertComponent } from '../../../../auth/components/alert.component';

@Component({
  selector: 'app-payment-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    AlertComponent,
  ],
  templateUrl: './payment-form.component.html',
})
export class PaymentFormComponent implements OnInit {
  bookingId = input.required<number>();
  amountDue = input.required<number>();
  paymentType = input<string>('Booking');
  paymentComplete = output<void>();

  paymentForm = new FormGroup({
    amount: new FormControl<number>(0, {
      nonNullable: true,
      validators: [Validators.required, Validators.min(0.01)],
    }),
    paymentMethod: new FormControl<string>('', {
      nonNullable: true,
      validators: Validators.required,
    }),
    transactionId: new FormControl<string>('', {
      nonNullable: true,
      validators: Validators.required,
    }),
  });

  submitting = signal(false);
  error = signal<string | null>(null);

  private billingApi = inject(BillingApiService);
  private snackBar = inject(MatSnackBar);
  private destroyRef = inject(DestroyRef);

  ngOnInit(): void {
    this.paymentForm.patchValue({
      amount: this.amountDue(),
    });
  }

  submitPayment(): void {
    if (this.submitting() || this.paymentForm.invalid) return;
    this.submitting.set(true);
    this.error.set(null);

    const dto = { ...this.paymentForm.getRawValue(), paymentType: this.paymentType() };
    this.billingApi
      .pay(this.bookingId(), dto)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.submitting.set(false))
      )
      .subscribe({
        next: () => {
          this.snackBar.open('Payment processed.', 'Close', { duration: 3000 });
          this.paymentComplete.emit();
        },
        error: (err: any) => this.error.set(this.extractErrorMessage(err)),
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
