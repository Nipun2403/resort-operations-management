import { Component, inject, signal, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl, Validators } from '@angular/forms';
import { MatDialogModule, MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule, provideNativeDateAdapter } from '@angular/material/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs/operators';

import { BookingApiService } from '../../../user/services/booking-api.service';
import { AlertComponent } from '../../../auth/components/alert.component';

@Component({
  selector: 'app-extend-stay-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    AlertComponent,
  ],
  providers: [provideNativeDateAdapter()],
  templateUrl: './extend-stay-dialog.component.html',
  host: { 'style': 'min-width: 350px; display: block;' }
})
export class ExtendStayDialogComponent {
  data: { bookingId: number; currentCheckOut: string } = inject(MAT_DIALOG_DATA);
  private dialogRef = inject(MatDialogRef<ExtendStayDialogComponent>);
  private bookingApi = inject(BookingApiService);
  private destroyRef = inject(DestroyRef);

  minDate: Date = this.parseDate(this.data.currentCheckOut);
  newCheckOut = new FormControl<Date | null>(null, { validators: Validators.required });
  submitting = signal(false);
  error = signal<string | null>(null);

  private parseDate(dateStr: string): Date {
    const parts = dateStr.split('-');
    if (parts.length === 3) {
      return new Date(+parts[2], +parts[1] - 1, +parts[0]);
    }
    return new Date(dateStr);
  }

  submit(): void {
    if (this.submitting() || this.newCheckOut.invalid) return;
    this.submitting.set(true);
    this.error.set(null);

    const newDate = this.newCheckOut.value!;
    const dto = { checkOutDate: newDate.toISOString() };

    this.bookingApi
      .extendStay(this.data.bookingId, dto)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.submitting.set(false))
      )
      .subscribe({
        next: () => this.dialogRef.close(true),
        error: (err: any) => this.error.set(err.error?.message || err.message || 'Extend stay failed.'),
      });
  }
}
