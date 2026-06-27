import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Booking } from '../models/booking.model';
import { PaginatedResponse } from '../../../core/models/paginated-response.model';

@Injectable({
  providedIn: 'root',
})
export class BookingApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.baseUrl}/bookings`;

  getAll(params: {
    status?: string;
    guestQuery?: string;
    pageNumber: number;
    pageSize: number;
    sortBy: string;
    sortDescending: boolean;
  }): Observable<PaginatedResponse<Booking>> {
    let httpParams = new HttpParams()
      .set('pageNumber', params.pageNumber.toString())
      .set('pageSize', params.pageSize.toString())
      .set('sortBy', params.sortBy)
      .set('sortDescending', params.sortDescending.toString());

    if (params.status) {
      httpParams = httpParams.set('status', params.status);
    }
    if (params.guestQuery) {
      httpParams = httpParams.set('guestQuery', params.guestQuery);
    }

    return this.http.get<PaginatedResponse<Booking>>(this.baseUrl, { params: httpParams });
  }
}
