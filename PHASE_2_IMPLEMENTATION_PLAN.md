# AI Concierge — Phase 2 (P1) Implementation Plan

**Global Feature Scope & Connections**

This Phase 2 plan builds directly on Phase 1 (P0: Core Engine) which implemented:
- Two-step tool pattern (propose → confirm) via `POST /api/v1/concierge/chat` + `POST /api/v1/concierge/confirm`
- PostgreSQL-backed conversation store (`conversation_messages` table) with user-scoped keys `concierge:conv:{userId}:{conversationId}`
- Proposal store (`concierge_proposals` table) with 5-min TTL
- `ConciergeActionLog` audit trail table
- `ConciergeService` orchestrating OpenAI function calling (8 tools: 3 side-effect, 5 read-only)
- `ConciergeController` with `[Idempotent]` attribute on mutations
- Frontend `ConciergeChatComponent` with confirmation gate, quick actions, context bar
- SignalR notification pipeline: BLL services → `INotificationService` → `SignalRNotificationService` → `NotificationHub` → Frontend `NotificationService` → snackbars

**All Phase 2 work MUST:**
- Not modify Phase 1 contracts (DTOs, API routes, DB schema)
- Reuse existing `INotificationService` / `SignalRNotificationService` / `NotificationHub`
- Follow existing patterns: scoped DI, `ICurrentUserService` for auth, `IMapper` for DTOs
- Use Serena for symbol-level edits; GitNexus `impact` before any modification
- Commit after each logical step with descriptive messages

---

## Phase 2 Scope (P1: API Hardening + Background Jobs + Frontend Polish)

### 2.1 API Hardening & Rate Limiting (Backend)

#### 2.1.1 Per-User Rate Limiting Policy
**Files to create/modify:**
- `HotelManagement.API/Program.cs` — Add `ConciergePolicy` rate limiter (30 req/min per JWT `sub`)
- `HotelManagement.BLL/Options/ConciergeOptions.cs` — Already has `RateLimitPerMinute = 30`

**Implementation:**
```csharp
// In Program.cs, inside rate limiter region:
options.AddPolicy("ConciergePolicy", context =>
    RateLimitPartition.GetTokenBucketLimiter(
        context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anon",
        _ => new TokenBucketRateLimiterOptions
        {
            TokenLimit = 30,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 5,
            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
            TokensPerPeriod = 30
        }));
```

**Controller:** Add `[RequireRateLimiting("ConciergePolicy")]` to `ConciergeController`.

#### 2.1.2 Request Validation & Error Enrichment
**Files to modify:**
- `HotelManagement.API/Controllers/ConciergeController.cs` — Add `ModelState` validation, enrich error responses
- `HotelManagement.BLL/Services/Concierge/ConciergeService.cs` — Throw typed exceptions instead of returning `Fail()` for validation errors

**New exception types (create):**
- `HotelManagement.BLL/Exceptions/ConciergeValidationException.cs` — wraps validation failures with field-level detail
- `HotelManagement.BLL/Exceptions/ConciergeProposalExpiredException.cs`
- `HotelManagement.BLL/Exceptions/ConciergeProposalNotFoundException.cs`

**Controller error mapping:**
```csharp
catch (ConciergeValidationException ex) 
    => BadRequest(new { ex.Message, ex.Errors }); // Errors: Dictionary<string, string[]>
catch (ConciergeProposalExpiredException) 
    => BadRequest(new { Message = "Proposal expired. Please try again." });
catch (ConciergeProposalNotFoundException) 
    => NotFound(new { Message = "Proposal not found." });
```

#### 2.1.3 Idempotency Key Generation (Frontend)
**Files to modify:**
- `Frontend/src/app/features/user/services/concierge-api.service.ts` — Add interceptor/header logic
- `Frontend/src/app/features/user/components/concierge-chat/concierge-chat.component.ts` — Track turn number

**Implementation:**
```typescript
// In concierge-api.service.ts interceptor (or HTTP interceptor)
private turnNumber = 0;
chat(request) {
  const key = `concierge:turn:${this.conversationId}:${++this.turnNumber}`;
  return this.http.post(url, request, { headers: new HttpHeaders({ 'X-Idempotency-Key': key }) });
}
```

---

### 2.2 Proposal TTL Background Job (Backend)

