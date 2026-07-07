import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl, Validators } from '@angular/forms';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DestroyRef } from '@angular/core';
import { finalize } from 'rxjs';
import { RoomTypeApiService } from '../../admin/services/room-type-api.service';
import { AuthService } from '../../../core/services/auth.service';
import { AvailableRoomType } from '../../user/models/available-room-type.model';

@Component({
  selector: 'app-availability',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, RouterModule,
    MatCardModule, MatButtonModule, MatIconModule, MatDatepickerModule,
    MatNativeDateModule, MatFormFieldModule, MatInputModule, MatProgressSpinnerModule,
    MatSnackBarModule
  ],
  templateUrl: './availability.component.html',
  styleUrls: ['./availability.component.scss']
})
export class AvailabilityComponent implements OnInit {
  private roomTypeApi = inject(RoomTypeApiService);
  private authService = inject(AuthService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private destroyRef = inject(DestroyRef);
  private snackBar = inject(MatSnackBar);

  minDate = new Date();

  checkIn = new FormControl<Date | null>(null, Validators.required);
  checkOut = new FormControl<Date | null>(null, Validators.required);
  guests = new FormControl(1, [Validators.required, Validators.min(1), Validators.max(20)]);

  availableRooms = signal<AvailableRoomType[]>([]);
  searchLoading = signal(false);
  searchError = signal<string | null>(null);
  hasSearched = signal(false);
  preSelectedRoomTypeId = signal<number | null>(null);

  ngOnInit(): void {
    // Automatically reset check-out if check-in changes to a date at or after check-out
    this.checkIn.valueChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(val => {
      if (val && this.checkOut.value && this.checkOut.value <= val) {
        this.checkOut.setValue(null);
      }
    });

    // Pre‑fill form from query params
    this.route.queryParams.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(params => {
      if (params['checkIn']) this.checkIn.setValue(new Date(params['checkIn']), { emitEvent: false });
      if (params['checkOut']) this.checkOut.setValue(new Date(params['checkOut']), { emitEvent: false });
      if (params['guests']) this.guests.setValue(+params['guests'], { emitEvent: false });
      if (params['roomTypeId']) {
        this.preSelectedRoomTypeId.set(+params['roomTypeId']);
      }

      // Also check session storage for pre‑selected room type ID (from detail page)
      const storedRoomId = sessionStorage.getItem('selectedRoomTypeId');
      if (storedRoomId && !this.preSelectedRoomTypeId()) {
        this.preSelectedRoomTypeId.set(Number(storedRoomId));
      }

      // Pre‑fill from availability search session storage (from home page)
      const storedSearch = sessionStorage.getItem('availabilitySearch');
      if (storedSearch) {
        try {
          const data = JSON.parse(storedSearch);
          if (data.checkIn && !this.checkIn.value) this.checkIn.setValue(new Date(data.checkIn));
          if (data.checkOut && !this.checkOut.value) this.checkOut.setValue(new Date(data.checkOut));
          if (data.guests && this.guests.value === 1) this.guests.setValue(data.guests);
        } catch { /* ignore */ }
      }

      // Auto-trigger search if both check-in and check-out dates are successfully set
      if (this.checkIn.value && this.checkOut.value) {
        this.searchAvailability();
      }
    });
  }

  searchAvailability(): void {
    if (this.checkIn.invalid || this.checkOut.invalid || this.guests.invalid) return;
    this.searchLoading.set(true);
    this.searchError.set(null);

    const formatDate = (date: Date): string => {
      const d = String(date.getDate()).padStart(2, '0');
      const m = String(date.getMonth() + 1).padStart(2, '0');
      const y = date.getFullYear();
      return `${d}-${m}-${y}`;
    };

    const params = {
      checkIn: formatDate(this.checkIn.value!),
      checkOut: formatDate(this.checkOut.value!),
      pageSize: 50,
    };
    this.roomTypeApi.getAvailability(params).pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.searchLoading.set(false))
    ).subscribe({
      next: res => {
        this.availableRooms.set(res.data);
        this.hasSearched.set(true);
      },
      error: (err: any) => this.searchError.set(this.extractErrorMessage(err))
    });
  }

  getFirstImage(room: AvailableRoomType): string {
    return room.imageUrls && room.imageUrls.length > 0 ? room.imageUrls[0] : 'assets/placeholder-room.jpg';
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

  bookNow(room: AvailableRoomType): void {
    const checkIn = this.checkIn.value!.toISOString();
    const checkOut = this.checkOut.value!.toISOString();
    const guestCount = this.guests.value!;
    const roomTypeId = room.roomTypeId;

    if (this.authService.isAuthenticated()) {
      // Navigate directly to user booking wizard with pre‑filled params
      this.router.navigate(['/user/bookings'], {
        queryParams: {
          new: true,
          roomTypeId,
          checkIn,
          checkOut,
          guests: guestCount
        }
      });
    } else {
      // Store pending booking and redirect to login
      sessionStorage.setItem('pendingBooking', JSON.stringify({
        roomTypeId,
        checkIn,
        checkOut,
        guests: guestCount
      }));
      this.router.navigate(['/auth'], { queryParams: { returnUrl: '/user/dashboard' } });
    }
  }

  private extractErrorMessage(err: any): string {
    if (typeof err === 'string') return err;
    if (err?.error?.message) return err.error.message;
    if (err?.message) return err.message;
    return 'An unexpected error occurred.';
  }
}
