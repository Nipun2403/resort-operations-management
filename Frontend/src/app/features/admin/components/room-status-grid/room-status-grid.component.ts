import { Component, inject, signal, input, output, effect, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonModule } from '@angular/material/button';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs/operators';

import { RoomApiService } from '../../services/room-api.service';
import { RoomStatus } from '../../models/room.model';
import { AlertComponent } from '../../../../features/auth/components/alert.component';

@Component({
  selector: 'app-room-status-grid',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatIconModule,
    MatTooltipModule,
    MatProgressSpinnerModule,
    MatButtonModule,
    AlertComponent,
  ],
  templateUrl: './room-status-grid.component.html',
  styleUrls: ['./room-status-grid.component.scss'],
})
export class RoomStatusGridComponent {
  roomTypeId = input<number | null>(null);
  roomClicked = output<RoomStatus>();

  rooms = signal<RoomStatus[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);

  private readonly roomApi = inject(RoomApiService);
  private readonly destroyRef = inject(DestroyRef);

  constructor() {
    effect(() => {
      // re-fetch when roomTypeId changes
      this.roomTypeId();
      this.fetchStatuses();
    });
  }

  fetchStatuses(): void {
    this.loading.set(true);
    this.error.set(null);
    this.roomApi
      .getStatuses({
        pageNumber: 1,
        pageSize: 100,
        roomTypeId: this.roomTypeId() ?? undefined,
        sortDescending: false,
      })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.loading.set(false)),
      )
      .subscribe({
        next: (res) => this.rooms.set(res.data),
        error: (err: Error) => this.error.set(err.message),
      });
  }

  tooltipContent(room: RoomStatus): string {
    if (room.status === 'Occupied') {
      return `Occupied - ${room.currentGuestName ?? 'Guest'}`;
    }
    return 'Available';
  }

  getStatusClass(status: string | null | undefined): string {
    const normalized = (status ?? '').toLowerCase();
    if (normalized === 'occupied') return 'occupied';
    if (normalized === 'available') return 'available';
    return 'neutral';
  }
}
