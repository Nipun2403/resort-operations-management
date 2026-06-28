import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-folio-detail-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule],
  template: `
    <h2 mat-dialog-title>Folio for Booking #{{ data.bookingId }}</h2>
    <mat-dialog-content>
      <p><strong>Guest:</strong> {{ data.guestName }}</p>
      <p><strong>Nights Stayed:</strong> {{ data.nightsStayed }}</p>
      <p><strong>Room Total:</strong> {{ data.roomTotal | currency }} ({{ data.roomBasePrice | currency }}/night)</p>
      <p><strong>Food Total:</strong> {{ data.foodTotal | currency }}</p>
      @if (data.foodItems && data.foodItems.length > 0) {
        <ul>
          @for (item of data.foodItems; track item) {
            <li>{{ item }}</li>
          }
        </ul>
      }
      <p><strong>Amenity Total:</strong> {{ data.amenityTotal | currency }}</p>
      @if (data.amenityItems && data.amenityItems.length > 0) {
        <ul>
          @for (item of data.amenityItems; track item) {
            <li>{{ item }}</li>
          }
        </ul>
      }
      <p><strong>Total Bill:</strong> {{ data.totalBill | currency }}</p>
      <p><strong>Payment Status:</strong> {{ data.paymentStatus }}</p>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close>Close</button>
    </mat-dialog-actions>
  `
})
export class FolioDetailDialogComponent {
  constructor(@Inject(MAT_DIALOG_DATA) public data: any) {}
}
