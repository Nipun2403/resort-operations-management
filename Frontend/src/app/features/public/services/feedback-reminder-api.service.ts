import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Feedback } from '../../admin/models/feedback.model';
import { FeedbackReminderValidation } from '../models/feedback-reminder.model';

@Injectable({ providedIn: 'root' })
export class FeedbackReminderApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.baseUrl}/feedback/public`;

  validateToken(token: string): Observable<FeedbackReminderValidation> {
    return this.http.get<FeedbackReminderValidation>(`${this.baseUrl}/validate/${token}`);
  }

  submit(token: string, rating: number, comments: string): Observable<Feedback> {
    return this.http.post<Feedback>(`${this.baseUrl}/submit`, { token, rating, comments });
  }
}
