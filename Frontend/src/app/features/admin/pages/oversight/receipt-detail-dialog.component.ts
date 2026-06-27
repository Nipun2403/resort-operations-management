import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { Receipt } from '../../models/receipt.model';

@Component({
  selector: 'app-receipt-detail-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatDividerModule,
  ],
  templateUrl: './receipt-detail-dialog.component.html',
  styles: [`
    .receipt-dialog-container {
      display: flex;
      flex-direction: column;
      gap: 12px;
      padding: 8px 0;
    }
    .info-section {
      p {
        margin: 8px 0;
        font-size: 14px;
        color: #333;
      }
    }
  `]
})
export class ReceiptDetailDialogComponent {
  constructor(@Inject(MAT_DIALOG_DATA) public data: Receipt) {}
}
