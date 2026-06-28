import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Amenity } from '../../../features/admin/models/amenity.model';
import { PaginatedResponse } from '../../../core/models/paginated-response.model';

@Injectable({ providedIn: 'root' })
export class AmenityApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.baseUrl}/amenities`;

  getAll(params: {
    pageNumber: number;
    pageSize: number;
    isAvailable?: boolean;
  }): Observable<PaginatedResponse<Amenity>> {
    let httpParams = new HttpParams()
      .set('pageNumber', params.pageNumber.toString())
      .set('pageSize', params.pageSize.toString());

    if (params.isAvailable !== undefined) {
      httpParams = httpParams.set('isAvailable', params.isAvailable.toString());
    }

    return this.http.get<PaginatedResponse<Amenity>>(this.baseUrl, { params: httpParams });
  }
}
