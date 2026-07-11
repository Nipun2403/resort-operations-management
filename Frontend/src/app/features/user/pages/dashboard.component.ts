import { CommonModule } from '@angular/common';
import { Component, inject, signal, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DestroyRef } from '@angular/core';
import { finalize, forkJoin } from 'rxjs';
import { AuthApiService } from '../../../core/services/auth-api.service';
import { BookingApiService } from '../services/booking-api.service';
import { HousekeepingApiService } from '../services/housekeeping-api.service';
import { MaintenanceApiService } from '../services/maintenance-api.service';
import { CustomerBookingFacade } from '../facades/customer-booking.facade';
import { of, switchMap } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { Booking } from '../../admin/models/booking.model';
import { AlertComponent } from '../../auth/components/alert.component';
import { RequestServiceDialogComponent, RequestServiceDialogData, RequestServiceDialogResult } from '../components/request-service-dialog.component';
import { RoomTypeApiService } from '../services/room-type-api.service';
import { OrderApiService } from '../services/order-api.service';
import { CustomerRequest } from '../models/customer-request.model';

@Component({
  selector: 'app-customer-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatDialogModule,
    MatSnackBarModule,
    AlertComponent,
  ],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss'],
})
export class CustomerDashboardComponent implements OnInit {
  firstName = signal('');
  loading = signal(false);
  error = signal<string | null>(null);
  currentBooking = signal<Booking | null>(null);
  upcomingBooking = signal<Booking | null>(null);
  upcomingRoomTypes = signal<string[]>([]);
  pendingHousekeeping = signal<CustomerRequest[]>([]);
  pendingMaintenance = signal<CustomerRequest[]>([]);
  pendingFoodOrders = signal<any[]>([]);

  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);
  private readonly authApi = inject(AuthApiService);
  private readonly bookingApi = inject(BookingApiService);
  private readonly housekeepingApi = inject(HousekeepingApiService);
  private readonly maintenanceApi = inject(MaintenanceApiService);
  private readonly roomTypeApi = inject(RoomTypeApiService);
  private readonly orderApi = inject(OrderApiService);
  private readonly bookingFacade = inject(CustomerBookingFacade);
  private readonly destroyRef = inject(DestroyRef);
  private readonly router = inject(Router);

  ngOnInit(): void {
    const pending = sessionStorage.getItem('pendingBooking');
    if (pending) {
      try {
        const data = JSON.parse(pending);
        sessionStorage.removeItem('pendingBooking'); // clear immediately
        // Navigate to bookings with pre‑fill query params
        this.router.navigate(['/user/bookings'], {
          queryParams: {
            new: true,
            roomTypeId: data.roomTypeId,
            checkIn: data.checkIn,
            checkOut: data.checkOut,
            guests: data.guests
          }
        });
        return; // skip normal dashboard loading
      } catch { /* ignore */ }
    }
    this.loadDashboard();
  }

  loadDashboard(): void {
    this.loading.set(true);
    this.error.set(null);

    this.bookingFacade.getCurrentCustomerProfile().pipe(
      takeUntilDestroyed(this.destroyRef),
      switchMap((profile) => {
        this.firstName.set(profile.firstName);
        if (!profile.email) {
          return forkJoin({
            active: of(null),
            upcoming: of({ data: [] as Booking[] })
          });
        }
        return forkJoin({
          active: this.bookingFacade.getActiveBooking(),
          upcoming: this.bookingApi.getAll({
            guestQuery: profile.email,
            status: 'Booked',
            pageNumber: 1,
            pageSize: 1,
            sortBy: 'checkInDate',
            sortDescending: false
          })
        });
      }),
      finalize(() => this.loading.set(false))
    ).subscribe({
      next: ({ active, upcoming }) => {
        this.currentBooking.set(active);
        this.upcomingBooking.set(upcoming.data.length > 0 ? upcoming.data[0] : null);
        if (active) {
          this.loadRoomServiceStatus();
        } else {
          this.pendingHousekeeping.set([]);
          this.pendingMaintenance.set([]);
          this.pendingFoodOrders.set([]);
        }
        if (upcoming.data.length > 0) {
          this.loadUpcomingRoomTypes(upcoming.data[0]);
        } else {
          this.upcomingRoomTypes.set([]);
        }
      },
      error: (err: unknown) => {
        this.error.set(this.extractErrorMessage(err));
      }
    });
  }

  private loadUpcomingRoomTypes(booking: Booking): void {
    if (!booking.rooms || booking.rooms.length === 0) {
      this.upcomingRoomTypes.set([]);
      return;
    }
    const ids = [...new Set(booking.rooms.map(r => r.roomTypeId))];
    const requests = ids.map(id =>
      this.roomTypeApi.getById(id).pipe(
        catchError(() => of(null)),
        map(rt => rt?.name ?? `Room Type ${id}`)
      )
    );
    forkJoin(requests).pipe(takeUntilDestroyed(this.destroyRef)).subscribe(names => {
      this.upcomingRoomTypes.set(names);
    });
  }

  private loadRoomServiceStatus(): void {
    const booking = this.currentBooking();
    if (!booking) return;
    const roomIds = booking.rooms.map(r => r.roomId).filter(id => id != null) as number[];
    if (roomIds.length === 0) return;

    // Helper to fetch housekeeping/maintenance for a single status
    const fetchHousekeeping = (status: string) =>
      forkJoin(roomIds.map(roomId =>
        this.housekeepingApi.getAll({ roomId, status, pageSize: 20 }).pipe(
          map(res => res.data.map(hk => ({
            ...hk,
            type: 'Housekeeping' as const,
            roomNumber: hk.location ?? `Room ${hk.roomId}`,
            description: hk.description ?? ''
          }))),
          catchError(() => of([]))
        )
      )).pipe(map(results => results.flat()));

    const fetchMaintenance = (status: string) =>
      forkJoin(roomIds.map(roomId =>
        this.maintenanceApi.getAll({ roomId, status, pageSize: 20 }).pipe(
          map(res => res.data.map(mt => ({
            ...mt,
            type: 'Maintenance' as const,
            roomNumber: mt.location ?? `Room ${mt.roomId}`,
            description: mt.description ?? ''
          }))),
          catchError(() => of([]))
        )
      )).pipe(map(results => results.flat()));

    // Fetch both statuses for housekeeping and maintenance
    forkJoin({
      hkPending: fetchHousekeeping('Pending'),
      hkInProgress: fetchHousekeeping('InProgress'),
      mtPending: fetchMaintenance('Pending'),
      mtInProgress: fetchMaintenance('InProgress'),
      food: this.orderApi.getAll({ status: 'Pending', pageSize: 50 }).pipe(
        switchMap((res: any) => {
          return this.orderApi.getAll({ status: 'Preparing', pageSize: 50 }).pipe(
            map((res2: any) => [...res.data, ...res2.data].filter(o => o.bookingId === booking.id))
          );
        }),
        catchError(() => of([]))
      )
    }).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: ({ hkPending, hkInProgress, mtPending, mtInProgress, food }) => {
        this.pendingHousekeeping.set([...hkPending, ...hkInProgress]);
        this.pendingMaintenance.set([...mtPending, ...mtInProgress]);
        // Normalize status field (API may return 'status' or 'orderStatus')
        this.pendingFoodOrders.set(
          (food as any[]).map((o: any) => ({
            ...o,
            orderStatus: o.orderStatus ?? o.status ?? 'Pending'
          }))
        );
      },
      error: (err: any) => this.snackBar.open(this.extractErrorMessage(err), 'Close', { duration: 5000 })
    });
  }

  openServiceRequest(type: 'housekeeping' | 'maintenance'): void {
    const booking = this.currentBooking();
    if (!booking || !booking.rooms.length || booking.rooms[0].roomId === null) {
      return;
    }

    const roomId = booking.rooms[0].roomId as number;
    const roomNumber = booking.rooms[0].roomNumber ?? roomId.toString();

    const data: RequestServiceDialogData = { roomNumber, roomId, type };
    const dialogRef = this.dialog.open<RequestServiceDialogComponent, RequestServiceDialogData, RequestServiceDialogResult>(
      RequestServiceDialogComponent,
      { data, width: '420px' }
    );

    dialogRef.afterClosed().pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe((result: RequestServiceDialogResult | undefined) => {
      if (!result) return;

      const api$ = type === 'housekeeping'
        ? this.housekeepingApi.trigger(roomId, { description: result.description, isEmergency: result.isEmergency })
        : this.maintenanceApi.trigger(roomId, { description: result.description, isEmergency: result.isEmergency });

      api$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
        next: () => {
          this.snackBar.open(
            type === 'housekeeping' ? 'Housekeeping request submitted.' : 'Maintenance request submitted.',
            'Close',
            { duration: 4000 }
          );
        },
        error: (err: unknown) => {
          this.snackBar.open(this.extractErrorMessage(err), 'Close', { duration: 5000 });
        }
      });
    });
  }

  getRoomNumbers(booking: Booking): string {
    return booking.rooms
      .filter(r => r.roomNumber !== null)
      .map(r => r.roomNumber as string)
      .join(', ') || '—';
  }

  private extractErrorMessage(err: unknown): string {
    if (typeof err === 'string') return err;
    const e = err as { error?: { message?: string }; message?: string };
    if (e?.error?.message) return e.error.message;
    if (e?.message) return e.message;
    return 'An unexpected error occurred.';
  }
}
