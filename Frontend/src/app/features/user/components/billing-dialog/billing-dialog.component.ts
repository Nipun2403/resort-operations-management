import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { BillingApiService } from '../../services/billing-api.service';
import { BillingFolio } from '../../models/billing-folio.model';
import { AlertComponent } from '../../../../features/auth/components/alert.component';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'app-billing-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule, AlertComponent],
  templateUrl: './billing-dialog.component.html',
  styles: [`
    :host {
      display: block;
    }
    .billing-content {
      min-width: 320px;
    }
    .bill-row {
      display: flex;
      justify-content: space-between;
      margin-bottom: 8px;
      font-size: 0.95rem;
    }
    .bill-label {
      font-weight: 500;
      color: var(--color-on-surface-variant);
    }
    .bill-value {
      font-weight: 600;
      color: var(--color-on-surface);
    }
    .total-bill-row {
      border-top: 1px solid var(--color-outline-variant);
      padding-top: 8px;
      margin-top: 8px;
      font-weight: bold;
      font-size: 1.1rem;
      color: var(--color-on-surface);
    }
    h3 {
      margin: 16px 0 8px 0;
      font-size: 0.85rem;
      letter-spacing: 0.08em;
      text-transform: uppercase;
      color: var(--color-on-surface-variant);
      border-bottom: 1px solid var(--color-outline-variant);
      padding-bottom: 4px;
    }
    ul {
      margin: 0;
      padding-left: 20px;
      font-size: 0.9rem;
      color: var(--color-on-surface);
    }
  `]
})
export class BillingDialogComponent implements OnInit {
  readonly bookingId: number = inject(MAT_DIALOG_DATA);
  private readonly billingApi = inject(BillingApiService);

  folio = signal<BillingFolio | null>(null);
  loading = signal(false);
  error = signal<string | null>(null);
  downloading = signal(false);

  ngOnInit(): void {
    this.fetchBilling();
  }

  fetchBilling(): void {
    this.loading.set(true);
    this.error.set(null);

    this.billingApi.getByBookingId(this.bookingId)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (data) => this.folio.set(data),
        error: (err) => {
          const message = err.error?.message || err.message || 'Could not fetch billing details.';
          this.error.set(message);
        }
      });
  }

  downloadPdf(): void {
    this.downloading.set(true);
    this.billingApi.downloadFolioPdf(this.bookingId)
      .pipe(finalize(() => this.downloading.set(false)))
      .subscribe({
        next: (blob) => {
          const url = window.URL.createObjectURL(blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = `Aetheris_Folio_BK-${this.bookingId}.pdf`;
          a.click();
          window.URL.revokeObjectURL(url);
        },
        error: () => this.error.set('Failed to download PDF. Please try again.')
      });
  }
}
