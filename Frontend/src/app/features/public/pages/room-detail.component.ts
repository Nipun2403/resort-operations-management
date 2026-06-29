import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DestroyRef } from '@angular/core';
import { finalize } from 'rxjs';
import { RoomTypeApiService } from '../../admin/services/room-type-api.service';
import { RoomType } from '../../admin/models/room-type.model';

@Component({
  selector: 'app-room-detail',
  standalone: true,
  imports: [
    CommonModule, RouterModule,
    MatCardModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule
  ],
  templateUrl: './room-detail.component.html',
  styleUrls: ['./room-detail.component.scss']
})
export class RoomDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private roomTypeApi = inject(RoomTypeApiService);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);

  room = signal<RoomType | null>(null);
  loading = signal(false);
  error = signal<string | null>(null);

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (id) {
      this.fetchRoom(id);
    } else {
      this.error.set('Room not found.');
    }
  }

  private fetchRoom(id: number): void {
    this.loading.set(true);
    this.roomTypeApi.getById(id).pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.loading.set(false))
    ).subscribe({
      next: (data: any) => this.room.set(data),
      error: (err: any) => this.error.set(this.extractErrorMessage(err))
    });
  }

  getBedEntries(): [string, number][] {
    const config = this.room()?.bedConfiguration;
    if (!config) return [];
    return Object.entries(config).filter(([, v]) => v > 0);
  }

  checkAvailability(): void {
    const roomId = this.room()?.id;
    if (roomId) {
      // Store room type ID for later booking flow
      sessionStorage.setItem('selectedRoomTypeId', String(roomId));
      this.router.navigate(['/availability'], { queryParams: { roomTypeId: roomId } });
    }
  }

  private extractErrorMessage(err: any): string {
    if (typeof err === 'string') return err;
    if (err?.error?.message) return err.error.message;
    if (err?.message) return err.message;
    return 'An unexpected error occurred.';
  }
}
