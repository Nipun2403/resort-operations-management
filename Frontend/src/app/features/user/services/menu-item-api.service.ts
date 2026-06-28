import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { MenuItem } from '../../../features/admin/models/menu-item.model';
import { PaginatedResponse } from '../../../core/models/paginated-response.model';

@Injectable({ providedIn: 'root' })
export class MenuItemApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.baseUrl}/menu-items`;

  getAll(params?: {
    isAvailable?: boolean;
    pageSize?: number;
    pageNumber?: number;
  }): Observable<PaginatedResponse<MenuItem>> {
    let httpParams = new HttpParams();
    if (params) {
      if (params.isAvailable !== undefined) {
        httpParams = httpParams.set('isAvailable', params.isAvailable.toString());
      }
      if (params.pageSize) {
        httpParams = httpParams.set('pageSize', params.pageSize.toString());
      }
      if (params.pageNumber) {
        httpParams = httpParams.set('pageNumber', params.pageNumber.toString());
      }
    }
    return this.http.get<PaginatedResponse<MenuItem>>(this.baseUrl, { params: httpParams });
  }
}
