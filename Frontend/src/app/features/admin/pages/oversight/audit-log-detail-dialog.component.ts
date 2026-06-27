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
    .detail-section {
      margin-bottom: 16px;
      p { margin: 6px 0; }
    }
    .values-row {
      display: flex;
      gap: 24px;
      margin-top: 16px;
    }
    .values-column {
      flex: 1;
      background: #fcfcfc;
      padding: 12px;
      border-radius: 8px;
      border: 1px solid #eee;
      h3 { margin-top: 0; margin-bottom: 12px; font-size: 14px; font-weight: 600; color: #555; }
      .value-list {
        display: flex;
        flex-direction: column;
        gap: 6px;
        .value-item {
          display: flex;
          justify-content: space-between;
          gap: 8px;
          font-size: 13px;
          border-bottom: 1px dashed #eee;
          padding-bottom: 4px;
          .key { font-weight: 500; color: #666; word-break: break-all; }
          .val { color: #333; text-align: right; word-break: break-all; }
        }
      }
    }
    @media (max-width: 600px) {
      .values-row {
        flex-direction: column;
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
