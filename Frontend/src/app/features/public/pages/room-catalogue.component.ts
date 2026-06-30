import { Component, inject, signal, computed, OnInit } from '@angular/core';
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

  // Pagination
  currentGroupIndex = signal(0);
  roomsPerGroup = 4;
  totalGroups = computed(() => Math.ceil(this.rooms().length / this.roomsPerGroup));

  // Rooms to display in the current grid
  displayedRooms = computed(() => {
    const start = this.currentGroupIndex() * this.roomsPerGroup;
    return this.rooms().slice(start, start + this.roomsPerGroup);
  });

  // Transition state for gold wash animation
  isTransitioning = signal(false);
  private readonly ANIMATION_DURATION = 600; // ms

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

  // Navigation methods
  nextGroup(): void {
    if (this.currentGroupIndex() < this.totalGroups() - 1) {
      this.triggerTransition(() => this.currentGroupIndex.update(i => i + 1));
    }
  }

  previousGroup(): void {
    if (this.currentGroupIndex() > 0) {
      this.triggerTransition(() => this.currentGroupIndex.update(i => i - 1));
    }
  }

  private triggerTransition(updateFn: () => void): void {
    if (this.isTransitioning()) return;
    this.isTransitioning.set(true);
    // Start animation, then update data after a short delay so the wash is visible
    setTimeout(() => {
      updateFn();
      // Remove transitioning class after the animation completes
      setTimeout(() => this.isTransitioning.set(false), this.ANIMATION_DURATION);
    }, 100);
  }

  // Wheel / touch detection for horizontal scroll
  private touchStartX = 0;
  onTouchStart(event: TouchEvent): void {
    this.touchStartX = event.changedTouches[0].screenX;
  }
  onTouchEnd(event: TouchEvent): void {
    const deltaX = event.changedTouches[0].screenX - this.touchStartX;
    if (deltaX < -50) this.nextGroup();
    else if (deltaX > 50) this.previousGroup();
  }
  onWheel(event: WheelEvent): void {
    // Only act if the user is scrolling horizontally over the grid area
    if (Math.abs(event.deltaX) > Math.abs(event.deltaY) && Math.abs(event.deltaX) > 30) {
      event.preventDefault();
      if (event.deltaX > 0) this.nextGroup();
      else this.previousGroup();
    }
  }

  getCardClasses(index: number): Record<string, boolean> {
    return {
      'card-large': index === 0,
      'card-small': index === 1,
      'card-medium': index === 2,
      'card-wide': index === 3,
    };
  }

  private extractErrorMessage(err: any): string {
    if (typeof err === 'string') return err;
    if (err?.error?.message) return err.error.message;
    if (err?.message) return err.message;
    return 'An unexpected error occurred.';
  }
}
