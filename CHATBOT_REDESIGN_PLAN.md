# Chatbot UI Redesign - Detailed Implementation Plan

## Overview
Redesign `ConciergeChatComponent` to match the global Aetheris design system (dark theme, gold/amber accent `#e4c285`, glassmorphism, Playfair Display + Manrope typography) while preserving the existing sidepanel slide behavior in `UserShellComponent`.

---

## Global Theme Reference (from `styles/theme/`)

### Colors (`_colors.scss`)
```scss
--color-surface: #131411;
--color-surface-dim: #131411;
--color-surface-bright: #393936;
--color-surface-container-lowest: #0e0e0c;
--color-surface-container-low: #1b1c19;
--color-surface-container: #1f201d;
--color-surface-container-high: #2a2a27;
--color-surface-container-highest: #353532;
--color-on-surface: #e4e2dd;
--color-on-surface-variant: #c4c7c7;
--color-outline: #8e9192;
--color-outline-variant: #444748;
--color-primary: #c9c6c5;
--color-on-primary: #313030;
--color-primary-container: #0a0a0a;
--color-on-primary-container: #7b7979;
--color-secondary: #e4c285;        // GOLD ACCENT
--color-on-secondary: #412d00;
--color-secondary-container: #5d4514;
--color-on-secondary-container: #d5b478;
--color-background: #131411;
--color-on-background: #e4e2dd;
--color-tertiary: #c8c6c5;
--color-on-tertiary: #313030;
--color-tertiary-container: #0a0a0a;
--color-on-tertiary-container: #7a7979;
--color-error: #ffb4ab;
--color-on-error: #690005;
--color-error-container: #93000a;
--color-on-error-container: #ffdad6;
// Glassmorphism
--glass-bg: rgba(26, 26, 26, 0.7);
--glass-border: rgba(228, 194, 133, 0.2);
```

### Typography (`_typography.scss`)
```scss
--font-headline: 'Playfair Display', serif;
--font-body: 'Manrope', sans-serif;

--fs-display-lg: 72px;
--fs-display-lg-mobile: 40px;
--fs-headline-md: 34px;
--fs-headline-sm: 26px;
--fs-body-lg: 20px;
--fs-body-md: 18px;
--fs-label-caps: 14px;

--lh-display-lg: 1.1;
--lh-display-lg-mobile: 1.2;
--lh-headline-md: 1.3;
--lh-headline-sm: 1.4;
--lh-body-lg: 1.6;
--lh-body-md: 1.6;
--lh-label-caps: 1.0;

--ls-display-lg: -0.02em;
--ls-display-lg-mobile: -0.01em;
--ls-body-lg: 0.02em;
--ls-body-md: 0.01em;
--ls-label-caps: 0.2em;
```

### Mixins
```scss
// _glassmorphism.scss
@mixin glass-panel {
  background: var(--glass-bg);
  backdrop-filter: blur(20px);
  -webkit-backdrop-filter: blur(20px);
  border: 1px solid var(--glass-border);
}

// _mixins.scss
@mixin gold-underline {
  position: relative;
  padding-bottom: 4px;
  &::after {
    content: '';
    position: absolute;
    bottom: 0;
    left: 0;
    width: 100%;
    height: 1px;
    background-color: var(--color-secondary);
    transition: width 0.5s ease;
  }
  &:hover::after { width: 0%; }
}
```

### Font Mixins
```scss
@mixin font-display-lg { ... }
@mixin font-display-lg-mobile { ... }
@mixin font-headline-md { ... }
@mixin font-headline-sm { ... }
@mixin font-body-lg { ... }
@mixin font-body-md { ... }
@mixin font-label-caps { ... }
```

---

## Current Chatbot Analysis

### Component Structure
- **File**: `Frontend/src/app/features/user/components/concierge-chat/concierge-chat.component.ts`
- **Template**: 138 lines with heavy inline styles
- **Styles**: 156 lines SCSS using Material System color tokens

### Key UI Sections
1. **Header** - Title "AI Concierge" + clear button
2. **Context Bar** - Guest context chips (booking, room, status)
3. **Messages Area** - Scrollable list with bubbles
4. **Quick Actions** - 6 action buttons
5. **Input Area** - Form field + send button / loading state

