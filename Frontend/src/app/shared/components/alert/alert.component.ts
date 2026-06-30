import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-alert',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule],
  template: `
    <div [class]="'alert-box ' + type" role="alert" aria-live="polite">
      <span class="alert-message">{{ message }}</span>
      <button mat-icon-button type="button" aria-label="Close alert" (click)="closed.emit()">
        <mat-icon>close</mat-icon>
      </button>
    </div>
  `,
  styles: [`
    .alert-box {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 12px 16px;
      margin: 16px 0;
      border-radius: 4px;
      font-size: 14px;
    }
    .alert-box.success {
      background-color: #e8f5e9;
      color: #2e7d32;
      border: 1px solid #c8e6c9;
    }
    .alert-box.error {
      background-color: #ffebee;
      color: #c62828;
      border: 1px solid #ffcdd2;
    }
    .alert-message {
      flex-grow: 1;
    }
    button {
      color: inherit;
    }
  `]
})
export class AlertComponent {
  @Input() type: 'success' | 'error' = 'success';
  @Input() message = '';
  @Output() closed = new EventEmitter<void>();
}
