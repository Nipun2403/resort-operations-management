import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
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
  data: { bookingId: number; guestName: string } = inject(MAT_DIALOG_DATA);
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
      error: (err) => this.snackBar.open('Check-in failed: ' + (err.error?.message || err.message), 'Close', { duration: 5000 })
    });
  }

  close(): void {
    this.dialogRef.close(false);
  }
}
