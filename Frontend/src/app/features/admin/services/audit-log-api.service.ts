import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { AuditLogEntry } from '../models/audit-log-entry.model';

@Injectable({ providedIn: 'root' })
export class AuditLogApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.baseUrl}/auditlogs`;

  getAll(params?: { sortBy?: string; sortDescending?: boolean; pageSize?: number }): Observable<AuditLogEntry[]> {
    let httpParams = new HttpParams();
    if (params?.sortBy) {
      httpParams = httpParams.set('sortBy', params.sortBy);
    }
    if (params?.sortDescending != null) {
      httpParams = httpParams.set('sortDescending', params.sortDescending.toString());
    }
    if (params?.pageSize != null) {
      httpParams = httpParams.set('pageSize', params.pageSize.toString());
    }
    return this.http.get<AuditLogEntry[]>(this.baseUrl, { params: httpParams });
  }
}
