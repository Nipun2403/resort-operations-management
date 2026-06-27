import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import { Amenity, CreateAmenityDTO, UpdateAmenityDTO } from '../models/amenity.model';
import { PaginatedResponse } from '../../../core/models/paginated-response.model';

@Injectable({
  providedIn: 'root',
})
export class AmenityApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.baseUrl}/amenities`;

  getAll(params: {
    pageNumber: number;
    pageSize: number;
    searchQuery?: string;
    sortBy: string;
    sortDescending: boolean;
  }): Observable<PaginatedResponse<Amenity>> {
    let httpParams = new HttpParams()
      .set('pageNumber', params.pageNumber.toString())
      .set('pageSize', params.pageSize.toString())
      .set('sortBy', params.sortBy)
      .set('sortDescending', params.sortDescending.toString());

    if (params.searchQuery) {
      httpParams = httpParams.set('searchQuery', params.searchQuery);
    }

    return this.http.get<PaginatedResponse<Amenity>>(this.baseUrl, { params: httpParams }).pipe(
      catchError((err) => {
        const message = err.error?.message || err.message || 'Unexpected error';
        return throwError(() => new Error(message));
      })
    );
  }

  create(dto: CreateAmenityDTO): Observable<Amenity> {
    return this.http.post<Amenity>(this.baseUrl, dto).pipe(
      catchError((err) => {
        const message = err.error?.message || err.message || 'Unexpected error';
        return throwError(() => new Error(message));
      })
    );
  }

  update(id: number, dto: UpdateAmenityDTO): Observable<{ message: string }> {
    return this.http.put<{ message: string }>(`${this.baseUrl}/${id}`, dto).pipe(
      catchError((err) => {
        const message = err.error?.message || err.message || 'Unexpected error';
        return throwError(() => new Error(message));
      })
    );
  }
}