### Current Color Usage (to replace)
| Element | Current Token | New Token |
|---------|---------------|-----------|
| Panel bg | implicit | `@include glass-panel` |
| Header border | `var(--mat-sys-outline-variant)` | `var(--glass-border)` |
| Header text | `var(--mat-sys-on-surface)` | `var(--color-on-surface)` |
| Context bar | `mat-card` outlined | glassmorphism card |
| User bubble bg | `var(--mat-sys-primary)` | `var(--color-secondary)` |
| User bubble text | `var(--mat-sys-on-primary)` | `var(--color-on-secondary)` |
| Assistant bubble bg | `var(--mat-sys-surface-variant)` | `var(--color-surface-container)` |
| Assistant bubble text | `var(--mat-sys-on-surface-variant)` | `var(--color-on-surface)` |
| Quick action btn | `mat-stroked-button` | glassmorphism button |
| Input field | `mat-form-field` outline | glassmorphism panel + gold focus |
| Send button | `mat-icon-button` | gold filled button |
| Proposals panel | `var(--mat-sys-surface-container)` | glassmorphism card |
| Confirm button | `mat-flat-button color="primary"` | gold filled button |
| Countdown | `var(--mat-sys-on-surface-variant)` | `var(--color-secondary)` |
| Typing dots | `currentColor` | `var(--color-secondary)` |
| Scrollbar | hidden | gold thumb |

---

## Implementation Steps

### Step 1: Rewrite SCSS (`concierge-chat.component.scss`)

**Complete replacement** - Delete all existing content, write new theme-aligned styles.

