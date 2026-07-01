import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { AuditLogEntry } from '../../models/audit-log-entry.model';

@Component({
  selector: 'app-audit-log-detail-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatDividerModule,
  ],
  templateUrl: './audit-log-detail-dialog.component.html',
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

      .detail-section {
        margin-bottom: 24px;
        
        h3 {
          font-family: var(--font-body, "Manrope"), sans-serif;
          font-size: 11px;
          font-weight: 600;
          letter-spacing: 0.15em;
          text-transform: uppercase;
          color: rgba(228, 194, 133, 0.6);
          margin-bottom: 12px;
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
    }

    .aetheris-divider {
      border-top-color: rgba(228, 194, 133, 0.1) !important;
      margin: 16px 0;
    }

    .values-row {
      display: flex;
      gap: 24px;
      margin-top: 16px;
    }

    .values-column {
      flex: 1;
      background: rgba(31, 32, 29, 0.4);
      padding: 16px;
      border-radius: 0px;
      border: 1px solid rgba(228, 194, 133, 0.15);

      h3 {
        margin-top: 0;
        margin-bottom: 16px;
        font-family: var(--font-body, "Manrope"), sans-serif;
        font-size: 10px;
        font-weight: 600;
        letter-spacing: 0.15em;
        text-transform: uppercase;
        color: var(--color-secondary, #e4c285);
      }

      .empty-val {
        font-size: 12px;
        color: rgba(228, 194, 133, 0.4);
        font-style: italic;
        margin: 0;
      }

      .value-list {
        display: flex;
        flex-direction: column;
        gap: 8px;
        max-height: 180px;
        overflow-y: auto;

        .value-item {
          display: flex;
          justify-content: space-between;
          gap: 12px;
          font-size: 12px;
          font-family: monospace;
          border-bottom: 1px dashed rgba(228, 194, 133, 0.1);
          padding-bottom: 6px;

          .key {
            font-weight: 500;
            color: var(--color-on-surface-variant, #c4c7c7);
            word-break: break-all;
          }

          .val {
            color: var(--color-on-surface, #e4e2dd);
            text-align: right;
            word-break: break-all;
          }
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

    @media (max-width: 600px) {
      .values-row {
        flex-direction: column;
        gap: 16px;
      }
    }
  `]
})
export class AuditLogDetailDialogComponent {
  constructor(@Inject(MAT_DIALOG_DATA) public data: AuditLogEntry) {}

  getKeys(obj: Record<string, any>): string[] {
    return Object.keys(obj);
  }

  formatValue(value: any): string {
    if (value === null || value === undefined) return 'null';
    if (typeof value === 'boolean') return value ? 'Yes' : 'No';
    if (typeof value === 'object') return JSON.stringify(value);
    return String(value);
  }
}
