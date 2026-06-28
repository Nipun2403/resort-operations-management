import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { MatListModule } from '@angular/material/list';
import { Booking } from '../../models/booking.model';

@Component({
  selector: 'app-booking-detail-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule, MatListModule, MatDividerModule],
  templateUrl: './booking-detail-dialog.component.html',
  styles: [`
    .detail-section {
      margin-bottom: 16px;
    }
    .detail-row {
      display: flex;
      margin-bottom: 8px;
      font-size: 0.95rem;
    }
    .detail-label {
      font-weight: 600;
      width: 140px;
      color: rgba(0, 0, 0, 0.6);
    }
    .detail-value {
      color: rgba(0, 0, 0, 0.87);
    }
    ul {
      margin: 4px 0 0 0;
      padding-left: 20px;
    }
  `]
})
export class BookingDetailDialogComponent {
  readonly booking: Booking = inject(MAT_DIALOG_DATA);
}
