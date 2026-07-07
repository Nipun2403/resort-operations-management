import { Component, input, inject, signal, computed, OnInit, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { forkJoin, of } from 'rxjs';
import { map, catchError, finalize } from 'rxjs/operators';

import { BillingApiService } from '../../../../features/user/services/billing-api.service';
import { Booking } from '../../../../features/admin/models/booking.model';
import { BillingFolio } from '../../../../features/user/models/billing-folio.model';
import { AlertComponent } from '../../../../features/auth/components/alert.component';
import { FolioDetailDialogComponent } from './folio-detail-dialog.component';

interface BillingRecord extends BillingFolio {
  checkOutDate: Date | null;
}

@Component({
  selector: 'app-guest-billing',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatDividerModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatExpansionModule,
    MatDialogModule,
    AlertComponent,
  ],
  templateUrl: './guest-billing.component.html',
  styleUrls: ['./guest-billing.component.scss'],
})
export class GuestBillingComponent implements OnInit {
  bookings = input.required<Booking[]>();

  billingRecords = signal<BillingRecord[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);

  latestBilling = computed(() => this.billingRecords()[0] || null);
  oldBilling = computed(() => this.billingRecords().slice(1));

  private readonly billingApi = inject(BillingApiService);
  private readonly dialog = inject(MatDialog);
  private readonly destroyRef = inject(DestroyRef);

  ngOnInit(): void {
    this.fetchAllBilling();
  }

  private parseDate(dateStr: string): Date | null {
    if (!dateStr) return null;
    const parts = dateStr.split('-');
    if (parts.length === 3) {
      const date = new Date(+parts[2], +parts[1] - 1, +parts[0]);
      if (!isNaN(date.getTime())) return date;
    }
    const date = new Date(dateStr);
    return !isNaN(date.getTime()) ? date : null;
  }

  private fetchAllBilling(): void {
    const bookings = this.bookings();
    if (bookings.length === 0) return;

    this.loading.set(true);
    this.error.set(null);

    const requests = bookings.map(b =>
      this.billingApi.getByBookingId(b.id).pipe(
        map(data => ({
          ...data,
          bookingId: b.id,
          checkOutDate: this.parseDate(b.checkOutDate)
        } as unknown as BillingRecord)),
        catchError(() => of(null))
      )
    );

    forkJoin(requests)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.loading.set(false))
      )
      .subscribe({
        next: (results) => {
          const valid = results.filter((r): r is BillingRecord => r !== null);
          valid.sort((a, b) => (b.checkOutDate?.getTime() ?? 0) - (a.checkOutDate?.getTime() ?? 0));
          this.billingRecords.set(valid);
        },
        error: (err) => {
          this.error.set(err?.message || 'Failed to fetch billing records.');
        }
      });
  }

  openFolioDetail(record: BillingRecord): void {
    this.dialog.open(FolioDetailDialogComponent, {
      data: record,
      width: '95vw',
      maxWidth: '600px',
    });
  }
}
