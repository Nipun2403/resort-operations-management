import { Injectable, inject } from '@angular/core';
import { Observable, of } from 'rxjs';
import { map, switchMap } from 'rxjs/operators';
import { AuthApiService } from '../../../core/services/auth-api.service';
import { BookingApiService } from '../services/booking-api.service';
import { Booking } from '../../../features/admin/models/booking.model';

export interface CustomerProfile {
  firstName: string;
  lastName: string;
  email: string;
}

@Injectable({ providedIn: 'root' })
export class CustomerBookingFacade {
  private readonly authApi = inject(AuthApiService);
  private readonly bookingApi = inject(BookingApiService);

  getActiveBooking(): Observable<Booking | null> {
    return this.authApi.getMe().pipe(
      switchMap((me) => {
        const email =
          me.claims?.find((c) => c.type === 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name')?.value ?? '';
        if (!email) return of(null);
        return this.bookingApi
          .getAll({
            guestQuery: email,
            status: 'CheckedIn',
            pageNumber: 1,
            pageSize: 1,
            sortBy: 'bookedAt',
            sortDescending: true
          })
          .pipe(map((res) => (res.data.length > 0 ? res.data[0] : null)));
      })
    );
  }

  getCurrentCustomerProfile(): Observable<CustomerProfile> {
    return this.authApi.getMe().pipe(
      map((me) => ({
        firstName:
          me.claims?.find((c) => c.type === 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname')?.value ??
          '',
        lastName:
          me.claims?.find((c) => c.type === 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname')?.value ?? '',
        email:
          me.claims?.find((c) => c.type === 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name')?.value ?? '',
      }))
    );
  }
}
