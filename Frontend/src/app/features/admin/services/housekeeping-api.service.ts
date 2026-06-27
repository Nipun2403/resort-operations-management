import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { HousekeepingTask } from '../models/housekeeping-task.model';
import { PaginatedResponse } from '../../../core/models/paginated-response.model';
import { CreateInternalTicketRequest } from '../models/create-internal-ticket-request.dto';

@Injectable({ providedIn: 'root' })
export class HousekeepingApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.baseUrl}/housekeeping`;

  getAll(params?: { status?: string; pageNumber?: number; pageSize?: number }): Observable<PaginatedResponse<HousekeepingTask>> {
    let httpParams = new HttpParams();
    if (params?.status) {
      httpParams = httpParams.set('status', params.status);
    }
    if (params?.pageNumber != null) {
      httpParams = httpParams.set('pageNumber', params.pageNumber.toString());
    }
    if (params?.pageSize != null) {
      httpParams = httpParams.set('pageSize', params.pageSize.toString());
    }
    return this.http.get<PaginatedResponse<HousekeepingTask>>(this.baseUrl, { params: httpParams });
  }

  createInternal(body: CreateInternalTicketRequest): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/internal`, body);
  }
}
