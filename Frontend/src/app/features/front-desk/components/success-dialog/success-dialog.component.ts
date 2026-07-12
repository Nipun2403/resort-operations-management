import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { HttpErrorResponse } from '@angular/common/http';
import { finalize } from 'rxjs/operators';
import { BookingApiService } from '../../../../features/user/services/booking-api.service';

@Component({
  selector: 'app-success-dialog',
  standalone: true,
  imports: [MatDialogModule, MatButtonModule, CommonModule, MatProgressSpinnerModule],
  template: `
    <h2 mat-dialog-title>Booking Created</h2>
    <mat-dialog-content>
      <p>Booking #{{ data.bookingId }} for {{ data.guestName }} has been created.</p>
      @if (data.origin === 'WalkIn' && data.guestEmail) {
        <p>
          <strong>Walk-in guest registered.</strong> Login email: <strong>{{ data.guestEmail }}</strong>, temporary password: <strong>{{ data.guestEmail.split('@')[0] }}</strong>.
          Ask the guest to change their password on first login.
        </p>
      }
      <p>Would you like to check in now?</p>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="close()">Close</button>
      <button mat-raised-button color="primary" (click)="checkInNow()" [disabled]="checkingIn()">
        @if (checkingIn()) { <mat-spinner diameter="20"></mat-spinner> }
        Check-In Now
      </button>
    </mat-dialog-actions>
  `
})
export class SuccessDialogComponent {
  data: { bookingId: number; guestName: string; guestEmail?: string; origin?: string } = inject(MAT_DIALOG_DATA);
  private dialogRef = inject(MatDialogRef<SuccessDialogComponent>);
  private bookingApi = inject(BookingApiService);
  private snackBar = inject(MatSnackBar);
  checkingIn = signal(false);

  checkInNow(): void {
    this.checkingIn.set(true);
    this.bookingApi.checkIn(this.data.bookingId).pipe(
      finalize(() => this.checkingIn.set(false))
    ).subscribe({
      next: (updated) => {
        this.snackBar.open(`Checked in. Room: ${updated.rooms?.[0]?.roomNumber || 'assigned'}`, 'Close', { duration: 3000 });
        this.dialogRef.close(true);
      },
      error: (err: HttpErrorResponse) => {
        const message = this.extractCheckInError(err);
        this.snackBar.open(message, 'Close', { duration: 5000 });
      }
    });
  }

  private extractCheckInError(err: HttpErrorResponse): string {
    // If the response body is a plain string, use it directly.
    if (typeof err.error === 'string') {
      return err.error;
    }
    // If it's an object with a message property (e.g., JSON error)
    if (err.error?.message) {
      return err.error.message;
    }
    // Fallback to the HTTP status text or generic message
    return `Check-in failed (${err.status})`;
  }

  close(): void {
    this.dialogRef.close(false);
  }
}
