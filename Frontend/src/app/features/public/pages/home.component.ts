import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl, Validators } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DestroyRef } from '@angular/core';
import { finalize } from 'rxjs';
import { RoomTypeApiService } from '../../admin/services/room-type-api.service';
import { RoomType } from '../../admin/models/room-type.model';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, RouterModule,
    MatButtonModule, MatCardModule, MatIconModule, MatDatepickerModule,
    MatNativeDateModule, MatFormFieldModule, MatInputModule, MatProgressSpinnerModule
  ],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit {
  private roomTypeApi = inject(RoomTypeApiService);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);

  featuredRooms = signal<RoomType[]>([]);
  roomsLoading = signal(false);
  roomsError = signal<string | null>(null);

  minDate = new Date();

  checkIn = new FormControl<Date | null>(null, Validators.required);
  checkOut = new FormControl<Date | null>(null, Validators.required);
  guests = new FormControl(1, [Validators.required, Validators.min(1), Validators.max(20)]);

  ngOnInit(): void {
    this.fetchFeaturedRooms();
    
    // Automatically reset check-out if check-in changes to a date at or after check-out
    this.checkIn.valueChanges.subscribe(val => {
      if (val && this.checkOut.value && this.checkOut.value <= val) {
        this.checkOut.setValue(null);
      }
    });
  }

  getMinCheckOutDate(): Date {
    if (this.checkIn.value) {
      const checkInDate = new Date(this.checkIn.value);
      checkInDate.setDate(checkInDate.getDate() + 1);
      return checkInDate;
    }
    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    return tomorrow;
  }

  private fetchFeaturedRooms(): void {
    this.roomsLoading.set(true);
    this.roomTypeApi.getAll({ includeRetired: false, pageNumber: 1, pageSize: 6, sortBy: 'basePrice', sortDescending: false }).pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.roomsLoading.set(false))
    ).subscribe({
      next: res => this.featuredRooms.set(res.data),
      error: (err: any) => this.roomsError.set(this.extractErrorMessage(err))
    });
  }

  getFirstImage(room: RoomType): string {
    return room.imageUrls && room.imageUrls.length > 0 ? room.imageUrls[0] : 'assets/placeholder-room.jpg';
  }

  viewRoom(roomId: number): void {
    this.router.navigate(['/rooms', roomId]);
  }

  searchAvailability(): void {
    if (this.checkIn.value && this.checkOut.value) {
      const checkIn = this.checkIn.value.toISOString();
      const checkOut = this.checkOut.value.toISOString();
      const guestCount = this.guests.value || 1;
      // Store for later booking flow
      sessionStorage.setItem('availabilitySearch', JSON.stringify({ checkIn, checkOut, guests: guestCount }));
      this.router.navigate(['/availability'], { queryParams: { checkIn, checkOut, guests: guestCount } });
    } else {
      this.router.navigate(['/availability']);
    }
  }

  private extractErrorMessage(err: any): string {
    if (typeof err === 'string') return err;
    if (err?.error?.message) return err.error.message;
    if (err?.message) return err.message;
    return 'An unexpected error occurred.';
  }
}
