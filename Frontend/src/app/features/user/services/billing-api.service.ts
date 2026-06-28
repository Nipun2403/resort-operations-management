import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { BillingFolio } from '../models/billing-folio.model';

@Injectable({ providedIn: 'root' })
export class BillingApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.baseUrl}/billing`;

  getByBookingId(bookingId: number): Observable<BillingFolio> {
    return this.http.get<BillingFolio>(`${this.baseUrl}/${bookingId}`);
  }
}
