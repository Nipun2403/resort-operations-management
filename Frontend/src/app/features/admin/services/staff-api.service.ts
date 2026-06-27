import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import { Staff, CreateStaffDTO, UpdateStaffDTO } from '../models/staff.model';
import { PaginatedResponse } from '../../../core/models/paginated-response.model';

@Injectable({
  providedIn: 'root',
})
export class StaffApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.baseUrl}/staff`;

  getAll(params: {
    includeFired: boolean;
    pageNumber: number;
    pageSize: number;
    sortBy: string;
    sortDescending: boolean;
    searchQuery?: string;
  }): Observable<PaginatedResponse<Staff>> {
    let httpParams = new HttpParams()
      .set('includeFired', params.includeFired.toString())
      .set('pageNumber', params.pageNumber.toString())
      .set('pageSize', params.pageSize.toString())
      .set('sortBy', params.sortBy)
      .set('sortDescending', params.sortDescending.toString());

    if (params.searchQuery) {
      httpParams = httpParams.set('searchQuery', params.searchQuery);
    }

    return this.http.get<PaginatedResponse<Staff>>(this.baseUrl, { params: httpParams }).pipe(
      catchError((err) => {
        const message = err.error?.message || err.message || 'Unexpected error';
        return throwError(() => new Error(message));
      })
    );
  }

  create(dto: CreateStaffDTO): Observable<Staff> {
    return this.http.post<Staff>(this.baseUrl, dto).pipe(
      catchError((err) => {
        const message = err.error?.message || err.message || 'Unexpected error';
        return throwError(() => new Error(message));
      })
    );
  }

  update(id: number, dto: UpdateStaffDTO): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/${id}`, dto).pipe(
      catchError((err) => {
        const message = err.error?.message || err.message || 'Unexpected error';
        return throwError(() => new Error(message));
      })
    );
  }
}
