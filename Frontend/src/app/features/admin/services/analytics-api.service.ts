import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { AnalyticsDashboardDTO } from '../models/analytics-dashboard.dto';

@Injectable({ providedIn: 'root' })
export class AnalyticsApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.baseUrl}/analytics`;

  getAnalytics(params?: { startDate?: string; endDate?: string }): Observable<AnalyticsDashboardDTO> {
    let httpParams = new HttpParams();
    if (params?.startDate) {
      httpParams = httpParams.set('startDate', params.startDate);
    }
    if (params?.endDate) {
      httpParams = httpParams.set('endDate', params.endDate);
    }
    return this.http.get<AnalyticsDashboardDTO>(this.baseUrl, { params: httpParams });
  }
}