#### 2.2.1 Hosted Service for Proposal Cleanup
**Files to create:**
- `HotelManagement.BLL/Workers/ProposalCleanupWorker.cs` — `IHostedService` running every 1 min
- Register in `Program.cs`: `builder.Services.AddHostedService<ProposalCleanupWorker>();`

**Implementation:**
```csharp
public class ProposalCleanupWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ProposalCleanupWorker> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(1);

    public ProposalCleanupWorker(IServiceScopeFactory scopeFactory, ILogger<ProposalCleanupWorker> logger) { ... }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(_interval, stoppingToken);
            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IConciergeProposalRepository>();
            await repo.CleanupExpiredAsync();
        }
    }
}
```

**Repository method already exists:** `IConciergeProposalRepository.CleanupExpiredAsync()` → calls `ConciergeProposalRepository.CleanupExpiredAsync()` (marks `status = 'expired'` where `status = 'pending' AND expires_at < NOW()`).

---

### 2.3 Frontend Polish (Angular)

#### 2.3.1 Loading States & Skeleton UI
**Files to modify:**
- `Frontend/src/app/features/user/components/concierge-chat/concierge-chat.component.ts` — Add `thinking` signal
- `Frontend/src/app/features/user/components/concierge-chat/concierge-chat.component.html` — Skeleton while loading

**Changes:**
```typescript
// In component
thinking = signal(false);
// In sendMessage()
this.thinking.set(true);
this.api.chat(request).pipe(
  finalize(() => this.thinking.set(false))
).subscribe(...);
```

**Template:** Show `mat-spinner` overlay on message bubble while `thinking()`.

#### 2.3.2 Error Toast Notifications
**Files to modify:**
- `Frontend/src/app/features/user/components/concierge-chat/concierge-chat.component.ts` — Inject `MatSnackBar`
- Show snackbars for: network errors, proposal expired, validation errors

**Implementation:**
```typescript
private snackBar = inject(MatSnackBar);
handleError(err: any) {
  const msg = err.error?.message || err.message || 'Something went wrong';
  this.snackBar.open(msg, 'Dismiss', { duration: 5000, panelClass: 'error-snackbar' });
}
```

#### 2.3.3 Accessibility (a11y)
**Files to modify:**
- `Frontend/src/app/features/user/components/concierge-chat/concierge-chat.component.html` — Add ARIA attributes

**Required attributes:**
```html
<div #messagesContainer role="log" aria-live="polite" aria-label="Conversation">
  ...
  <div class="message" role="article" aria-label="{{ msg.role }} message">
  <button mat-icon-button aria-label="Send message">
  <button mat-flat-button aria-label="Confirm and execute proposed actions">
```

#### 2.3.4 Proposal Card UX Improvements
**Files to modify:**
- `Frontend/src/app/features/user/components/concierge-chat/concierge-chat.component.html` — Countdown timer, dismiss button
- `Frontend/src/app/features/user/components/concierge-chat/concierge-chat.component.ts` — Timer logic, dismiss handler

**Add to proposal card:**
```html
<div class="proposal-countdown" *ngIf="getTimeRemaining(p) > 0">
  Expires in {{ getTimeRemaining(p) }}s
</div>
<button mat-icon-button (click)="dismissProposal(p.proposalId)" aria-label="Dismiss proposal">
  <mat-icon>close</mat-icon>
</button>
```
```typescript
getTimeRemaining(proposal: ConciergeProposal): number {
  return Math.max(0, Math.ceil((new Date(proposal.expiresAt).getTime() - Date.now()) / 1000));
}
dismissProposal(id: string) {
  this.pendingProposals.update(p => p.filter(x => x.proposalId !== id));
}
```

#### 2.3.5 Message Timestamps & Grouping
**Files to modify:**
- `Frontend/src/app/features/user/components/concierge-chat/concierge-chat.component.html` — Group consecutive messages by same role

**Implementation:** Track `lastRole` in template loop, show timestamp only on role change or >5 min gap.

#### 2.3.6 Conversation History Persistence (localStorage)
**Files to modify:**
- `Frontend/src/app/features/user/services/concierge-api.service.ts` — Add `saveConversation()` / `loadConversation()`
- `Frontend/src/app/features/user/components/concierge-chat/concierge-chat.component.ts` — Load on init, save on each message

