import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { GuestProfile } from '../models/guest-profile.model';

@Injectable({ providedIn: 'root' })
export class GuestApiService {
  private http = inject(HttpClient);
  private baseUrl = environment.baseUrl;

  search(
    query: string,
  ): Observable<{ totalCount: number; data: GuestProfile[] }> {
    return this.http.get<{ totalCount: number; data: GuestProfile[] }>(
      `${this.baseUrl}/guests`,
      { params: { search: query, pageSize: 25 } },
    );
  }
}
