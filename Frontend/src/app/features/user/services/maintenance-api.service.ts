import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { MaintenanceTask } from '../../admin/models/maintenance-task.model';
import { PaginatedResponse } from '../../../core/models/paginated-response.model';

@Injectable({ providedIn: 'root' })
export class MaintenanceApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.baseUrl}/maintenance`;

  trigger(roomId: number, body: { description: string }): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/trigger/${roomId}`, body);
  }

  getAll(params?: {
    pageNumber?: number;
    pageSize?: number;
    status?: string;
    roomId?: number;
  }): Observable<PaginatedResponse<MaintenanceTask>> {
    let httpParams = new HttpParams();
    if (params) {
      if (params.pageNumber) httpParams = httpParams.set('pageNumber', params.pageNumber.toString());
      if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());
      if (params.status) httpParams = httpParams.set('status', params.status);
      if (params.roomId) httpParams = httpParams.set('roomId', params.roomId.toString());
    }
    return this.http.get<PaginatedResponse<MaintenanceTask>>(this.baseUrl, { params: httpParams });
  }
}
