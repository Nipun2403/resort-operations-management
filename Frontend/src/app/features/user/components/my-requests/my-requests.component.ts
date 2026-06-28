import { Component, input, effect, inject, signal, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatSortModule } from '@angular/material/sort';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { BreakpointObserver } from '@angular/cdk/layout';
import { toSignal } from '@angular/core/rxjs-interop';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { forkJoin, Observable } from 'rxjs';
import { map, finalize } from 'rxjs/operators';
import { HousekeepingApiService } from '../../services/housekeeping-api.service';
import { MaintenanceApiService } from '../../services/maintenance-api.service';
import { CustomerRequest } from '../../models/customer-request.model';
import { AlertComponent } from '../../../../features/auth/components/alert.component';
import { RequestDetailDialogComponent } from './request-detail-dialog.component';

@Component({
  selector: 'app-my-requests',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatSortModule,
    MatProgressSpinnerModule,
    MatButtonModule,
    MatDialogModule,
    AlertComponent
  ],
  templateUrl: './my-requests.component.html',
  styleUrls: ['./my-requests.component.scss']
})
export class MyRequestsComponent {
  roomIds = input.required<number[]>();
  refresh = input(0);

  private readonly housekeepingApi = inject(HousekeepingApiService);
  private readonly maintenanceApi = inject(MaintenanceApiService);
  private readonly dialog = inject(MatDialog);
  private readonly breakpointObserver = inject(BreakpointObserver);
  private readonly destroyRef = inject(DestroyRef);

  requests = signal<CustomerRequest[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);
  displayedColumns = ['type', 'room', 'description', 'status', 'createdAt'];

  isMobile = toSignal(
    this.breakpointObserver.observe('(max-width: 599px)').pipe(map(r => r.matches)),
    { initialValue: false }
  );

  openDetail(request: CustomerRequest): void {
    this.dialog.open(RequestDetailDialogComponent, {
      data: request,
      width: '400px'
    });
  }

  constructor() {
    effect(() => {
      // Trigger fetch when roomIds or refresh trigger changes
      const ids = this.roomIds();
      const ref = this.refresh();
      if (ids && ids.length > 0) {
        this.fetchRequests();
      } else {
        this.requests.set([]);
      }
    });
  }

  fetchRequests(): void {
    const ids = this.roomIds();
    if (ids.length === 0) {
      this.requests.set([]);
      return;
    }

    this.loading.set(true);
    this.error.set(null);

    const obsList: Observable<CustomerRequest[]>[] = [];
    ids.forEach((roomId) => {
      obsList.push(
        this.housekeepingApi.getAll({ pageSize: 100 }).pipe(
          map((res) =>
            res.data
              .filter((hk) => hk.roomId === roomId)
              .map((hk) => ({
                id: hk.id,
                type: 'Housekeeping' as const,
                roomId: hk.roomId,
                roomNumber: hk.location ?? `Room ${hk.roomId}`,
                description: hk.description ?? '',
                status: hk.status,
                createdAt: hk.createdAt
              }))
          )
        )
      );
      obsList.push(
        this.maintenanceApi.getAll({ pageSize: 100 }).pipe(
          map((res) =>
            res.data
              .filter((m) => m.roomId === roomId)
              .map((m) => ({
                id: m.id,
                type: 'Maintenance' as const,
                roomId: m.roomId,
                roomNumber: m.location ?? `Room ${m.roomId}`,
                description: m.description ?? '',
                status: m.status,
                createdAt: m.createdAt
              }))
          )
        )
      );
    });

    forkJoin(obsList)
      .pipe(
        finalize(() => this.loading.set(false)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: (results) => {
          const merged = results.reduce((acc, curr) => acc.concat(curr), []);
          // Sort by createdAt descending
          merged.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
          this.requests.set(merged);
        },
        error: (err) => {
          const msg = err.error?.message || err.message || 'Failed to fetch requests.';
          this.error.set(msg);
        }
      });
  }
}
