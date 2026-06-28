import { Component, inject, signal, OnInit, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize, forkJoin, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';

import { BookingApiService } from '../../user/services/booking-api.service';
import { HousekeepingApiService } from '../../user/services/housekeeping-api.service';
import { MaintenanceApiService } from '../../user/services/maintenance-api.service';
import { OrderApiService } from '../../user/services/order-api.service';
import { AlertComponent } from '../../auth/components/alert.component';
import { ActiveTicketsDialogComponent } from '../components/active-tickets-dialog/active-tickets-dialog.component';
import { MovementTableComponent } from '../components/movement-table/movement-table.component';
import { Booking } from '../../admin/models/booking.model';
import { BookingActionModalComponent } from '../components/booking-action-modal/booking-action-modal.component';

@Component({
  selector: 'app-front-desk-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatDialogModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    AlertComponent,
    MovementTableComponent,
  ],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss'],
})
export class PlaceholderDashboardComponent implements OnInit {
  refreshTable = signal(0);
  arrivalsCount = signal(0);
  departuresCount = signal(0);
  activeTickets = signal<{
    housekeeping: number;
    maintenance: number;
    foodOrders: number;
  }>({
    housekeeping: 0,
    maintenance: 0,
    foodOrders: 0,
  });

  loadingSummary = signal(false);
  error = signal<string | null>(null);

  private readonly bookingApi = inject(BookingApiService);
  private readonly housekeepingApi = inject(HousekeepingApiService);
  private readonly maintenanceApi = inject(MaintenanceApiService);
  private readonly orderApi = inject(OrderApiService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);
  private readonly destroyRef = inject(DestroyRef);

  ngOnInit(): void {
    this.loadSummary();
  }

  private loadSummary(): void {
    this.loadingSummary.set(true);
    this.error.set(null);

    const arrivals$ = this.bookingApi.getAll({ movementStatus: 'incoming', pageNumber: 1, pageSize: 1 }).pipe(
      map(r => r.totalCount),
      catchError(() => of(0))
    );
    const departures$ = this.bookingApi.getAll({ movementStatus: 'outgoing', pageNumber: 1, pageSize: 1 }).pipe(
      map(r => r.totalCount),
      catchError(() => of(0))
    );

    const hkPending$ = this.housekeepingApi.getAll({ status: 'Pending', pageSize: 1 }).pipe(
      map(r => r.totalCount),
      catchError(() => of(0))
    );
    const hkInProgress$ = this.housekeepingApi.getAll({ status: 'InProgress', pageSize: 1 }).pipe(
      map(r => r.totalCount),
      catchError(() => of(0))
    );
    const mtPending$ = this.maintenanceApi.getAll({ status: 'Pending', pageSize: 1 }).pipe(
      map(r => r.totalCount),
      catchError(() => of(0))
    );
    const mtInProgress$ = this.maintenanceApi.getAll({ status: 'InProgress', pageSize: 1 }).pipe(
      map(r => r.totalCount),
      catchError(() => of(0))
    );
    const foodPending$ = this.orderApi.getAll({ status: 'Pending', pageSize: 1 }).pipe(
      map(r => r.totalCount),
      catchError(() => of(0))
    );
    const foodPreparing$ = this.orderApi.getAll({ status: 'Preparing', pageSize: 1 }).pipe(
      map(r => r.totalCount),
      catchError(() => of(0))
    );

    forkJoin({
      arrivals: arrivals$,
      departures: departures$,
      hk: forkJoin([hkPending$, hkInProgress$]).pipe(map(([p, ip]) => p + ip)),
      mt: forkJoin([mtPending$, mtInProgress$]).pipe(map(([p, ip]) => p + ip)),
      food: forkJoin([foodPending$, foodPreparing$]).pipe(map(([p, ip]) => p + ip)),
    })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.loadingSummary.set(false))
      )
      .subscribe({
        next: ({ arrivals, departures, hk, mt, food }) => {
          this.arrivalsCount.set(arrivals);
          this.departuresCount.set(departures);
          this.activeTickets.set({ housekeeping: hk, maintenance: mt, foodOrders: food });
        },
        error: (err: any) => this.error.set(this.extractErrorMessage(err)),
      });
  }

  openActiveTickets(): void {
    this.dialog.open(ActiveTicketsDialogComponent, {
      data: {
        housekeepingCount: this.activeTickets().housekeeping,
        maintenanceCount: this.activeTickets().maintenance,
        foodOrdersCount: this.activeTickets().foodOrders,
      },
      width: '90vw',
      maxWidth: '800px',
    });
  }

  openBookingModal(booking: Booking): void {
    const dialogRef = this.dialog.open(BookingActionModalComponent, {
      data: { booking },
      width: '95vw',
      maxWidth: '700px',
      panelClass: 'booking-action-modal',
    });
    dialogRef
      .afterClosed()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(result => {
        if (result === true) {
          this.refreshTable.update(n => n + 1);
          this.loadSummary();
        }
      });
  }

  private extractErrorMessage(err: any): string {
    return err?.error?.message || err?.message || 'An unexpected error occurred.';
  }
}
