import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { AuditLogEntry } from '../models/audit-log-entry.model';
import { PaginatedResponse } from '../../../core/models/paginated-response.model';

@Injectable({ providedIn: 'root' })
export class AuditLogApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.baseUrl}/auditlogs`;

  getAll(params: {
    guestQuery?: string;
    pageNumber: number;
    pageSize: number;
    sortBy: string;
    sortDescending: boolean;
  }): Observable<PaginatedResponse<AuditLogEntry>> {
    let httpParams = new HttpParams()
      .set('pageNumber', params.pageNumber.toString())
      .set('pageSize', params.pageSize.toString())
      .set('sortBy', params.sortBy)
      .set('sortDescending', params.sortDescending.toString());

    if (params.guestQuery) {
      httpParams = httpParams.set('guestQuery', params.guestQuery);
    }

    return this.http.get<PaginatedResponse<AuditLogEntry>>(this.baseUrl, { params: httpParams });
  }
}
