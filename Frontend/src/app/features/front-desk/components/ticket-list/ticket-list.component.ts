import { Component, input, OnInit, inject, signal, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatSortModule } from '@angular/material/sort';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonModule } from '@angular/material/button';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { forkJoin, Observable, of } from 'rxjs';
import { map, finalize, catchError } from 'rxjs/operators';
import { HousekeepingApiService } from '../../../user/services/housekeeping-api.service';
import { MaintenanceApiService } from '../../../user/services/maintenance-api.service';
import { OrderApiService } from '../../../user/services/order-api.service';
import { AlertComponent } from '../../../auth/components/alert.component';

@Component({
  selector: 'app-ticket-list',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatSortModule,
    MatProgressSpinnerModule,
    MatButtonModule,
    AlertComponent,
  ],
  templateUrl: './ticket-list.component.html',
  styleUrls: ['./ticket-list.component.scss'],
})
export class TicketListComponent implements OnInit {
  type = input.required<'housekeeping' | 'maintenance' | 'foodOrder'>();

  private readonly hkApi = inject(HousekeepingApiService);
  private readonly mtApi = inject(MaintenanceApiService);
  private readonly orderApi = inject(OrderApiService);
  private readonly destroyRef = inject(DestroyRef);

  displayedColumns = ['id', 'room', 'description', 'status', 'isEmergency', 'createdAt'];

  tickets = signal<any[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);

  ngOnInit(): void {
    this.fetch();
  }

  fetch(): void {
    this.loading.set(true);
    this.error.set(null);
    let request$: Observable<any[]>;

    switch (this.type()) {
      case 'housekeeping':
        request$ = forkJoin([
          this.hkApi
            .getAll({ status: 'Pending', pageSize: 200, sortBy: 'id', sortDescending: true })
            .pipe(
              map((r) => r.data),
              catchError(() => of([] as any[])),
            ),
          this.hkApi
            .getAll({ status: 'InProgress', pageSize: 200, sortBy: 'id', sortDescending: true })
            .pipe(
              map((r) => r.data),
              catchError(() => of([] as any[])),
            ),
        ]).pipe(map(([p, ip]) => [...(p || []), ...(ip || [])]));
        break;
      case 'maintenance':
        request$ = forkJoin([
          this.mtApi
            .getAll({ status: 'Pending', pageSize: 200, sortBy: 'id', sortDescending: true })
            .pipe(
              map((r) => r.data),
              catchError(() => of([] as any[])),
            ),
          this.mtApi
            .getAll({ status: 'InProgress', pageSize: 200, sortBy: 'id', sortDescending: true })
            .pipe(
              map((r) => r.data),
              catchError(() => of([] as any[])),
            ),
        ]).pipe(map(([p, ip]) => [...(p || []), ...(ip || [])]));
        break;
      case 'foodOrder':
        request$ = forkJoin([
          this.orderApi
            .getAll({ status: 'Pending', pageSize: 200, sortBy: 'id', sortDescending: true })
            .pipe(
              map((r) => r.data),
              catchError(() => of([] as any[])),
            ),
          this.orderApi
            .getAll({ status: 'Preparing', pageSize: 200, sortBy: 'id', sortDescending: true })
            .pipe(
              map((r) => r.data),
              catchError(() => of([] as any[])),
            ),
        ]).pipe(map(([p, ip]) => [...(p || []), ...(ip || [])]));
        break;
    }

    request$
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.loading.set(false)),
      )
      .subscribe({
        next: (data) => {
          const normalized = data.map((t) => {
            let description = t.description;
            if (this.type() === 'foodOrder') {
              const itemsArray = t.orderItems || [];
              description =
                itemsArray.length > 0
                  ? itemsArray
                      .map(
                        (i: any) => `${i.quantity}x ${i.menuItemName ?? 'Item #' + i.menuItemId}`,
                      )
                      .join(', ')
                  : `Order #${t.id}`;
            }
            return {
              ...t,
              status: t.orderStatus ?? t.status ?? 'Pending',
              roomNumber: t.roomNumber ?? (t.roomId ? 'Room ' + t.roomId : 'N/A'),
              description: description ?? `Order #${t.id}`,
              createdAt: t.generatedAt ?? t.createdAt ?? '',
            };
          });
          this.tickets.set(normalized);
        },
        error: (err) => this.error.set(this.extractErrorMessage(err)),
      });
  }

  private extractErrorMessage(err: any): string {
    return err?.error?.message || err?.message || 'Failed to fetch tickets.';
  }
}
