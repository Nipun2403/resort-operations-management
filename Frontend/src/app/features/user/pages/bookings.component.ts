import { Component, inject, signal, OnInit, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
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
    MatButtonToggleModule,
    MatProgressSpinnerModule,
    BookingHistoryComponent,
    BookingWizardComponent
  ],
  templateUrl: './bookings.component.html',
  styleUrls: ['./bookings.component.scss']
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
    this.route.queryParams.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(params => {
      if (params['new'] === 'true') {
        this.viewMode.setValue('new'); // switch to new booking view
        // Set pre‑fill values
        this.initialCheckIn = params['checkIn'] ? new Date(params['checkIn']) : null;
        this.initialCheckOut = params['checkOut'] ? new Date(params['checkOut']) : null;
        this.initialGuests = params['guests'] ? +params['guests'] : null;
        this.initialRoomTypeId = params['roomTypeId'] ? +params['roomTypeId'] : null;
      }
    });

    this.authApi.getMe().pipe(takeUntilDestroyed(this.destroyRef)).subscribe(me => {
      const given = me.claims?.find(c => c.type === 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname')?.value ?? '';
      const surname = me.claims?.find(c => c.type === 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname')?.value ?? '';
      const email = me.claims?.find(c => c.type === 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name')?.value ?? '';
      this.userEmail.set(email);
      this.userProfile.set({ firstName: given, lastName: surname, email });
    });
  }

  onBookingCreated(bookingId: number): void {
    this.newBookingId.set(bookingId);
    this.refreshTrigger.update(n => n + 1);
    this.viewMode.setValue('history');
  }
}