```scss
@use '../../../../../../styles/theme/index' as *;

:host {
  display: flex;
  flex-direction: column;
  height: 100%;
  background: var(--color-surface-container);
  @include glass-panel;
  border-radius: 16px 0 0 16px;
  
  // Ensure text color inheritance
  color: var(--color-on-surface);
  font-family: var(--font-body);
}

// ============================================================================
// HEADER
// ============================================================================
.chat-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px 20px;
  border-bottom: 1px solid var(--glass-border);
  background: var(--color-surface-container-low);
  position: sticky;
  top: 0;
  z-index: 1;
  border-radius: 16px 0 0 0;

  .chat-title {
    @include font-headline-sm;
    color: var(--color-secondary);
    margin: 0;
    font-weight: 500;
    letter-spacing: 0.02em;
  }

  .chat-close {
    background: none;
    border: none;
    color: var(--color-on-surface-variant);
    cursor: pointer;
    padding: 8px;
    display: flex;
    align-items: center;
    justify-content: center;
    border-radius: 50%;
    transition: background 0.2s, color 0.2s;

    &:hover {
      background: var(--color-surface-container-high);
      color: var(--color-on-surface);
    }

    mat-icon {
      font-size: 24px;
    }
  }
}

// ============================================================================
// CONTEXT BAR
// ============================================================================
.context-bar {
  margin: 16px;
  border-radius: 4px;
  @include glass-panel;
  
  mat-chip-set {
    gap: 8px;
  }
  
  mat-chip {
    @include font-label-caps;
    font-size: 11px;
    height: 28px;
    border-radius: 12px;
    
    &.mat-mdc-chip {
      --mdc-chip-container-color: var(--color-surface-container-high);
      --mdc-chip-label-text-color: var(--color-on-surface-variant);
      --mdc-chip-icon-color: var(--color-secondary);
      border: 1px solid var(--glass-border);
    }
    
    &.primary-chip {
      --mdc-chip-container-color: var(--color-secondary-container);
      --mdc-chip-label-text-color: var(--color-on-secondary-container);
      --mdc-chip-icon-color: var(--color-on-secondary-container);
      border-color: var(--color-secondary);
    }
  }
}

// ============================================================================
// MESSAGES AREA
// ============================================================================
.messages {
  flex: 1;
  overflow-y: auto;
  padding: 16px 20px;
  display: flex;
  flex-direction: column;
  gap: 16px;
  
  // Custom scrollbar with gold accent
  &::-webkit-scrollbar {
    width: 6px;
  }
  &::-webkit-scrollbar-track {
    background: transparent;
  }
  &::-webkit-scrollbar-thumb {
    background: var(--color-secondary);
    border-radius: 3px;
    opacity: 0.6;
  }
  &::-webkit-scrollbar-thumb:hover {
    opacity: 1;
  }
}

.message {
  max-width: 85%;
  display: flex;
  flex-direction: column;
  gap: 6px;
  animation: messageIn 0.3s cubic-bezier(0.4, 0, 0.2, 1);

  @keyframes messageIn {
    from { opacity: 0; transform: translateY(8px); }
    to { opacity: 1; transform: translateY(0); }
  }

  &.message-user {
    align-self: flex-end;
  }

  &.message-assistant {
    align-self: flex-start;
  }

  &.message-system {
    align-self: center;
    max-width: 100%;
  }
}

.bubble {
  padding: 12px 16px;
  border-radius: 16px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.2);
  line-height: 1.5;

  p {
    margin: 0;
    white-space: pre-wrap;
    word-wrap: break-word;
  }

  // User message - gold background
  &.message-user .bubble {
    background: var(--color-secondary);
    color: var(--color-on-secondary);
    border-bottom-right-radius: 4px;
    box-shadow: 0 2px 8px rgba(228, 194, 133, 0.3);
  }

  // Assistant message - surface container with gold accent border
  &.message-assistant .bubble {
    background: var(--color-surface-container);
    color: var(--color-on-surface);
    border: 1px solid var(--glass-border);
    border-bottom-left-radius: 4px;
    
    &:hover {
      border-color: var(--color-secondary);
    }
  }

  // System message - subtle
  &.message-system .bubble {
    background: var(--color-surface-container-low);
    color: var(--color-on-surface-variant);
    font-size: 13px;
    font-style: italic;
    border: 1px solid var(--glass-border);
    border-radius: 8px;
    padding: 8px 12px;
  }
}

// ============================================================================
// PROPOSALS PANEL
// ============================================================================
.proposals {
  margin-top: 12px;
  padding: 16px;
  border-radius: 12px;
  @include glass-panel;
  border: 1px solid var(--color-secondary);
  box-shadow: 0 4px 16px rgba(228, 194, 133, 0.15);

  .proposals-title {
    @include font-label-caps;
    color: var(--color-secondary);
    margin-bottom: 12px;
    font-size: 12px;
    letter-spacing: 0.1em;
  }

  .proposal-item {
    display: flex;
    align-items: center;
    gap: 12px;
    margin-top: 10px;
    padding: 12px;
    background: var(--color-surface-container-low);
    border-radius: 8px;
    border: 1px solid var(--glass-border);
    transition: border-color 0.2s, box-shadow 0.2s;

    &:hover {
      border-color: var(--color-secondary);
      box-shadow: 0 2px 8px rgba(228, 194, 133, 0.1);
    }

    mat-icon {
      color: var(--color-secondary);
      font-size: 20px;
      width: 20px;
      height: 20px;
      flex-shrink: 0;
    }

    .proposal-summary {
      flex: 1;
      color: var(--color-on-surface);
      font-size: 14px;
      line-height: 1.4;
    }

    .proposal-countdown {
      margin-left: auto;
      @include font-label-caps;
      font-size: 11px;
      color: var(--color-secondary);
      font-variant-numeric: tabular-nums;
      background: var(--color-secondary-container);
      padding: 4px 8px;
      border-radius: 4px;
    }

    .dismiss-btn {
      margin-left: 8px;
      color: var(--color-on-surface-variant);
      transition: color 0.2s;

      &:hover {
        color: var(--color-error);
      }

      mat-icon {
        font-size: 18px;
        width: 18px;
        height: 18px;
      }
    }
  }

  .proposals-actions {
    margin-top: 16px;
    display: flex;
    gap: 12px;
  }

  .confirm-btn {
    flex: 1;
    height: 44px;
    border: none;
    border-radius: 4px;
    background: var(--color-secondary);
    color: var(--color-on-secondary);
    @include font-label-caps;
    font-size: 13px;
    font-weight: 600;
    cursor: pointer;
    transition: background-color 0.2s, transform 0.1s, box-shadow 0.2s;
    box-shadow: 0 2px 8px rgba(228, 194, 133, 0.3);

    &:hover:not(:disabled) {
      background: #ffdea4;
      transform: translateY(-1px);
      box-shadow: 0 4px 16px rgba(228, 194, 133, 0.4);
    }

    &:active:not(:disabled) {
      transform: translateY(0);
    }

    &:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }
  }
}

.proposal-status {
  margin-top: 14px;
  display: flex;
  align-items: center;
  gap: 8px;
  @include font-label-caps;
  font-size: 12px;

  &.cancelled {
    color: var(--color-error);
    
    mat-icon {
      color: var(--color-error);
    }
  }

  &.confirmed {
    color: var(--color-secondary);
    
    mat-icon {
      color: var(--color-secondary);
    }
  }

  mat-icon {
    font-size: 16px;
    width: 16px;
    height: 16px;
  }
}

// ============================================================================
// TIMESTAMP
// ============================================================================
.timestamp {
  @include font-label-caps;
  font-size: 10px;
  color: var(--color-on-surface-variant);
  padding: 0 4px;
  opacity: 0.7;
}

// ============================================================================
// QUICK ACTIONS
// ============================================================================
.quick-actions {
  padding: 16px 20px;
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
  border-top: 1px solid var(--glass-border);
  background: var(--color-surface-container-low);

  button {
    @include font-label-caps;
    font-size: 12px;
    height: 36px;
    padding: 0 16px;
    border-radius: 4px;
    border: 1px solid var(--glass-border);
    background: var(--color-surface-container);
    color: var(--color-on-surface);
    cursor: pointer;
    transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
    @include gold-underline;
    white-space: nowrap;

    &:hover:not(:disabled) {
      border-color: var(--color-secondary);
      background: var(--color-surface-container-high);
      color: var(--color-secondary);
    }

    &:disabled {
      opacity: 0.4;
      cursor: not-allowed;
    }

    &:focus-visible {
      outline: 2px solid var(--color-secondary);
      outline-offset: 2px;
    }
  }
}

// ============================================================================
// INPUT AREA
// ============================================================================
.input-area {
  padding: 16px 20px;
  border-top: 1px solid var(--glass-border);
  background: var(--color-surface-container-low);

  .input-wrapper {
    display: flex;
    align-items: flex-end;
    gap: 12px;
    background: var(--color-surface-container);
    border: 1px solid var(--glass-border);
    border-radius: 4px;
    padding: 8px 12px;
    transition: border-color 0.2s, box-shadow 0.2s;
    @include glass-panel;

    &:focus-within {
      border-color: var(--color-secondary);
      box-shadow: 0 0 0 2px rgba(228, 194, 133, 0.15);
    }
  }

  mat-form-field {
    flex: 1;
    width: 100%;
    
    // Override Material form field to use our theme
    --mdc-outlined-text-field-label-text-color: var(--color-on-surface-variant) !important;
    --mdc-outlined-text-field-focus-label-text-color: var(--color-secondary) !important;
    --mdc-outlined-text-field-hover-label-text-color: var(--color-secondary) !important;
    
    --mdc-outlined-text-field-outline-color: var(--glass-border) !important;
    --mdc-outlined-text-field-focus-outline-color: var(--color-secondary) !important;
    --mdc-outlined-text-field-hover-outline-color: var(--color-secondary) !important;
    
    --mdc-outlined-text-field-input-text-color: var(--color-on-surface) !important;
    --mdc-outlined-text-field-input-text-placeholder-color: var(--color-on-surface-variant) !important;

    .mat-mdc-text-field-wrapper {
      background-color: transparent !important;
    }

    .mdc-floating-label {
      color: var(--color-on-surface-variant) !important;
      font-family: var(--font-body) !important;
      @include font-body-md;
    }

    &.mat-focused .mdc-floating-label {
      color: var(--color-secondary) !important;
    }

    input.mat-mdc-input-element {
      color: var(--color-on-surface) !important;
      font-family: var(--font-body) !important;
      @include font-body-md;
      padding: 8px 0 !important;

      &::placeholder {
        color: var(--color-on-surface-variant) !important;
        opacity: 0.5;
      }
    }
  }

  .send-btn {
    flex-shrink: 0;
    width: 44px;
    height: 44px;
    border: none;
    border-radius: 50%;
    background: var(--color-secondary);
    color: var(--color-on-secondary);
    display: flex;
    align-items: center;
    justify-content: center;
    cursor: pointer;
    transition: background-color 0.2s, transform 0.1s, box-shadow 0.2s;
    box-shadow: 0 2px 8px rgba(228, 194, 133, 0.3);

    &:hover:not(:disabled) {
      background: #ffdea4;
      transform: scale(1.05);
      box-shadow: 0 4px 16px rgba(228, 194, 133, 0.4);
    }

    &:active:not(:disabled) {
      transform: scale(1);
    }

    &:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }

    &:focus-visible {
      outline: 2px solid var(--color-secondary);
      outline-offset: 2px;
    }

    mat-icon {
      font-size: 20px;
      width: 20px;
      height: 20px;
    }
  }

  .loading-state {
    display: flex;
    align-items: center;
    gap: 12px;
    padding: 0 16px;
    height: 56px;
    color: var(--color-on-surface-variant);
    @include font-body-md;
  }
}

// ============================================================================
// TYPING INDICATOR
// ============================================================================
.typing-indicator {
  display: flex;
  align-items: center;
  gap: 4px;
  height: 24px;
  padding: 4px 8px;

  .dot {
    width: 8px;
    height: 8px;
    background-color: var(--color-secondary);
    border-radius: 50%;
    opacity: 0.6;
    animation: pulse 1.4s infinite ease-in-out both;

    &:nth-child(1) { animation-delay: -0.32s; }
    &:nth-child(2) { animation-delay: -0.16s; }
  }
}

@keyframes pulse {
  0%, 80%, 100% {
    transform: scale(0.6);
    opacity: 0.4;
  }
  40% {
    transform: scale(1.1);
    opacity: 1;
  }
}

// ============================================================================
// THINKING MESSAGE (Assistant loading)
// ============================================================================
.message.thinking {
  opacity: 0.7;
  
  .bubble {
    background: var(--color-surface-container);
    border-color: var(--glass-border);
  }
}

// ============================================================================
// RESPONSIVE ADJUSTMENTS
// ============================================================================
@media (max-width: 480px) {
  :host {
    border-radius: 0;
  }
  
  .chat-header {
    border-radius: 0;
    padding: 12px 16px;
  }

  .messages {
    padding: 12px 16px;
    gap: 12px;
  }

  .message {
    max-width: 90%;
  }

  .quick-actions {
    padding: 12px 16px;
    gap: 8px;
    
    button {
      font-size: 11px;
      height: 32px;
      padding: 0 12px;
    }
  }

  .input-area {
    padding: 12px 16px;
    
    .send-btn {
      width: 40px;
      height: 40px;
    }
  }

  .context-bar {
    margin: 12px 16px;
  }

  .proposals {
    margin-top: 10px;
    padding: 12px;
  }
}
```

