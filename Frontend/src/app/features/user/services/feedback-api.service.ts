import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Feedback, CreateFeedbackDTO } from '../models/feedback.model';

@Injectable({ providedIn: 'root' })
export class FeedbackApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.baseUrl}/feedback`;

  getByBookingId(bookingId: number): Observable<Feedback | null> {
    return this.http.get<Feedback | null>(`${this.baseUrl}/booking/${bookingId}`);
  }

  submit(dto: CreateFeedbackDTO): Observable<Feedback> {
    return this.http.post<Feedback>(this.baseUrl, dto);
  }
}
