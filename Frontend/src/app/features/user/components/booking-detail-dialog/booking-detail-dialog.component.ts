import { Component, inject, OnInit, signal, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { MatListModule } from '@angular/material/list';
import { Booking, BookingRoom } from '../../models/booking.model';
import { RoomTypeApiService } from '../../services/room-type-api.service';
import { forkJoin, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-booking-detail-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule, MatListModule, MatDividerModule],
  templateUrl: './booking-detail-dialog.component.html',
  styles: [`
    .detail-section {
      margin-bottom: 16px;
    }
    .detail-row {
      display: flex;
      margin-bottom: 8px;
      font-size: 0.95rem;
    }
    .detail-label {
      font-weight: 600;
      width: 140px;
      color: rgba(0, 0, 0, 0.6);
    }
    .detail-value {
      color: rgba(0, 0, 0, 0.87);
    }
    ul {
      margin: 4px 0 0 0;
      padding-left: 20px;
    }
  `]
})
export class BookingDetailDialogComponent implements OnInit {
  readonly booking: Booking = inject(MAT_DIALOG_DATA);
  private readonly roomTypeApi = inject(RoomTypeApiService);
  private readonly destroyRef = inject(DestroyRef);

  enrichedRooms = signal<(BookingRoom & { roomTypeName: string })[]>([]);

  ngOnInit(): void {
    this.enrichRooms();
  }

  private enrichRooms(): void {
    const rooms = this.booking.rooms ?? [];
    if (rooms.length === 0) return;

    const requests = rooms.map(room =>
      this.roomTypeApi.getById(room.roomTypeId).pipe(
        map(roomType => ({
          ...room,
          roomTypeName: roomType?.name ?? `Room Type ${room.roomTypeId}`
        })),
        catchError(() => of({
          ...room,
          roomTypeName: `Room Type ${room.roomTypeId}`
        }))
      )
    );

    forkJoin(requests).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(enriched => {
      this.enrichedRooms.set(enriched);
    });
  }
}
