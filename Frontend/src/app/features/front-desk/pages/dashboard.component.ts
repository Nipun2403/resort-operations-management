import { Component, inject, signal, OnInit, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl } from '@angular/forms';
import { Router } from '@angular/router';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize, forkJoin, of, debounceTime, distinctUntilChanged } from 'rxjs';
import { map, catchError } from 'rxjs/operators';

import { BookingApiService } from '../../user/services/booking-api.service';
import { HousekeepingApiService } from '../../user/services/housekeeping-api.service';
import { MaintenanceApiService } from '../../user/services/maintenance-api.service';
import { OrderApiService } from '../../user/services/order-api.service';
import { AlertComponent } from '../../auth/components/alert.component';
import { ActiveTicketsDialogComponent } from '../components/active-tickets-dialog/active-tickets-dialog.component';
import { InternalTicketPanelComponent } from '../components/booking-action-modal/internal-ticket-panel/internal-ticket-panel.component';
import { Booking } from '../../admin/models/booking.model';

interface SearchResult {
  guestName: string;
  guestEmail: string;
  currentStatus: string;
  bookings: Booking[];
}

@Component({
  selector: 'app-front-desk-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatProgressSpinnerModule,
    MatPaginatorModule,
    AlertComponent,
  ],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss'],
})
export class DashboardComponent implements OnInit {
  // Summary card signals (keep intact)
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

  // Search
  searchControl = new FormControl('', { nonNullable: true });
  searchResults = signal<SearchResult[]>([]);
  searchLoading = signal(false);
  searchError = signal<string | null>(null);

  // Movement table (today's arrivals/departures)
  movementData = signal<Booking[]>([]);
  movementTotal = signal(0);
  movementLoading = signal(false);
  movementError = signal<string | null>(null);
  movementPage = signal(0);
  movementPageSize = signal(10);
  movementActiveFilter = new FormControl<'arrivals' | 'departures'>('arrivals', {
    nonNullable: true,
  });

  private readonly bookingApi = inject(BookingApiService);
  private readonly housekeepingApi = inject(HousekeepingApiService);
  private readonly maintenanceApi = inject(MaintenanceApiService);
  private readonly orderApi = inject(OrderApiService);
  private readonly dialog = inject(MatDialog);
  private readonly destroyRef = inject(DestroyRef);
  private readonly router = inject(Router);

  ngOnInit(): void {
    this.loadSummary();
    this.fetchMovement();

    this.searchControl.valueChanges
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntilDestroyed(this.destroyRef))
      .subscribe((value) => this.onSearch(value.trim()));
  }

  private loadSummary(): void {
    this.loadingSummary.set(true);
    this.error.set(null);

    const arrivals$ = this.bookingApi
      .getAll({ movementStatus: 'incoming', pageNumber: 1, pageSize: 1 })
      .pipe(
        map((r) => r.totalCount),
        catchError(() => of(0)),
      );
    const departures$ = this.bookingApi
      .getAll({ movementStatus: 'outgoing', pageNumber: 1, pageSize: 1 })
      .pipe(
        map((r) => r.totalCount),
        catchError(() => of(0)),
      );

    const hkPending$ = this.housekeepingApi.getAll({ status: 'Pending', pageSize: 1 }).pipe(
      map((r) => r.totalCount),
      catchError(() => of(0)),
    );
    const hkInProgress$ = this.housekeepingApi.getAll({ status: 'InProgress', pageSize: 1 }).pipe(
      map((r) => r.totalCount),
      catchError(() => of(0)),
    );
    const mtPending$ = this.maintenanceApi.getAll({ status: 'Pending', pageSize: 1 }).pipe(
      map((r) => r.totalCount),
      catchError(() => of(0)),
    );
    const mtInProgress$ = this.maintenanceApi.getAll({ status: 'InProgress', pageSize: 1 }).pipe(
      map((r) => r.totalCount),
      catchError(() => of(0)),
    );
    const foodPending$ = this.orderApi.getAll({ status: 'Pending', pageSize: 1 }).pipe(
      map((r) => r.totalCount),
      catchError(() => of(0)),
    );
    const foodPreparing$ = this.orderApi.getAll({ status: 'Preparing', pageSize: 1 }).pipe(
      map((r) => r.totalCount),
      catchError(() => of(0)),
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
        finalize(() => this.loadingSummary.set(false)),
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

  private onSearch(query: string): void {
    if (!query) {
      this.searchResults.set([]);
      return;
    }
    this.searchLoading.set(true);
    this.bookingApi
      .getAll({ guestQuery: query, pageSize: 200 })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.searchLoading.set(false)),
      )
      .subscribe({
        next: (res) => {
          const grouped = this.groupByEmail(res.data);
          this.searchResults.set(grouped);
        },
        error: (err: any) => this.searchError.set(this.extractErrorMessage(err)),
      });
  }

  private groupByEmail(bookings: Booking[]): SearchResult[] {
    const map = new Map<string, Booking[]>();
    bookings.forEach((b) => {
      if (!b.guestEmail) return;
      const arr = map.get(b.guestEmail) || [];
      arr.push(b);
      map.set(b.guestEmail, arr);
    });
    return Array.from(map.entries()).map(([email, bookings]) => {
      const statuses = bookings.map((b) => b.bookingStatus);
      let currentStatus = 'Cancelled';
      if (statuses.includes('CheckedIn')) currentStatus = 'CheckedIn';
      else if (statuses.includes('Booked')) currentStatus = 'Booked';
      else if (statuses.includes('CheckedOut')) currentStatus = 'CheckedOut';
      return {
        guestName: bookings[0].guestName,
        guestEmail: email,
        currentStatus,
        bookings,
      };
    });
  }

  fetchMovement(): void {
    this.movementLoading.set(true);
    const params: any = {
      pageNumber: this.movementPage() + 1,
      pageSize: this.movementPageSize(),
    };
    if (this.movementActiveFilter.value === 'arrivals') {
      params.movementStatus = 'incoming';
      params.bookingStatus = 'Booked';
    } else {
      params.movementStatus = 'outgoing';
      params.bookingStatus = 'CheckedIn';
    }
    this.bookingApi
      .getAll(params)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.movementLoading.set(false)),
      )
      .subscribe({
        next: (res) => {
          this.movementData.set(res.data);
          this.movementTotal.set(res.totalCount);
        },
        error: (err: any) => this.movementError.set(this.extractErrorMessage(err)),
      });
  }

  onMovementToggle(): void {
    this.movementPage.set(0);
    this.fetchMovement();
  }

  onMovementPageChange(event: PageEvent): void {
    this.movementPage.set(event.pageIndex);
    this.movementPageSize.set(event.pageSize);
    this.fetchMovement();
  }

  getRoomNumbers(booking: Booking): string {
    const assignedRooms = booking.rooms
      ?.filter((r) => r.roomNumber)
      .map((r) => r.roomNumber)
      .join(', ');

    if (assignedRooms) {
      return assignedRooms;
    }

    return booking.bookingStatus === 'Cancelled'
      ? 'Booking Cancelled'
      : booking.rooms?.[0]?.roomAssignmentText || 'Room Not Assigned Yet';
  }

  navigateToGuest(email: string): void {
    const encoded = encodeURIComponent(email);
    this.router.navigate(['/operations/front-desk/guest', encoded]);
  }

  openInternalTicket(): void {
    this.dialog.open(InternalTicketPanelComponent, {
      width: '95vw',
      maxWidth: '500px',
    });
  }

  private extractErrorMessage(err: any): string {
    return err?.error?.message || err?.message || 'An unexpected error occurred.';
  }
}
