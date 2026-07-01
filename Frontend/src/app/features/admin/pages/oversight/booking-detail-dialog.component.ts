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

    .booking-dialog-container {
      display: flex;
      flex-direction: column;
      gap: 24px;
    }

    .info-section {
      h3 {
        margin-top: 0;
        margin-bottom: 12px;
        font-family: var(--font-body, "Manrope"), sans-serif;
        font-size: 11px;
        font-weight: 600;
        letter-spacing: 0.15em;
        text-transform: uppercase;
        color: rgba(228, 194, 133, 0.6);
      }

      p {
        margin: 6px 0;
        font-size: 13px;
        color: var(--color-on-surface-variant, #c4c7c7);

        strong {
          color: var(--color-on-surface, #e4e2dd);
          font-weight: 500;
        }
      }
    }

    mat-divider {
      border-top-color: rgba(228, 194, 133, 0.1) !important;
      margin: 8px 0;
    }

    .status-chip {
      display: inline-block;
      font-family: var(--font-body) !important;
      font-size: 9px !important;
      font-weight: 600 !important;
      letter-spacing: 0.1em;
      padding: 2px 8px !important;
      border-radius: 0 !important;
      background: rgba(228, 194, 133, 0.05) !important;
      border: 1px solid rgba(228, 194, 133, 0.3) !important;
      color: var(--color-secondary) !important;
      text-transform: uppercase;
      margin-left: 8px;
      
      &.Cancelled {
        border-color: rgba(255, 180, 171, 0.4) !important;
        color: var(--color-error) !important;
        background: rgba(255, 180, 171, 0.05) !important;
      }
      &.CheckedIn {
        border-color: rgba(80, 227, 194, 0.4) !important;
        color: #50e3c2 !important;
        background: rgba(80, 227, 194, 0.05) !important;
      }
      &.CheckedOut {
        border-color: rgba(196, 199, 199, 0.3) !important;
        color: var(--color-on-surface-variant) !important;
        background: rgba(196, 199, 199, 0.05) !important;
      }
    }

    ::ng-deep .mat-mdc-list-item {
      color: var(--color-on-surface) !important;
      
      .mat-mdc-list-item-title {
        color: var(--color-secondary) !important;
        font-family: var(--font-body) !important;
        font-size: 13px !important;
      }
      .mat-mdc-list-item-line {
        color: var(--color-on-surface-variant) !important;
        font-size: 11px !important;
      }
      .mat-icon {
        color: rgba(228, 194, 133, 0.6) !important;
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
export class BookingDetailDialogComponent {
  constructor(@Inject(MAT_DIALOG_DATA) public data: Booking) {}
}
