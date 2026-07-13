import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders, HttpInterceptorFn, HttpRequest, HttpHandlerFn, HttpEvent } from '@angular/common/http';
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

export interface ConciergeErrorResponse {
  errorCode: string;
  message: string;
  details?: Record<string, string[]>;
  traceId?: string;
}

export interface GuestContext {
  bookingId?: number;
  roomId?: number;
  roomNumber?: string;
  checkInDate?: string;
  checkOutDate?: string;
  bookingStatus?: string;
}

interface ConversationHistory {
  messages: Array<{ role: 'user' | 'assistant' | 'system'; content: string; timestamp: Date }>;
  turnNumber: number;
}

@Injectable({ providedIn: 'root' })
export class ConciergeApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.baseUrl}/concierge`;
  private readonly storageKey = 'concierge_conversations';

  // Idempotency key tracking
  private conversationTurns = new Map<string, number>();

  chat(request: ConciergeChatRequest): Observable<ConciergeChatResponse> {
    const conversationId = request.conversationId ?? this.generateConversationId();
    const turnNumber = this.getNextTurnNumber(conversationId);
    const idempotencyKey = `concierge:turn:${conversationId}:${turnNumber}`;

    const headers = new HttpHeaders({
      'X-Idempotency-Key': idempotencyKey
    });

    return this.http.post<ConciergeChatResponse>(`${this.baseUrl}/chat`, request, { headers });
  }

  confirm(request: ConciergeConfirmRequest): Observable<ConciergeChatResponse> {
    const conversationId = request.conversationId;
    const turnNumber = this.getNextTurnNumber(conversationId);
    const idempotencyKey = `concierge:confirm:${conversationId}:${turnNumber}`;

    const headers = new HttpHeaders({
      'X-Idempotency-Key': idempotencyKey
    });

    return this.http.post<ConciergeChatResponse>(`${this.baseUrl}/confirm`, request, { headers });
  }

  getPendingProposals(conversationId: string): Observable<ConciergeProposal[]> {
    return this.http.get<ConciergeProposal[]>(`${this.baseUrl}/proposals`, {
      params: { conversationId }
    });
  }

  getContext(): Observable<GuestContext> {
    return this.http.get<GuestContext>(`${this.baseUrl}/context`);
  }

  // LocalStorage persistence
  saveConversation(conversationId: string, messages: Array<{ role: 'user' | 'assistant' | 'system'; content: string; timestamp: Date }>): void {
    try {
      const all = JSON.parse(localStorage.getItem(this.storageKey) || '{}');
      all[conversationId] = messages.slice(-20).map(m => ({
        role: m.role,
        content: m.content,
        timestamp: m.timestamp.toISOString()
      }));
      localStorage.setItem(this.storageKey, JSON.stringify(all));
    } catch (e) {
      console.warn('Failed to save conversation to localStorage', e);
    }
  }

  loadConversation(conversationId: string): Array<{ role: 'user' | 'assistant' | 'system'; content: string; timestamp: Date }> {
    try {
      const all = JSON.parse(localStorage.getItem(this.storageKey) || '{}');
      const conv = all[conversationId];
      if (!conv) return [];
      return conv.map((m: any) => ({
        role: m.role,
        content: m.content,
        timestamp: new Date(m.timestamp)
      }));
    } catch (e) {
      return [];
    }
  }

  clearConversation(conversationId: string): void {
    try {
      const all = JSON.parse(localStorage.getItem(this.storageKey) || '{}');
      delete all[conversationId];
      localStorage.setItem(this.storageKey, JSON.stringify(all));
    } catch (e) {
      console.warn('Failed to clear conversation from localStorage', e);
    }
  }

  private generateConversationId(): string {
    return crypto.randomUUID();
  }

  private getNextTurnNumber(conversationId: string): number {
    const current = this.conversationTurns.get(conversationId) || 0;
    const next = current + 1;
    this.conversationTurns.set(conversationId, next);
    return next;
  }
}

// HTTP Interceptor for idempotency keys
export const conciergeIdempotencyInterceptor: HttpInterceptorFn = (req: HttpRequest<unknown>, next: HttpHandlerFn): Observable<HttpEvent<unknown>> => {
  // Only add idempotency key for concierge endpoints that need it
  const isConciergeChat = req.url.includes('/concierge/chat');
  const isConciergeConfirm = req.url.includes('/concierge/confirm');
  
  if (!isConciergeChat && !isConciergeConfirm) {
    return next(req);
  }

  // The service already adds the header, but this ensures it's present
  if (!req.headers.has('X-Idempotency-Key')) {
    const cloned = req.clone({
      setHeaders: {
        'X-Idempotency-Key': `concierge:${Date.now()}:${Math.random().toString(36).substr(2, 9)}`
      }
    });
    return next(cloned);
  }

  return next(req);
};