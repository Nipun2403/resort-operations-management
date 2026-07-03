import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Receipt } from '../models/receipt.model';
import { PaginatedResponse } from '../../../core/models/paginated-response.model';

@Injectable({
  providedIn: 'root',
})
export class BillingApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.baseUrl}/billing`;

  getReceipts(params: {
    startDate?: string;
    endDate?: string;
    pageNumber: number;
    pageSize: number;
    sortBy: string;
    sortDescending: boolean;
  }): Observable<PaginatedResponse<Receipt>> {
    let httpParams = new HttpParams()
      .set('pageNumber', params.pageNumber.toString())
      .set('pageSize', params.pageSize.toString())
      .set('sortBy', params.sortBy)
      .set('sortDescending', params.sortDescending.toString());

    if (params.startDate) {
      httpParams = httpParams.set('startDate', params.startDate);
    }
    if (params.endDate) {
      httpParams = httpParams.set('endDate', params.endDate);
    }

    return this.http.get<PaginatedResponse<Receipt>>(`${this.baseUrl}/receipts`, {
      params: httpParams,
    });
  }

  downloadFolioPdf(bookingId: number): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/${bookingId}/folio/pdf`, { responseType: 'blob' });
  }
}
