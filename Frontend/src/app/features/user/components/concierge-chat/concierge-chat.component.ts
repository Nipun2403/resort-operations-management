import { Component, inject, signal, OnInit, OnDestroy, AfterViewInit, DestroyRef, ViewChild, ElementRef, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl, Validators } from '@angular/forms';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs/operators';

import {
  ConciergeApiService, ConciergeChatRequest, ConciergeChatResponse,
  ConciergeProposal, ConciergeActionResult, GuestContext, PersistedChatMessage
} from '../../services/concierge-api.service';
import { AuthService } from '../../../../core/services/auth.service';

interface ChatMessage {
  role: 'user' | 'assistant' | 'system';
  content: string;
  proposals?: ConciergeProposal[];
  actions?: ConciergeActionResult[];
  proposalStatus?: 'pending' | 'confirmed' | 'cancelled';
  timestamp: Date;
}

@Component({
  selector: 'app-concierge-chat',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule,
    MatInputModule, MatButtonModule, MatIconModule,
    MatProgressSpinnerModule, MatCardModule, MatChipsModule,
    MatSnackBarModule
  ],
  templateUrl: './concierge-chat.component.html',
  styleUrls: ['./concierge-chat.component.scss']
})
export class ConciergeChatComponent implements OnInit, OnDestroy, AfterViewInit {
  private readonly api = inject(ConciergeApiService);
  private readonly auth = inject(AuthService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly snackBar = inject(MatSnackBar);

  @ViewChild('messagesContainer') messagesContainer!: ElementRef<HTMLDivElement>;
  @Output() closeChat = new EventEmitter<void>();

  messages = signal<ChatMessage[]>([]);
  conversationId = signal<string | null>(null);
  pendingProposals = signal<ConciergeProposal[]>([]);
  loading = signal(false);
  context = signal<GuestContext | null>(null);

  messageControl = new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(1000)] });

  quickActions = [
    { label: 'Order Food', prompt: 'I\'d like to order a burger and fries' },
    { label: 'Extra Pillows', prompt: 'Can I get extra pillows and blankets?' },
    { label: 'Report Issue', prompt: 'There\'s a maintenance issue in my room' },
    { label: 'Check Bill', prompt: 'What\'s my current folio balance?' },
    { label: 'Check-out Time', prompt: 'What time is check-out?' },
    { label: 'Room Status', prompt: 'Has my room been cleaned yet?' }
  ];

  private countdownTimer: ReturnType<typeof setInterval> | null = null;
  private readonly CHAT_TIMEOUT_MINUTES = 1;

  ngOnInit(): void {
    this.loadContext();
    this.restoreConversation();
    this.addWelcomeMessage();
    this.startCountdownTimer();
  }

  ngOnDestroy(): void {
    if (this.countdownTimer) {
      clearInterval(this.countdownTimer);
    }
  }

  ngAfterViewInit(): void {
    this.scrollToBottom();
  }

  private loadContext(): void {
    this.api.getContext().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (ctx) => this.context.set(ctx)
    });
  }

  private restoreConversation(): void {
    const savedConvId = localStorage.getItem('concierge_conversation_id');
    if (savedConvId) {
      this.conversationId.set(savedConvId);
      const savedMessages = this.api.loadConversation(savedConvId);
      if (savedMessages.length > 0) {
        this.messages.set(savedMessages.map(m => ({
          role: m.role,
          content: m.content,
          proposals: m.proposals,
          proposalStatus: m.proposalStatus,
          timestamp: m.timestamp
        })));

        // Restore pending proposals from history if present
        const pendingMsg = savedMessages.slice().reverse().find(m => m.proposalStatus === 'pending');
        if (pendingMsg && pendingMsg.proposals) {
          this.pendingProposals.set(pendingMsg.proposals);
        }
      }
    }
  }

  private addWelcomeMessage(): void {
    const name = this.auth.fullName() || 'there';
    const welcomeText = `Hello ${name}! 👋 I'm ATLAS. I can help with room service, housekeeping, maintenance, billing questions, and more. What can I do for you today?`;
    const welcomeMsg: ChatMessage = {
      role: 'assistant',
      content: welcomeText,
      timestamp: new Date()
    };

    const currentMessages = this.messages();
    if (currentMessages.length === 0) {
      this.messages.set([welcomeMsg]);
      return;
    }

    // Check if we should append a new welcome message
    const lastMsg = currentMessages[currentMessages.length - 1];
    const lastMessageTime = lastMsg.timestamp.getTime();
    const elapsedMinutes = (Date.now() - lastMessageTime) / (1000 * 60);

    const hasPendingProposals = this.pendingProposals().length > 0;

    // Robust check to see if the last message was a welcome message
    const isLastMessageWelcome = lastMsg.content.includes("I'm ATLAS.") ||
      lastMsg.content.includes("I'm your AI Concierge");

    // Timeout triggered, no active proposals, and last message is not already a welcome message
    if (elapsedMinutes >= this.CHAT_TIMEOUT_MINUTES && !hasPendingProposals && !isLastMessageWelcome) {
      this.messages.update(msgs => [...msgs, welcomeMsg]);
      this.saveConversation();
    }
  }

  private startCountdownTimer(): void {
    this.countdownTimer = setInterval(() => {
      this.pendingProposals.update(proposals =>
        proposals.map(p => ({ ...p, expiresAt: p.expiresAt }))
      );
    }, 1000);
  }

  sendMessage(): void {
    if (this.messageControl.invalid || this.loading() || this.pendingProposals().length > 0) return;

    const userMessage = this.messageControl.value;
    this.messageControl.reset();
    this.loading.set(true);

    const userMsg: ChatMessage = { role: 'user', content: userMessage, timestamp: new Date() };
    this.messages.update(msgs => [...msgs, userMsg]);
    this.saveConversation();
    this.scrollToBottom();

    const request: ConciergeChatRequest = {
      message: userMessage,
      conversationId: this.conversationId() || undefined
    };

    this.api.chat(request).pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.loading.set(false))
    ).subscribe({
      next: (response) => this.handleResponse(response),
      error: (err) => this.handleError(err)
    });
  }

  confirmProposals(proposalId?: string): void {
    const idsToConfirm = proposalId ? [proposalId] : this.pendingProposals().map(p => p.proposalId);
    if (idsToConfirm.length === 0 || this.loading()) return;

    this.loading.set(true);

    this.api.confirm({
      conversationId: this.conversationId()!,
      proposalIds: idsToConfirm
    }).pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.loading.set(false))
    ).subscribe({
      next: (response) => {
        this.pendingProposals.set([]);

        this.messages.update(msgs => msgs.map(msg => {
          if (msg.proposals?.some(p => idsToConfirm.includes(p.proposalId))) {
            return {
              ...msg,
              proposalStatus: 'confirmed'
            };
          }
          return msg;
        }));

        this.handleResponse(response);
      },
      error: (err) => this.handleError(err)
    });
  }

  dismissProposal(proposalId: string): void {
    this.pendingProposals.update(p => p.filter(p => p.proposalId !== proposalId));

    this.messages.update(msgs => msgs.map(msg => {
      if (msg.proposals?.some(p => p.proposalId === proposalId)) {
        return {
          ...msg,
          proposalStatus: 'cancelled'
        };
      }
      return msg;
    }));

    this.saveConversation();
    this.showToast('Proposal dismissed');
  }

  getTimeRemaining(proposal: ConciergeProposal): number {
    const expiresAt = new Date(proposal.expiresAt).getTime();
    const now = Date.now();
    return Math.max(0, Math.ceil((expiresAt - now) / 1000));
  }

  formatTimeRemaining(seconds: number): string {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins}:${secs.toString().padStart(2, '0')}`;
  }

  private handleResponse(response: ConciergeChatResponse): void {
    this.conversationId.set(response.conversationId);
    localStorage.setItem('concierge_conversation_id', response.conversationId);

    if (response.proposals.length > 0) {
      this.pendingProposals.set(response.proposals);

      this.messages.update(msgs => [...msgs, {
        role: 'system',
        content: 'Proposals ready for confirmation:',
        timestamp: new Date()
      }]);
    }

    this.messages.update(msgs => [...msgs, {
      role: 'assistant',
      content: response.reply,
      proposals: response.proposals.length > 0 ? response.proposals : undefined,
      proposalStatus: response.proposals.length > 0 ? 'pending' : undefined,
      actions: response.actions.length > 0 ? response.actions : undefined,
      timestamp: new Date()
    }]);

    this.saveConversation();
    this.scrollToBottom();
  }

  private handleError(err: any): void {
    const errorResponse = err.error as { errorCode?: string; message?: string; details?: Record<string, string[]> } | undefined;
    let msg = 'Something went wrong. Please try again.';

    if (errorResponse?.errorCode === 'VALIDATION_ERROR') {
      msg = errorResponse.message || 'Invalid request. Please check your input.';
    } else if (errorResponse?.errorCode === 'PROPOSAL_EXPIRED') {
      msg = 'One or more proposals have expired. Please try again.';
    } else if (errorResponse?.errorCode === 'PROPOSAL_NOT_FOUND') {
      msg = 'Proposal not found. Please try again.';
    } else if (err.status === 429) {
      msg = 'Too many requests. Please wait a moment and try again.';
    } else if (err.status === 401) {
      msg = 'Your session has expired. Please log in again.';
    } else if (err.message) {
      msg = err.message;
    }

    this.showToast(msg, 'error');
    this.messages.update(msgs => [...msgs, {
      role: 'assistant',
      content: `I'm sorry — ${msg}`,
      timestamp: new Date()
    }]);
    this.scrollToBottom();
  }

  private saveConversation(): void {
    const convId = this.conversationId();
    if (!convId) return;

    const messagesToSave = this.messages().slice(-20).map(m => ({
      role: m.role,
      content: m.content,
      proposals: m.proposals,
      proposalStatus: m.proposalStatus,
      timestamp: m.timestamp
    }));

    this.api.saveConversation(convId, messagesToSave);
  }

  clearChat(): void {
    const convId = this.conversationId();
    if (convId) {
      this.api.clearConversation(convId);
    }
    localStorage.removeItem('concierge_conversation_id');
    this.conversationId.set(null);
    this.pendingProposals.set([]);
    this.messages.set([]);
    this.addWelcomeMessage();
    this.showToast('Chat history cleared', 'info');
  }

  useQuickAction(prompt: string): void {
    this.messageControl.setValue(prompt);
    this.sendMessage();
  }

  private showToast(message: string, type: 'error' | 'success' | 'info' = 'info'): void {
    this.snackBar.open(message, 'Dismiss', {
      duration: 5000,
      panelClass: [`${type}-snackbar`],
      horizontalPosition: 'right',
      verticalPosition: 'top'
    });
  }

  private scrollToBottom(): void {
    setTimeout(() => {
      const el = this.messagesContainer?.nativeElement;
      if (el) el.scrollTop = el.scrollHeight;
    }, 0);
  }
}