import { Component, inject, signal, OnInit, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AuthApiService } from '../../../core/services/auth-api.service';
import { BookingHistoryComponent } from '../components/booking-history/booking-history.component';
import { BookingWizardComponent } from '../components/booking-wizard/booking-wizard.component';

@Component({
  selector: 'app-customer-bookings',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatProgressSpinnerModule,
    BookingHistoryComponent,
    BookingWizardComponent,
  ],
  templateUrl: './bookings.component.html',
  styleUrls: ['./bookings.component.scss'],
})
export class BookingsComponent implements OnInit {
  viewMode = new FormControl<'history' | 'new'>('new', { nonNullable: true });
  userEmail = signal('');
  userProfile = signal<{ firstName: string; lastName: string; email: string } | null>(null);

  refreshTrigger = signal(0);
  newBookingId = signal<number | null>(null);

  initialCheckIn: Date | null = null;
  initialCheckOut: Date | null = null;
  initialGuests: number | null = null;
  initialRoomTypeId: number | null = null;

  private readonly authApi = inject(AuthApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);

  ngOnInit(): void {
    this.route.queryParams.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((params) => {
      if (params['new'] === 'true') {
        this.viewMode.setValue('new');
        this.initialCheckIn = params['checkIn'] ? new Date(params['checkIn']) : null;
        this.initialCheckOut = params['checkOut'] ? new Date(params['checkOut']) : null;
        this.initialGuests = params['guests'] ? +params['guests'] : null;
        this.initialRoomTypeId = params['roomTypeId'] ? +params['roomTypeId'] : null;
      }
    });

    this.authApi
      .getMe()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (me: any) => {
          let email = '';
          let firstName = '';
          let lastName = '';

          // Try claims array first (JWT style)
          if (Array.isArray(me.claims)) {
            const findClaim = (type: string) =>
              me.claims.find((c: any) => c.type === type)?.value ?? '';

            firstName = findClaim('http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname');
            lastName = findClaim('http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname');
            email = findClaim('http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name');
          } else {
            // Fallback to flat object (like ProfileComponent)
            email = me.email ?? '';
            firstName = me.firstName ?? '';
            lastName = me.lastName ?? '';
          }

          this.userEmail.set(email);
          this.userProfile.set({ firstName, lastName, email });
        },
        error: (err) => {
          console.error('Failed to load user profile:', err);
          this.userProfile.set(null);
        },
      });
  }

  onBookingCreated(bookingId: number): void {
    this.newBookingId.set(bookingId);
    this.refreshTrigger.update((n) => n + 1);
    this.viewMode.setValue('history');
  }
}
