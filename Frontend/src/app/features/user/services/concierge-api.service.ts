import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

export interface ConciergeChatRequest {
  message: string;
  conversationId?: string;
}

export interface ConciergeConfirmRequest {
  conversationId: string;
  proposalIds: string[];
}

export interface ConciergeProposal {
  proposalId: string;
  action: string;
  summary: string;
  argumentsJson: string;
  expiresAt: string;
}

export interface ConciergeActionResult {
  toolCallId: string;
  action: string;
  success: boolean;
  resultSummary?: string;
  error?: string;
}

export interface ConciergeChatResponse {
  reply: string;
  conversationId: string;
  proposals: ConciergeProposal[];
  actions: ConciergeActionResult[];
  isComplete: boolean;
}

export interface GuestContext {
  bookingId?: number;
  roomId?: number;
  roomNumber?: string;
  checkInDate?: string;
  checkOutDate?: string;
  bookingStatus?: string;
}

@Injectable({ providedIn: 'root' })
export class ConciergeApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.baseUrl}/concierge`;

  chat(request: ConciergeChatRequest): Observable<ConciergeChatResponse> {
    return this.http.post<ConciergeChatResponse>(`${this.baseUrl}/chat`, request);
  }

  confirm(request: ConciergeConfirmRequest): Observable<ConciergeChatResponse> {
    return this.http.post<ConciergeChatResponse>(`${this.baseUrl}/confirm`, request);
  }

  getPendingProposals(conversationId: string): Observable<ConciergeProposal[]> {
    return this.http.get<ConciergeProposal[]>(`${this.baseUrl}/proposals`, {
      params: { conversationId }
    });
  }

  getContext(): Observable<GuestContext> {
    return this.http.get<GuestContext>(`${this.baseUrl}/context`);
  }
}