**Implementation:**
```typescript
// Service
private readonly STORAGE_KEY = 'concierge_conversations';
saveConversation(convId: string, messages: ChatMessage[]) {
  const all = JSON.parse(localStorage.getItem(this.STORAGE_KEY) || '{}');
  all[convId] = messages.slice(-20); // keep last 20
  localStorage.setItem(this.STORAGE_KEY, JSON.stringify(all));
}
loadConversation(convId: string): ChatMessage[] {
  return JSON.parse(localStorage.getItem(this.STORAGE_KEY) || '{}')[convId] || [];
}
```

---

### 2.4 Backend Observability & Logging

#### 2.4.1 Structured Logging in ConciergeService
**Files to modify:**
- `HotelManagement.BLL/Services/Concierge/ConciergeService.cs` — Add `ILogger` calls at key points

**Log events:**
```csharp
_logger.LogInformation("Concierge chat started: user={UserId}, conv={ConvId}", userId, convId);
_logger.LogInformation("Tool calls: {Count}, proposals={Proposals}, actions={Actions}", 
    toolCalls.Count, proposals.Count, actions.Count);
_logger.LogWarning("Proposal validation failed: {ToolName} - {Error}", toolName, error);
_logger.LogInformation("Proposals confirmed: {Count} for conv={ConvId}", proposalIds.Count, conversationId);
_logger.LogError(ex, "Concierge error for user={UserId}", userId);
```

#### 2.4.2 OpenTelemetry Metrics (config-driven)
**Files to modify:**
- `HotelManagement.API/Program.cs` — Add `AddOpenTelemetry().WithMetrics(...)`
- `HotelManagement.BLL/Options/ConciergeOptions.cs` — Add `EnableMetrics` flag

**Metrics to emit:**
- `concierge.chat.requests` (counter, tags: outcome=success|error)
- `concierge.tool.calls` (counter, tags: tool_name, outcome)
- `concierge.proposals.created` / `.confirmed` / `.expired` (counter)
- `concierge.latency.ms` (histogram, tags: endpoint)

---

### 2.5 API Response Enrichment

#### 2.5.1 Structured Error Response Format
**Files to modify:**
- `HotelManagement.BLL/DTOs/ConciergeDTOs.cs` — Add `ConciergeErrorResponseDTO`
- `HotelManagement.API/Controllers/ConciergeController.cs` — Return consistent error shape

**DTO:**
```csharp
public class ConciergeErrorResponseDTO
{
    public string ErrorCode { get; set; } = string.Empty; // VALIDATION_ERROR, PROPOSAL_EXPIRED, etc.
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, string[]>? Details { get; set; }
    public string? TraceId { get; set; }
}
```

**Controller returns:** `BadRequest(errorDto)` or `StatusCode(422, errorDto)` for validation errors.

#### 2.5.2 Request/Response Logging Middleware (Optional)
**Files to create:**
- `HotelManagement.API/Middleware/ConciergeRequestLoggingMiddleware.cs` — Log request/response bodies for concierge endpoints only (sanitized)

---

### 2.6 Testing Additions

#### 2.6.1 Unit Tests for New Logic
**Files to create:**
- `HotelManagement.UnitTesting/Services/Concierge/ProposalCleanupWorkerTests.cs`
- `HotelManagement.UnitTesting/Services/Concierge/ConciergeService_ValidationTests.cs`
- `HotelManagement.UnitTesting/Controllers/ConciergeControllerTests.cs`

**Key test cases:**
- `ProcessMessageAsync` returns proposals for side-effect tools, executes read-only tools immediately
- `ConfirmProposalsAsync` rejects expired proposals, executes confirmed ones, logs audit
- `ProposalCleanupWorker` marks expired proposals within 1 min of TTL
- Controller returns 429 when rate limit exceeded, 400 on validation error, 404 on missing proposal

#### 2.6.2 Frontend Component Tests
**Files to create:**
- `Frontend/src/app/features/user/components/concierge-chat/concierge-chat.component.spec.ts`

**Key test cases:**
- Sends message → shows proposal card → confirm executes → shows success
- Dismisses proposal → proposal removed from UI
- Rate limit error shows snackbar
- Accessibility: role="log", aria-live, button labels

---

## Phase 2 Acceptance Criteria Checklist

