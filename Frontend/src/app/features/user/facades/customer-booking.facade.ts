import { Injectable, inject } from '@angular/core';
import { Observable, of } from 'rxjs';
import { map, switchMap } from 'rxjs/operators';
import { AuthApiService } from '../../../core/services/auth-api.service';
import { BookingApiService } from '../services/booking-api.service';
import { Booking } from '../../../features/admin/models/booking.model';
import { CLAIM_TYPES } from '../../../core/utils/claim-constants';

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
          email = me.claims.find((c) => c.type === CLAIM_TYPES.NAME)?.value;
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
          firstName = findClaim(CLAIM_TYPES.GIVENNAME);
          lastName = findClaim(CLAIM_TYPES.SURNAME);
          email = findClaim(CLAIM_TYPES.NAME);
        }

        return { firstName, lastName, email };
      })
    );
  }
}

