import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class HousekeepingApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.baseUrl}/housekeeping`;

  trigger(roomId: number, body: { description: string }): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/trigger/${roomId}`, body);
  }
}
