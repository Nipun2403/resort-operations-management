import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { MatCardModule } from '@angular/material/card';
import { CustomerRequest } from '../../models/customer-request.model';

@Component({
  selector: 'app-request-detail-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule, MatDividerModule, MatCardModule],
  template: `
    <h2 mat-dialog-title>Request Details</h2>
    <mat-dialog-content>
      <mat-card appearance="outlined" class="detail-card">
        <mat-card-content>
          <div class="detail-row">
            <span class="label">ID:</span>
            <span class="value">#{{ data.id }}</span>
          </div>
          <mat-divider></mat-divider>
          <div class="detail-row">
            <span class="label">Type:</span>
            <span class="value">{{ data.type }}</span>
          </div>
          <mat-divider></mat-divider>
          <div class="detail-row">
            <span class="label">Room:</span>
            <span class="value">{{ data.roomNumber }}</span>
          </div>
          <mat-divider></mat-divider>
          <div class="detail-row">
            <span class="label">Status:</span>
            <span class="value">
              <span class="status-badge" [class]="data.status.toLowerCase()">
                {{ data.status }}
              </span>
            </span>
          </div>
          <mat-divider></mat-divider>
          <div class="detail-row">
            <span class="label">Created At:</span>
            <span class="value">{{ data.createdAt | date:'medium' }}</span>
          </div>
          <mat-divider></mat-divider>
          <div class="detail-row description-row">
            <span class="label">Description:</span>
            <span class="value block-text">{{ data.description }}</span>
          </div>
        </mat-card-content>
      </mat-card>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close>Close</button>
    </mat-dialog-actions>
  `,
  styles: [`
    .detail-card {
      margin-top: 8px;
    }
    .detail-row {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 12px 0;
      font-size: 0.95rem;

      .label {
        font-weight: 500;
        color: rgba(0, 0, 0, 0.6);
      }
      .value {
        color: rgba(0, 0, 0, 0.87);
        font-weight: 500;
      }
    }
    .description-row {
      flex-direction: column;
      align-items: flex-start;
      gap: 6px;
      
      .block-text {
        width: 100%;
        background-color: #fafafa;
        padding: 8px 12px;
        border-radius: 4px;
        border: 1px solid #f0f0f0;
        white-space: pre-wrap;
        box-sizing: border-box;
      }
    }
    .status-badge {
      display: inline-block;
      padding: 4px 8px;
      border-radius: 4px;
      font-size: 0.85rem;
      font-weight: 500;

      &.pending {
        background-color: #fff3e0;
        color: #e65100;
      }
      &.inprogress {
        background-color: #e8eaf6;
        color: #1a237e;
      }
      &.completed {
        background-color: #e8f5e9;
        color: #1b5e20;
      }
    }
  `]
})
export class RequestDetailDialogComponent {
  readonly data = inject<CustomerRequest>(MAT_DIALOG_DATA);
}
