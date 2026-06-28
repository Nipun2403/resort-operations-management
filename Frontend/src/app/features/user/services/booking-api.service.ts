import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Booking } from '../../../features/admin/models/booking.model';
import { PaginatedResponse } from '../../../core/models/paginated-response.model';

@Injectable({ providedIn: 'root' })
export class BookingApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.baseUrl}/bookings`;

  getAll(params: {
    status?: string;
    guestQuery?: string;
    movementStatus?: string;
    pageNumber: number;
    pageSize: number;
    sortBy?: string;
    sortDescending?: boolean;
  }): Observable<PaginatedResponse<Booking>> {
    let httpParams = new HttpParams()
      .set('pageNumber', params.pageNumber.toString())
      .set('pageSize', params.pageSize.toString())
      .set('sortBy', params.sortBy ?? 'id')
      .set('sortDescending', (params.sortDescending ?? false).toString());

    if (params.status) {
      httpParams = httpParams.set('bookingStatus', params.status);
    }
    if (params.guestQuery) {
      httpParams = httpParams.set('guestQuery', params.guestQuery);
    }
    if (params.movementStatus) {
      httpParams = httpParams.set('movementStatus', params.movementStatus);
    }

    return this.http.get<PaginatedResponse<Booking>>(this.baseUrl, { params: httpParams });
  }

  create(booking: {
    roomTypeIds: number[];
    guestCount: number;
    checkInDate: string;
    checkOutDate: string;
    guestName?: string;
    guestEmail?: string;
    amenityIds?: number[];
  }): Observable<Booking> {
    return this.http.post<Booking>(this.baseUrl, booking);
  }

  cancel(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}/cancel`);
  }

  checkIn(id: number): Observable<Booking> {
    return this.http.post<Booking>(`${this.baseUrl}/${id}/checkin`, {});
  }

  extendStay(id: number, dto: { checkOutDate: string }): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/${id}/extend-stay`, dto);
  }

  checkOut(id: number): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/checkout`, {});
  }
}
