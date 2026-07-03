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
        let email = me.email;
        if (!email && me.claims) {
          email = me.claims.find((c) => c.type === 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name')?.value;
        }
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
      map((me) => {
        let firstName = me.firstName ?? '';
        let lastName = me.lastName ?? '';
        let email = me.email ?? '';

        if (!email && me.claims) {
          const findClaim = (type: string) => me.claims?.find((c) => c.type === type)?.value ?? '';
          firstName = findClaim('http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname');
          lastName = findClaim('http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname');
          email = findClaim('http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name');
        }

        return { firstName, lastName, email };
      })
    );
  }
}

