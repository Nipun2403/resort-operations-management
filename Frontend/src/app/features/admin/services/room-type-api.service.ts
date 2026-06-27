import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { RoomType, CreateRoomTypeDTO, UpdateRoomTypeDTO } from '../models/room-type.model';
import { PaginatedResponse } from '../../../core/models/paginated-response.model';

@Injectable({ providedIn: 'root' })
export class RoomTypeApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.baseUrl}/room-types`;

  getAll(params: {
    includeRetired: boolean;
    pageNumber: number;
    pageSize: number;
    sortBy: string;
    sortDescending: boolean;
    searchQuery?: string;
  }): Observable<PaginatedResponse<RoomType>> {
    let httpParams = new HttpParams()
      .set('includeRetired', params.includeRetired.toString())
      .set('pageNumber', params.pageNumber.toString())
      .set('pageSize', params.pageSize.toString())
      .set('sortBy', params.sortBy)
      .set('sortDescending', params.sortDescending.toString());
    if (params.searchQuery) {
      httpParams = httpParams.set('searchQuery', params.searchQuery);
    }
    return this.http.get<PaginatedResponse<RoomType>>(this.baseUrl, { params: httpParams });
  }

  create(dto: CreateRoomTypeDTO): Observable<RoomType> {
    return this.http.post<RoomType>(this.baseUrl, dto);
  }

  update(id: number, dto: UpdateRoomTypeDTO): Observable<RoomType> {
    return this.http.patch<RoomType>(`${this.baseUrl}/${id}`, dto);
  }
}
