import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

export interface CreateFoodOrderDTO {
  bookingId: number;
  items: { menuItemId: number; quantity: number }[];
}

@Injectable({ providedIn: 'root' })
export class OrderApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.baseUrl}/orders`;

  create(dto: CreateFoodOrderDTO): Observable<any> {
    return this.http.post<any>(this.baseUrl, dto);
  }

  getAll(params?: any): Observable<any> {
    return this.http.get<any>(this.baseUrl, { params });
  }
}