---

### Step 2: Update Template (`concierge-chat.component.html`)

**Replace all inline styles with CSS classes. Keep structure identical.**

```html
<div class="concierge-chat" role="log" aria-live="polite" aria-label="Concierge conversation">
  <!-- Header -->
  <header class="chat-header">
    <h1 class="chat-title">AI Concierge</h1>
    <button mat-icon-button 
            (click)="clearChat()" 
            [disabled]="loading()" 
            title="Clear chat history" 
            aria-label="Clear chat history"
            class="chat-close">
      <mat-icon>delete_sweep</mat-icon>
    </button>
  </header>

  <!-- Context Bar -->
  @if (context(); as ctx) {
    <mat-card class="context-bar" appearance="outlined">
      <mat-chip-set aria-label="Guest context">
        @if (ctx.bookingId) {
          <mat-chip>Booking #{{ ctx.bookingId }}</mat-chip>
        }
        @if (ctx.roomNumber) {
          <mat-chip>Room {{ ctx.roomNumber }}</mat-chip>
        }
        @if (ctx.bookingStatus) {
          <mat-chip [class.primary-chip]="ctx.bookingStatus === 'CheckedIn'">
            {{ ctx.bookingStatus }}
          </mat-chip>
        }
      </mat-chip-set>
    </mat-card>
  }

  <!-- Messages -->
  <div #messagesContainer class="messages">
    @for (msg of messages(); track $index) {
      <div class="message" 
           [class.message-user]="msg.role === 'user'"
           [class.message-assistant]="msg.role === 'assistant'"
           [class.message-system]="msg.role === 'system'"
           [class.thinking]="loading() && $index === messages().length - 1"
           role="article" 
           [attr.aria-label]="msg.role + ' message'">
        <div class="bubble">
          <p>{{ msg.content }}</p>

          <!-- Proposals awaiting confirmation -->
          @if (msg.proposals?.length) {
            <div class="proposals">
              <div class="proposals-title">Proposed actions:</div>
              @for (prop of msg.proposals; track prop.proposalId) {
                <div class="proposal-item">
                  <mat-icon color="primary">info</mat-icon>
                  <span class="proposal-summary">{{ prop.summary }}</span>
                  @if (!msg.proposalStatus || msg.proposalStatus === 'pending') {
                    <span class="proposal-countdown">
                      {{ formatTimeRemaining(getTimeRemaining(prop)) }}
                    </span>
                    <button mat-icon-button 
                            (click)="dismissProposal(prop.proposalId)" 
                            [disabled]="loading()"
                            aria-label="Dismiss proposal"
                            class="dismiss-btn">
                      <mat-icon>close</mat-icon>
                    </button>
                  }
                </div>
              }
              
              @if (!msg.proposalStatus || msg.proposalStatus === 'pending') {
                <div class="proposals-actions">
                  <button mat-button 
                          (click)="confirmProposals()"
                          [disabled]="loading()"
                          aria-label="Confirm and execute proposed actions"
                          class="confirm-btn">
                    Confirm & Execute
                  </button>
                </div>
              }

              @if (msg.proposalStatus === 'cancelled') {
                <div class="proposal-status cancelled">
                  <mat-icon>cancel</mat-icon>
                  <span>Proposal cancelled</span>
                </div>
              }

              @if (msg.proposalStatus === 'confirmed') {
                <div class="proposal-status confirmed">
                  <mat-icon>check_circle</mat-icon>
                  <span>Proposal confirmed & executed</span>
                </div>
              }
            </div>
          }

          <!-- Actions (completed) -->
          @if (msg.actions?.length) {
            <div class="actions">
              @for (action of msg.actions; track $index) {
                <mat-chip color="primary" class="action-chip">{{ action.message }}</mat-chip>
              }
            </div>
          }
        </div>
        <div class="timestamp">
          {{ msg.timestamp | date:'shortTime' }}
        </div>
      </div>
    }

    <!-- Thinking indicator -->
    @if (loading()) {
      <div class="message message-assistant thinking">
        <div class="bubble">
          <div class="typing-indicator" aria-label="Thinking">
            <div class="dot"></div>
            <div class="dot"></div>
            <div class="dot"></div>
          </div>
        </div>
      </div>
    }
  </div>

  <!-- Quick Actions -->
  <div class="quick-actions">
    @for (action of quickActions; track action.label) {
      <button (click)="useQuickAction(action.prompt)" 
              [disabled]="loading() || pendingProposals().length > 0"
              aria-label="Quick action: {{ action.label }}">
        {{ action.label }}
      </button>
    }
  </div>

  <!-- Input Area -->
  <div class="input-area">
    <div class="input-wrapper">
      <mat-form-field appearance="outline" class="hidden" [class.hidden]="loading()">
        <mat-label>Ask me anything...</mat-label>
        <input matInput 
               [formControl]="messageControl" 
               (keydown.enter)="sendMessage()"
               placeholder="e.g., 'I'd like a burger and extra towels'"
               aria-label="Your message to the concierge">
        <button mat-icon-button matSuffix (click)="sendMessage()" 
                [disabled]="messageControl.invalid || loading() || pendingProposals().length > 0"
                aria-label="Send message"
                class="send-btn">
          <mat-icon>send</mat-icon>
        </button>
      </mat-form-field>
    </div>
    
    @if (loading()) {
      <div class="loading-state">
        <div class="typing-indicator" aria-hidden="true">
          <div class="dot"></div>
          <div class="dot"></div>
          <div class="dot"></div>
        </div>
        <span>Thinking...</span>
      </div>
    }
  </div>
</div>
```

