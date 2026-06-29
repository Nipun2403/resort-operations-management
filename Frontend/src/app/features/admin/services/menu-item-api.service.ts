import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import { MenuItem, CreateMenuItemDTO, UpdateMenuItemDTO } from '../models/menu-item.model';
import { PaginatedResponse } from '../../../core/models/paginated-response.model';

@Injectable({
  providedIn: 'root',
})
export class MenuItemApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.baseUrl}/menu-items`;

  getAll(params: {
    pageNumber: number;
    pageSize: number;
    searchQuery?: string;
    sortBy: string;
    sortDescending: boolean;
    isAvailable?: boolean;
  }): Observable<PaginatedResponse<MenuItem>> {
    let httpParams = new HttpParams()
      .set('pageNumber', params.pageNumber.toString())
      .set('pageSize', params.pageSize.toString())
      .set('sortBy', params.sortBy)
      .set('sortDescending', params.sortDescending.toString());

    if (params.searchQuery) {
      httpParams = httpParams.set('searchQuery', params.searchQuery);
    }

    if (params.isAvailable !== undefined) {
      httpParams = httpParams.set('isAvailable', params.isAvailable.toString());
    }

    return this.http.get<PaginatedResponse<MenuItem>>(this.baseUrl, { params: httpParams }).pipe(
      catchError((err) => {
        const message = err.error?.message || err.message || 'Unexpected error';
        return throwError(() => new Error(message));
      })
    );
  }

  create(dto: CreateMenuItemDTO): Observable<MenuItem> {
    return this.http.post<MenuItem>(this.baseUrl, dto).pipe(
      catchError((err) => {
        const message = err.error?.message || err.message || 'Unexpected error';
        return throwError(() => new Error(message));
      })
    );
  }

  update(id: number, dto: UpdateMenuItemDTO): Observable<MenuItem> {
    return this.http.put<MenuItem>(`${this.baseUrl}/${id}`, dto).pipe(
      catchError((err) => {
        const message = err.error?.message || err.message || 'Unexpected error';
        return throwError(() => new Error(message));
      })
    );
  }

  updateStatus(id: number, isAvailable: boolean): Observable<void> {
    const params = new HttpParams().set('isAvailable', isAvailable.toString());
    return this.http.patch<void>(`${this.baseUrl}/${id}/status`, null, { params }).pipe(
      catchError((err) => {
        const message = err.error?.message || err.message || 'Unexpected error';
        return throwError(() => new Error(message));
      })
    );
  }
}