| # | Criterion | Done |
|---|-----------|------|
| 1 | Per-user rate limit (30 req/min) enforced on `/chat` and `/confirm` | ☐ |
| 2 | Idempotency key sent by frontend (`concierge:turn:{convId}:{turn}`) | ☐ |
| 3 | Proposal TTL cleanup worker runs every 1 min, marks expired proposals | ☐ |
| 4 | Frontend shows skeleton while `thinking=true` | ☐ |
| 5 | Error toasts appear for network/validation/proposal-expired errors | ☐ |
| 6 | ARIA attributes on chat container, messages, buttons | ☐ |
| 7 | Proposal cards show countdown timer + dismiss button | ☐ |
| 8 | Conversation history persists in localStorage (last 20 messages) | ☐ |
| 9 | Structured logging at INFO/WARN/ERROR in `ConciergeService` | ☐ |
| 10 | OpenTelemetry metrics emitted for chat/proposals/latency | ☐ |
| 11 | Structured error responses with `ErrorCode`, `Details`, `TraceId` | ☐ |
| 12 | Unit tests pass for worker, validation, controller | ☐ |
| 13 | Frontend component tests pass (a11y, proposal flow, error handling) | ☐ |

---

## File Edit Surface Summary (Phase 2)

### New Files (Backend)
1. `HotelManagement.BLL/Exceptions/ConciergeValidationException.cs`
2. `HotelManagement.BLL/Exceptions/ConciergeProposalExpiredException.cs`
3. `HotelManagement.BLL/Exceptions/ConciergeProposalNotFoundException.cs`
4. `HotelManagement.BLL/Workers/ProposalCleanupWorker.cs`
5. `HotelManagement.API/Middleware/ConciergeRequestLoggingMiddleware.cs` (optional)
6. Test files in `HotelManagement.UnitTesting/...`

### Modified Files (Backend)
1. `HotelManagement.API/Program.cs` — rate limiter, hosted service, OTel
2. `HotelManagement.API/Controllers/ConciergeController.cs` — validation, error mapping, `[RequireRateLimiting]`
3. `HotelManagement.BLL/Services/Concierge/ConciergeService.cs` — typed exceptions, logging
4. `HotelManagement.BLL/Options/ConciergeOptions.cs` — `EnableMetrics` flag

### New Files (Frontend)
1. `Frontend/src/app/features/user/components/concierge-chat/concierge-chat.component.spec.ts`

### Modified Files (Frontend)
1. `Frontend/src/app/features/user/services/concierge-api.service.ts` — idempotency interceptor, localStorage persistence
2. `Frontend/src/app/features/user/components/concierge-chat/concierge-chat.component.ts` — loading, toasts, a11y, countdown, dismiss
3. `Frontend/src/app/features/user/components/concierge-chat/concierge-chat.component.html` — skeleton, ARIA, timer, dismiss
4. `Frontend/src/app/features/user/components/concierge-chat/concierge-chat.component.scss` — skeleton styles, toast panelClass

---

## Serena/GitNexus Commands for Planning Pass

Before implementation, run these to verify edit surface:

```bash
# Verify ConciergeService impact
gitnexus impact --target "ConciergeService" --direction upstream

# Verify ConciergeController impact
gitnexus impact --target "ConciergeController" --direction upstream

# Verify no conflict with existing hosted services
gitnexus query --search "IHostedService" --limit 10

# Verify frontend component references
gitnexus query --search "concierge-chat" --limit 5
```

---

## Execution Order (Recommended)

1. **Backend: Rate Limiting + Idempotency Key Flow** (Program.cs, Controller, Frontend interceptor)
2. **Backend: Proposal Cleanup Worker** (Worker, DI registration, test)
3. **Backend: Validation Exceptions + Controller Error Mapping** (Exceptions, Service, Controller)
4. **Backend: Structured Logging + Metrics** (Service, Program.cs)
4. **Frontend: Loading State + Error Toasts** (Component, HTML, SCSS)
5. **Frontend: Proposal Card Polish** (Countdown, dismiss, a11y)
6. **Frontend: Conversation Persistence** (Service, Component init)
7. **Tests** (Backend unit, Frontend component)

---

**This plan is complete and ready for execution. All file paths, function names, and connections to Phase 1 artifacts are explicitly specified. No ambiguity remains.**