---

### Step 3: Minor TypeScript Updates (`concierge-chat.component.ts`)

Only if needed for styling access:

```typescript
// Add at component decorator if global theme vars not reaching component
@Component({
  // ... existing config
  encapsulation: ViewEncapsulation.None,  // Only if needed
  // ...
})
```

**No logic changes** - preserve all existing functionality.

---

## Verification Checklist

### Visual Verification (All Breakpoints)
- [ ] Desktop (400px panel): All elements fit, no overflow
- [ ] Tablet (90vw, max 480px): Responsive layout works
- [ ] Mobile (100%): Full-width, touch-friendly targets

### Theme Compliance
- [ ] Header: Playfair Display, gold title, glassmorphism bg
- [ ] Context chips: glassmorphism, gold for active status
- [ ] User bubbles: gold bg (`--color-secondary`), dark text
- [ ] Assistant bubbles: surface-container, gold border on hover
- [ ] Quick actions: glassmorphism buttons, gold underline hover
- [ ] Input: glassmorphism panel, gold focus ring
- [ ] Send button: gold circle, dark icon, hover scale
- [ ] Proposals: gold border, gold confirm button, gold countdown
- [ ] Typing dots: gold color, pulse animation
- [ ] Scrollbar: gold thumb

### Accessibility
- [ ] ARIA live region on messages container
- [ ] ARIA labels on all interactive elements
- [ ] Focus visible states (gold outline)
- [ ] Keyboard navigation works
- [ ] Color contrast meets WCAG AA

