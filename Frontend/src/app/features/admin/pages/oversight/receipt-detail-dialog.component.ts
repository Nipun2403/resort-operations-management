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
    :host {
      display: block;
      background: transparent !important;
      color: var(--color-on-surface, #e4e2dd);
      font-family: var(--font-body, "Manrope"), sans-serif;
    }
    
    ::ng-deep .mat-mdc-dialog-container .mdc-dialog__surface {
      background: rgba(27, 28, 25, 0.98) !important;
      backdrop-filter: blur(20px) !important;
      -webkit-backdrop-filter: blur(20px) !important;
      border: 1px solid rgba(228, 194, 133, 0.2) !important;
      border-radius: 0px !important;
      box-shadow: 0 24px 48px rgba(0, 0, 0, 0.8) !important;
      padding: 0 !important;
    }

    h2[mat-dialog-title] {
      margin: 0;
      padding: 24px 32px;
      font-family: var(--font-display-lg, "Playfair Display"), serif;
      font-size: 20px;
      font-weight: 400;
      font-style: italic;
      color: var(--color-secondary, #e4c285) !important;
      border-bottom: 1px solid rgba(228, 194, 133, 0.15) !important;
    }

    mat-dialog-content {
      padding: 32px !important;
      background: transparent !important;
      color: var(--color-on-surface, #e4e2dd) !important;
      font-family: var(--font-body, "Manrope"), sans-serif !important;
    }

    .receipt-dialog-container {
      display: flex;
      flex-direction: column;
      gap: 16px;
    }

    .info-section {
      p {
        margin: 8px 0;
        font-size: 13px;
        color: var(--color-on-surface-variant, #c4c7c7);

        strong {
          color: var(--color-on-surface, #e4e2dd);
          font-weight: 500;
        }
      }
    }

    mat-dialog-actions {
      padding: 16px 32px 24px !important;
      border-top: 1px solid rgba(228, 194, 133, 0.15) !important;
      background: transparent !important;
    }

    .close-btn {
      background: transparent !important;
      border: 1px solid rgba(228, 194, 133, 0.3) !important;
      color: var(--color-on-surface, #e4e2dd) !important;
      font-family: var(--font-body, "Manrope"), sans-serif !important;
      font-size: 10px !important;
      font-weight: 500 !important;
      letter-spacing: 0.2em !important;
      text-transform: uppercase !important;
      padding: 8px 24px !important;
      border-radius: 0px !important;
      transition: all 0.3s ease !important;
      height: auto !important;
      line-height: normal !important;
      cursor: pointer !important;

      &:hover {
        background: rgba(228, 194, 133, 0.08) !important;
        border-color: var(--color-secondary, #e4c285) !important;
        color: var(--color-secondary, #e4c285) !important;
      }
    }
  `]
})
export class ReceiptDetailDialogComponent {
  constructor(@Inject(MAT_DIALOG_DATA) public data: Receipt) {}
}
