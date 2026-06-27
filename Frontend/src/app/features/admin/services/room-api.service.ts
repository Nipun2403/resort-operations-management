import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Room, CreateRoomDTO, UpdateRoomDTO, RoomStatus } from '../models/room.model';
import { PaginatedResponse } from '../../../core/models/paginated-response.model';

@Injectable({ providedIn: 'root' })
export class RoomApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.baseUrl}/rooms`;

  getAll(params: {
    pageNumber: number;
    pageSize: number;
    roomTypeId?: number;
    includeRetired: boolean;
    searchQuery?: string;
    sortBy: string;
    sortDescending: boolean;
  }): Observable<PaginatedResponse<Room>> {
    let httpParams = new HttpParams()
      .set('pageNumber', params.pageNumber.toString())
      .set('pageSize', params.pageSize.toString())
      .set('includeRetired', params.includeRetired.toString())
      .set('sortBy', params.sortBy)
      .set('sortDescending', params.sortDescending.toString());
    if (params.roomTypeId != null) {
      httpParams = httpParams.set('roomTypeId', params.roomTypeId.toString());
    }
    if (params.searchQuery) {
      httpParams = httpParams.set('searchQuery', params.searchQuery);
    }
    return this.http.get<PaginatedResponse<Room>>(this.baseUrl, { params: httpParams });
  }

  create(dto: CreateRoomDTO): Observable<Room> {
    return this.http.post<Room>(this.baseUrl, dto);
  }

  update(id: number, dto: UpdateRoomDTO): Observable<{ message: string }> {
    return this.http.patch<{ message: string }>(`${this.baseUrl}/${id}`, dto);
  }

  getStatuses(params: {
    pageNumber: number;
    pageSize: number;
    roomTypeId?: number;
    sortDescending: boolean;
  }): Observable<PaginatedResponse<RoomStatus>> {
    let httpParams = new HttpParams()
      .set('pageNumber', params.pageNumber.toString())
      .set('pageSize', params.pageSize.toString())
      .set('sortDescending', params.sortDescending.toString());
    if (params.roomTypeId != null) {
      httpParams = httpParams.set('roomTypeId', params.roomTypeId.toString());
    }
    return this.http.get<PaginatedResponse<RoomStatus>>(`${this.baseUrl}/status`, {
      params: httpParams,
    });
  }
}
