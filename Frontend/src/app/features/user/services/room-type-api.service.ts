import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { AvailableRoomType } from '../models/available-room-type.model';
import { PaginatedResponse } from '../../../core/models/paginated-response.model';

@Injectable({ providedIn: 'root' })
export class RoomTypeApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.baseUrl}/room-types`;

  getAvailable(
    checkIn: string,
    checkOut: string,
    pageNumber: number = 1,
    pageSize: number = 100
  ): Observable<PaginatedResponse<AvailableRoomType>> {
    const params = new HttpParams()
      .set('checkIn', checkIn)
      .set('checkOut', checkOut)
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<PaginatedResponse<AvailableRoomType>>(`${this.baseUrl}/availability`, { params });
  }
}
