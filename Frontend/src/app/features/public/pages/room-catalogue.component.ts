import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { ReactiveFormsModule, FormControl } from '@angular/forms';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DestroyRef } from '@angular/core';
import { finalize } from 'rxjs';
import { RoomTypeApiService } from '../../admin/services/room-type-api.service';
import { RoomType } from '../../admin/models/room-type.model';

@Component({
  selector: 'app-room-catalogue',
  standalone: true,
  imports: [
    CommonModule, RouterModule, ReactiveFormsModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './room-catalogue.component.html',
  styleUrls: ['./room-catalogue.component.scss']
})
export class RoomCatalogueComponent implements OnInit {
  private roomTypeApi = inject(RoomTypeApiService);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);

  rooms = signal<RoomType[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);

  emailControl = new FormControl('', { nonNullable: true });
  subscribed = signal(false);

  ngOnInit(): void {
    this.fetchRooms();
  }

  fetchRooms(): void {
    this.loading.set(true);
    this.roomTypeApi.getAll({ includeRetired: false, pageNumber: 1, pageSize: 100, sortBy: 'name', sortDescending: false }).pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.loading.set(false))
    ).subscribe({
      next: res => this.rooms.set(res.data),
      error: (err: any) => this.error.set(this.extractErrorMessage(err))
    });
  }

  getFirstImage(room: RoomType): string {
    return room.imageUrls && room.imageUrls.length > 0 ? room.imageUrls[0] : 'assets/placeholder-room.jpg';
  }

  viewRoom(roomId: number): void {
    this.router.navigate(['/rooms', roomId]);
  }

  subscribe(): void {
    if (!this.emailControl.value || this.subscribed()) return;
    this.emailControl.setValue('');
    this.subscribed.set(true);
    // TODO: wire up newsletter subscription to backend
  }

  private extractErrorMessage(err: any): string {
    if (typeof err === 'string') return err;
    if (err?.error?.message) return err.error.message;
    if (err?.message) return err.message;
    return 'An unexpected error occurred.';
  }
}
