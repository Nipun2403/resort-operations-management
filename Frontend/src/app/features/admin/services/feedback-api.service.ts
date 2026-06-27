import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Feedback, ModerateFeedbackRequest } from '../models/feedback.model';
import { PaginatedResponse } from '../../../core/models/paginated-response.model';

@Injectable({
  providedIn: 'root',
})
export class FeedbackApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.baseUrl}/feedback`;

  getAll(params: {
    includeHidden: boolean;
    pageNumber: number;
    pageSize: number;
    sortBy: string;
    sortDescending: boolean;
  }): Observable<PaginatedResponse<Feedback>> {
    const httpParams = new HttpParams()
      .set('includeHidden', params.includeHidden.toString())
      .set('pageNumber', params.pageNumber.toString())
      .set('pageSize', params.pageSize.toString())
      .set('sortBy', params.sortBy)
      .set('sortDescending', params.sortDescending.toString());

    return this.http.get<PaginatedResponse<Feedback>>(this.baseUrl, {
      params: httpParams,
    });
  }

  moderate(id: number, request: ModerateFeedbackRequest): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/${id}/moderate`, request);
  }
}