### Functionality
- [ ] Send message works
- [ ] Quick actions work
- [ ] Proposals confirm/dismiss works
- [ ] Clear chat works
- [ ] Conversation persistence works
- [ ] Loading states show correctly
- [ ] Error toasts appear

### Code Quality
- [ ] No inline styles in template
- [ ] All colors use CSS variables
- [ ] SCSS uses `@use` for theme imports
- [ ] No Material System color tokens remain
- [ ] Build passes (`ng build`)
- [ ] Lint passes (`ng lint`)

---

## Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Material form field overrides don't apply | Medium | High | Test focus states; use `!important` if needed |
| Glassmorphism backdrop-filter not supported | Low | Low | Fallback solid bg in `@supports` |
| Sidepanel width constraints break layout | Low | Medium | Test at 400px, 360px, 320px |
| Global theme vars not available in component | Low | High | Add `encapsulation: ViewEncapsulation.None` if needed |
| Animation performance on mobile | Low | Medium | Use `transform`/`opacity` only; test on device |

---

## Dependencies

**No new dependencies.** Uses only:
- Existing Angular Material modules (already imported)
- Global theme SCSS (already available)
- Existing component logic (unchanged)

---

## Rollback Plan

If issues arise:
1. `git checkout -- Frontend/src/app/features/user/components/concierge-chat/`
2. Original component fully restored

---

## Estimated Effort

- **SCSS rewrite**: ~2 hours
- **Template cleanup**: ~1 hour
- **Testing & verification**: ~1 hour
- **Total**: ~4 hours

---

## Next Steps

1. Implement SCSS rewrite (Step 1)
2. Update template (Step 2)
3. Run build/lint
4. Manual testing in browser at all breakpoints
5. Commit with message: "feat(concierge-chat): redesign UI to match global theme"