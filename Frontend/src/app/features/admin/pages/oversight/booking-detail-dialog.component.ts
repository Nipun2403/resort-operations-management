import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { MatChipsModule } from '@angular/material/chips';
import { MatListModule } from '@angular/material/list';
import { Booking } from '../../models/booking.model';

@Component({
  selector: 'app-booking-detail-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatDividerModule,
    MatChipsModule,
    MatListModule,
  ],
  templateUrl: './booking-detail-dialog.component.html',
  styles: [`
    .booking-dialog-container {
      display: flex;
      flex-direction: column;
      gap: 16px;
      padding: 8px 0;
    }
    .info-section {
      h3 {
        margin-top: 0;
        margin-bottom: 8px;
        color: #1976d2;
        font-size: 16px;
        font-weight: 500;
      }
      p {
        margin: 4px 0;
      }
    }
    .status-chip {
      display: inline-block;
      padding: 4px 12px;
      border-radius: 16px;
      font-size: 12px;
      font-weight: 500;
      &.Booked { background-color: #e3f2fd; color: #1565c0; }
      &.CheckedIn { background-color: #e8f5e9; color: #2e7d32; }
      &.CheckedOut { background-color: #eceff1; color: #37474f; }
      &.Cancelled { background-color: #ffebee; color: #c62828; }
    }
  `]
})
export class BookingDetailDialogComponent {
  constructor(@Inject(MAT_DIALOG_DATA) public data: Booking) {}
}
