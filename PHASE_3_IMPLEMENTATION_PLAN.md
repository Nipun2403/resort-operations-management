# AI Concierge — Phase 3 (P3: Polish & Streaming) Implementation Plan

**Version**: 1.0  
**Date**: 2026-07-14  
**Based on**: `AI_CONCIERGE_DESIGN.md` Section 10 (Rollout Plan) + current codebase state post-Phase 2  
**Governance**: Per `AGENTS.md` — Explicit Requirements Protocol, GitNexus impact analysis, Minimal Change Discipline, Ponytail YAGNI

---

## 📊 Current State Summary (Post Phase 2)

| Component | Status | Key Details |
|-----------|--------|-------------|
| **Backend API** | ✅ Complete | `ConciergeController` with chat, confirm, proposals, context endpoints; rate limiting (30/min), idempotency, structured errors |
| **BLL Service** | ✅ Complete | `ConciergeService` with 8 tools (3 side-effect, 5 read-only), two-step pattern, validation exceptions, structured logging |
| **Frontend Chat** | ✅ Complete | `ConciergeChatComponent` with proposals, confirm/dismiss, countdown timer, localStorage persistence, ARIA, toasts |
| **SignalR** | ✅ Complete | `SignalRNotificationService` → `NotificationHub` → kitchen/housekeeping/maintenance groups |
| **OpenTelemetry** | ⚠️ Configured only | Meter registered, Prometheus exporter added, **no metrics emitted yet** |
| **Background Jobs** | ✅ Complete | `ProposalCleanupWorker` runs every 1 min |

---

## 🎯 Phase 3 Scope (Per Design Doc Section 10)

| Feature | Duration | Priority | Design Doc Reference |
|---------|----------|----------|---------------------|
| **3.1 SSE/SignalR Streaming** | 1 day | P0 | "SSE/SignalR streaming for token-by-token replies" |
| **3.2 Menu Carousel** | 0.5 day | P1 | "Menu browsing (carousel)" |
| **3.3 Order History** | 0.5 day | P1 | "Order history" |
| **3.4 Multi-language (i18n)** | 0.5 day | P2 | "Multi-language (i18n keys)" |
| **3.5 Emit OTel Metrics** | 0.25 day | P1 | Section 7.5, 16.3 |
| **3.6 Testing & Hardening** | 0.25 day | P1 | Section 9 |

**Total: ~3 days**

---

## 📋 3.1 SSE/SignalR Streaming Implementation

### Architecture Decision
- **SSE (Server-Sent Events)** over HTTP for streaming — works through any load balancer/proxy, no sticky sessions
- **SignalR** retained for staff alerts (already implemented)
- **Streaming endpoint**: `GET /api/v1/concierge/chat/stream?message=...&conversationId=...`

### 3.1.1 New DTOs
**File**: `Backend/HotelManagement.BLL/DTOs/ConciergeDTOs.cs` (append)

```csharp
public class ConciergeStreamRequestDTO
{
    public string Message { get; set; } = string.Empty;
    public string? ConversationId { get; set; }
}

public class ConciergeStreamChunkDTO
{
    public string ConversationId { get; set; } = string.Empty;
    public string Delta { get; set; } = string.Empty;        // Token delta
    public bool IsComplete { get; set; } = false;            // Final chunk
    public List<ConciergeProposalDTO>? Proposals { get; set; } // Only on final
    public List<ConciergeActionResultDTO>? Actions { get; set; } // Only on final
}
```

### 3.1.2 IConciergeService Extension
**File**: `Backend/HotelManagement.BLL/Interfaces/IConciergeService.cs`

```csharp
Task ProcessMessageStreamAsync(
    string userMessage,
    string? conversationId,
    IAsyncEnumerable<ConciergeStreamChunkDTO> outputStream,
    CancellationToken ct);
```

### 3.1.3 ConciergeService Streaming Implementation
**File**: `Backend/HotelManagement.BLL/Services/Concierge/ConciergeService.cs`

Add new method `ProcessMessageStreamAsync` that:
1. Reuses existing `BuildGuestContextAsync`, `PromptBuilder.BuildSystemPrompt`, conversation history loading
2. Uses `_chatClient.CompleteChatStreamingAsync` instead of `CompleteChatAsync`
3. Yields `ConciergeStreamChunkDTO` for each token delta
4. On tool calls: same two-step logic (propose → confirm) but only emits final chunk with proposals
5. On completion: emits final chunk with `IsComplete=true`, proposals, actions
6. Persists conversation same as non-streaming

### 3.1.4 Controller Endpoint
**File**: `Backend/HotelManagement.API/Controllers/ConciergeController.cs`

```csharp
[HttpGet("chat/stream")]
[RequireRateLimiting("ConciergePolicy")]
public async Task StreamChat(
    [FromQuery] string message,
    [FromQuery] string? conversationId,
    CancellationToken ct)
{
    if (string.IsNullOrWhiteSpace(message))
    {
        Response.StatusCode = 400;
        await Response.WriteAsync("data: {\"error\":\"Message is required\"}\n\n", ct);
        return;
    }

    Response.Headers.ContentType = "text/event-stream";
    Response.Headers.CacheControl = "no-cache";
    Response.Headers.Connection = "keep-alive";

    var sanitized = InputSanitizer.Sanitize(message);
    var stream = _concierge.ProcessMessageStreamAsync(sanitized, conversationId, ct);
    
    await foreach (var chunk in stream.WithCancellation(ct))
    {
        var json = JsonSerializer.Serialize(chunk);
        await Response.WriteAsync($"data: {json}\n\n", ct);
        await Response.Body.FlushAsync(ct);
    }
}
```

### 3.1.5 Frontend API Service
**File**: `Frontend/src/app/features/user/services/concierge-api.service.ts`

```typescript
export interface ConciergeStreamChunk {
  conversationId: string;
  delta: string;
  isComplete: boolean;
  proposals?: ConciergeProposal[];
  actions?: ConciergeActionResult[];
}

streamChat(request: ConciergeChatRequest): Observable<ConciergeStreamChunk> {
  const conversationId = request.conversationId ?? this.generateConversationId();
  const turnNumber = this.getNextTurnNumber(conversationId);
  const idempotencyKey = `concierge:turn:${conversationId}:${turnNumber}`;

  const url = `${this.baseUrl}/chat/stream?message=${encodeURIComponent(request.message)}&conversationId=${conversationId}`;
  
  return new Observable<ConciergeStreamChunk>(observer => {
    const eventSource = new EventSource(url, { withCredentials: true });
    
    eventSource.onmessage = (event) => {
      const chunk = JSON.parse(event.data) as ConciergeStreamChunk;
      observer.next(chunk);
      if (chunk.isComplete) { observer.complete(); eventSource.close(); }
    };
    eventSource.onerror = (err) => { observer.error(err); eventSource.close(); };
    return () => eventSource.close();
  });
}
```

### 3.1.6 Frontend Component Streaming Integration
**File**: `Frontend/src/app/features/user/components/concierge-chat/concierge-chat.component.ts`

- Add signals: `streaming = signal(false)`, `streamingReply = signal('')`
- In `sendMessage()`: if streaming supported, call `api.streamChat()` and subscribe
- Accumulate `delta` into `streamingReply`, update message bubble in real-time
- On `isComplete`: finalize message, handle proposals/actions same as non-streaming

---

## 📋 3.2 Menu Carousel Implementation

### 3.2.1 Backend: Enhanced Menu Item DTO
**File**: `Backend/HotelManagement.BLL/DTOs/ConciergeDTOs.cs`

```csharp
public class MenuItemCarouselDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public bool IsAvailable { get; set; }
    public List<string> Tags { get; set; } = new(); // e.g., ["vegetarian", "spicy"]
}
```

**File**: `Backend/HotelManagement.BLL/Services/Concierge/ConciergeService.cs` — `GetMenuItemsAsync`
- Return `MenuItemCarouselDTO[]` instead of raw JSON
- Include `ImageUrl` from `MenuItem` entity (check if property exists)

### 3.2.2 Frontend: Carousel Component
**New File**: `Frontend/src/app/features/user/components/concierge-chat/menu-carousel/menu-carousel.component.ts`

```typescript
@Component({
  selector: 'app-menu-carousel',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatButtonModule, MatIconModule, MatChipsModule],
  template: `
    <div class="carousel" role="region" aria-label="Menu items">
      @for (item of items(); track item.id) {
        <mat-card class="menu-card" appearance="outlined">
          @if (item.imageUrl) {
            <img mat-card-image [src]="item.imageUrl" [alt]="item.name" loading="lazy">
          }
          <mat-card-content>
            <h3>{{ item.name }}</h3>
            <p class="description">{{ item.description }}</p>
            <mat-chip-set>
              @for (tag of item.tags; track tag) { <mat-chip>{{ tag }}</mat-chip> }
            </mat-chip-set>
            <div class="price">{{ item.price | currency }}</div>
          </mat-card-content>
          <mat-card-actions>
            <button mat-flat-button color="primary" (click)="select.emit(item)">Add to Order</button>
          </mat-card-actions>
        </mat-card>
      }
    </div>
  `,
  styles: [`
    .carousel { display: flex; gap: 16px; overflow-x: auto; padding: 8px; }
    .menu-card { min-width: 280px; max-width: 280px; flex-shrink: 0; }
    .description { font-size: 13px; color: var(--mat-sys-on-surface-variant); margin: 8px 0; }
    .price { font-weight: 600; color: var(--mat-sys-primary); margin-top: 8px; }
  `]
})
export class MenuCarouselComponent {
  items = input<MenuItemCarouselDTO[]>([]);
  select = output<MenuItemCarouselDTO>();
}
```

### 3.2.3 Integration in Chat Component
- When `GetMenuItems` tool returns results, render `MenuCarouselComponent` in assistant message bubble
- User clicks "Add to Order" → pre-fills message input with "I'd like to order [item name]"

---

## 📋 3.3 Order History Implementation

### 3.3.1 Backend: New Tool
**File**: `Backend/HotelManagement.BLL/Services/Concierge/ToolDefinitions.cs`

```csharp
new FunctionDefinition
{
    Name = "get_order_history",
    Description = "Retrieve guest's past room-service orders (delivered/cancelled)",
    Parameters = JsonSchema.FromType<EmptyArgs>()
}
```

**File**: `Backend/HotelManagement.BLL/Services/Concierge/ConciergeService.cs`

```csharp
public async Task<ConciergeActionResultDTO> GetOrderHistoryAsync(GuestContextDTO ctx, CancellationToken ct)
{
    if (ctx.BookingId == null) return Fail("No active booking.");
    var orders = await _orderService.GetOrderHistoryAsync(1, 10, ctx.BookingId.Value);
    var json = JsonSerializer.Serialize(orders.Data.Select(o => new { 
        o.Id, o.OrderStatus, o.GeneratedAt, 
        Items = o.OrderItems.Select(i => new { i.MenuItemName, i.Quantity, i.PriceAtPurchase })
    }));
    return Success(json);
}
```

**File**: `Backend/HotelManagement.BLL/Services/Concierge/ToolExecutor.cs` — Add case for `GetOrderHistory`

### 3.3.2 Frontend: Order History Display
- Render as expandable list in chat bubble with status badges
- Re-order button: pre-fills message with "I'd like to order [items from order #X] again"

---

## 📋 3.4 Multi-language (i18n) Support

### 3.4.1 Backend: Language Detection in Prompt
**File**: `Backend/HotelManagement.BLL/Services/Concierge/PromptBuilder.cs`

```csharp
private static string BuildSystemPrompt(GuestContextDTO ctx, string? language = null)
{
    var lang = language ?? "en";
    var sb = new StringBuilder();
    sb.AppendLine($"You are the AI Concierge for a luxury hotel. Reply in {GetLanguageName(lang)}.");
    // ... rest of prompt
}

private static string GetLanguageName(string code) => code switch {
    "en" => "English", "es" => "Spanish", "fr" => "French", "de" => "German",
    "zh" => "Chinese", "ja" => "Japanese", "ar" => "Arabic", "hi" => "Hindi",
    "pt" => "Portuguese", "ru" => "Russian", _ => "English"
};
```

**File**: `Backend/HotelManagement.BLL/Interfaces/IConciergeService.cs`

```csharp
Task<ConciergeChatResponseDTO> ProcessMessageAsync(string userMessage, string? conversationId = null, string? language = null, CancellationToken ct = default);
```

### 3.4.2 Frontend: Language Selector
**File**: `Frontend/src/app/features/user/components/concierge-chat/concierge-chat.component.ts`
- Add language dropdown in header (EN, ES, FR, DE, ZH, JA, AR, HI, PT, RU)
- Pass `language` param in `chat()` and `streamChat()` calls
- Store preference in localStorage

### 3.4.3 Translation Keys
**New Files**: `Frontend/src/app/features/user/components/concierge-chat/i18n/{en,es,fr,de,zh,ja,ar,hi,pt,ru}.json`

```json
{
  "chat.welcome": "Hello {name}! I'm your AI Concierge...",
  "chat.placeholder": "Ask me anything...",
  "chat.thinking": "Thinking...",
  "proposal.confirm": "Confirm & Execute",
  "proposal.dismiss": "Dismiss",
  "proposal.expires": "Expires in {time}",
  "proposal.cancelled": "Proposal cancelled",
  "proposal.confirmed": "Proposal confirmed & executed",
  "quick.food": "Order Food",
  "quick.pillows": "Extra Pillows",
  "quick.issue": "Report Issue",
  "quick.bill": "Check Bill",
  "quick.checkout": "Check-out Time",
  "quick.status": "Room Status"
}
```

---

## 📋 3.5 Emit OpenTelemetry Metrics (Complete the Setup)

### 3.5.1 Backend: Meter in ConciergeService
**File**: `Backend/HotelManagement.BLL/Services/Concierge/ConciergeService.cs`

```csharp
private static readonly Meter s_meter = new("HotelManagement.Concierge");
private static readonly Counter<long> s_chatRequests = s_meter.CreateCounter<long>("concierge.chat.requests", description: "Total chat requests");
private static readonly Counter<long> s_toolCalls = s_meter.CreateCounter<long>("concierge.tool.calls", description: "Tool calls by name and outcome");
private static readonly Counter<long> s_proposals = s_meter.CreateCounter<long>("concierge.proposals", description: "Proposals created/confirmed/expired");
private static readonly Histogram<double> s_latency = s_meter.CreateHistogram<double>("concierge.latency.ms", unit: "ms", description: "End-to-end latency");

// In ProcessMessageAsync:
var sw = Stopwatch.StartNew();
s_chatRequests.Add(1, new("outcome", "started"));
try {
  // ... existing logic
  s_chatRequests.Add(1, new("outcome", "success"));
} catch (Exception) {
  s_chatRequests.Add(1, new("outcome", "error"));
  throw;
} finally {
  s_latency.Record(sw.Elapsed.TotalMilliseconds, new("endpoint", "chat"));
}
```

---

## 📋 3.6 Testing & Hardening Checklist

### Unit Tests (Backend)
- [ ] `ConciergeService_StreamingTests.cs` — token delta emission, final chunk structure, proposal handling in stream
- [ ] `ConciergeController_StreamTests.cs` — SSE response format, cancellation, rate limiting on stream endpoint

### Unit Tests (Frontend)
- [ ] `MenuCarouselComponentTests.ts` — rendering, selection output
- [ ] `OrderHistoryDisplayTests.ts` — display, re-order action

### Integration Tests
- [ ] Full streaming flow: Controller → Service → OpenAI streaming → SSE → Frontend accumulation
- [ ] Menu carousel: Tool call → Carousel render → Add to order → New message
- [ ] Order history: Tool call → History display → Re-order

### E2E (Playwright)
- [ ] Guest sends message → sees token-by-token reply → confirms proposal → staff alerts fire
- [ ] Guest clicks "Order Food" → menu carousel appears → clicks item → message pre-filled
- [ ] Guest switches language → UI + AI replies in selected language

### Performance
- [ ] k6 script: 50 concurrent streaming connections, verify <2s TTFT (time to first token), no memory leaks

---

## 📝 Edit Surface Summary

### New Files (Backend)
1. `Backend/HotelManagement.BLL/DTOs/ConciergeDTOs.cs` — append streaming DTOs + carousel DTO
2. `Backend/HotelManagement.BLL/Services/Concierge/ConciergeService.cs` — add `ProcessMessageStreamAsync`, `GetOrderHistoryAsync`
3. `Backend/HotelManagement.BLL/Interfaces/IConciergeService.cs` — add streaming + language + order history methods
4. `Backend/HotelManagement.API/Controllers/ConciergeController.cs` — add `StreamChat` endpoint
5. Test files in `Backend/HotelManagement.UnitTesting/...`

### Modified Files (Backend)
1. `Backend/HotelManagement.BLL/DTOs/ConciergeDTOs.cs` — append new DTOs
2. `Backend/HotelManagement.BLL/Services/Concierge/ToolDefinitions.cs` — add `get_order_history`
3. `Backend/HotelManagement.BLL/Services/Concierge/ToolExecutor.cs` — dispatch `GetOrderHistory`
4. `Backend/HotelManagement.BLL/Services/Concierge/ConciergeService.cs` — streaming impl + OTel metrics
5. `Backend/HotelManagement.BLL/Services/Concierge/PromptBuilder.cs` — language parameter

### New Files (Frontend)
1. `Frontend/src/app/features/user/components/concierge-chat/menu-carousel/` — component + template + styles
2. `Frontend/src/app/features/user/components/concierge-chat/i18n/` — 10 locale JSON files
3. `Frontend/src/app/features/user/services/concierge-api.service.ts` — add `streamChat()` method
4. Test files

### Modified Files (Frontend)
1. `Frontend/src/app/features/user/components/concierge-chat/concierge-chat.component.ts` — streaming state, language selector, order history render
2. `Frontend/src/app/features/user/components/concierge-chat/concierge-chat.component.html` — language dropdown, carousel slot, history slot
3. `Frontend/src/app/features/user/components/concierge-chat/concierge-chat.component.scss` — carousel styles, language dropdown
4. `Frontend/src/app/features/user/services/concierge-api.service.ts` — streaming observable

---

## ⚖️ Assumptions & Decisions (Per AGENTS.md)

| # | Decision | Rationale |
|---|----------|-----------|
| 1 | SSE over SignalR for streaming | Simpler HTTP, works through any proxy/LB, no sticky sessions |
| 2 | Reuse existing `ConciergeTools` for new tools | No new schema; just add function definition |
| 3 | Language passed per-request (not stored) | Stateless; guest can switch anytime |
| 4 | Menu images from existing `MenuItem.ImageUrl` | No new storage; reuse entity field |
| 5 | OTel metrics emitted from BLL (not Controller) | Captures service logic latency, not just HTTP |
| 6 | Carousel is read-only (no quantity picker in card) | Keeps UX simple; quantity confirmed in chat |
| 7 | Order history limited to 10 most recent | Performance; pagination if needed later |

---

## ✅ Acceptance Criteria Checklist (Per AGENTS.md)

| # | Criterion | Done? |
|---|-----------|-------|
| 1 | SSE endpoint returns token deltas as `data: {...}\n\n` | ☐ |
| 2 | Frontend accumulates deltas, shows typing animation | ☐ |
| 3 | Final chunk includes proposals/actions, triggers confirmation UI | ☐ |
| 4 | Menu carousel renders image, name, price, tags, "Add to Order" | ☐ |
| 5 | Clicking carousel item pre-fills message input | ☐ |
| 6 | Order history tool returns last 10 orders with status | ☐ |
| 7 | Language selector (10 langs) updates UI + AI reply language | ☐ |
| 8 | OTel metrics: `concierge.chat.requests`, `concierge.tool.calls`, `concierge.proposals.*`, `concierge.latency.ms` | ☐ |
| 9 | All existing Phase 2 tests pass (no regressions) | ☐ |
| 10 | New unit/integration/E2E tests pass | ☐ |

---

## ⚠️ Risk Assessment (Per AGENTS.md)

| Risk | Level | Mitigation |
|------|-------|------------|
| SSE connection drops on mobile/network change | Medium | Frontend auto-reconnect with `EventSource` retry; fallback to non-streaming endpoint |
| OpenAI streaming rate limits | Low | Existing `MaxToolCallsPerTurn=5`, token budget `maxTokens=1000` |
| Menu images missing/broken | Low | `loading="lazy"`, fallback placeholder, graceful degradation |
| i18n: AI replies in wrong language | Medium | System prompt enforces language; add few-shot examples in prompt |
| OTel metrics cardinality explosion | Low | Fixed label sets (tool_name, outcome, endpoint) |

---

## 📦 Dependencies to Add

```xml
<!-- Backend/HotelManagement.BLL.csproj -->
<PackageReference Include="OpenAI" Version="2.0.0" /> <!-- Already present -->
<!-- No new packages needed for streaming (OpenAI 2.0 supports CompleteChatStreamingAsync) -->

<!-- Frontend/package.json -->
"@angular/common": "^18.0.0",  <!-- Already present -->
"@angular/material": "^18.0.0" <!-- Already present -->
```

---

## 🚀 Execution Order (Recommended)

1. **3.5 OTel Metrics** (30 min) — independent, unblocks observability
2. **3.1 SSE Streaming Backend** (4 hrs) — core feature, test with curl first
3. **3.1 SSE Streaming Frontend** (3 hrs) — integrate EventSource, token accumulation
4. **3.2 Menu Carousel** (2 hrs) — new component, tool integration
5. **3.3 Order History** (2 hrs) — new tool, display component
6. **3.4 i18n** (3 hrs) — 10 locale files, prompt modification, selector UI
7. **3.6 Testing** (4 hrs) — unit, integration, E2E, k6

---

## 🔍 Pre-Execution Verification (GitNexus Impact Analysis)

Before any edits, run:

```bash
# Verify ConciergeService impact
gitnexus impact --target "ConciergeService" --direction upstream

# Verify ConciergeController impact
gitnexus impact --target "ConciergeController" --direction upstream

# Check for existing streaming usage
gitnexus query --search "CompleteChatStreamingAsync" --limit 5

# Verify frontend component references
gitnexus query --search "concierge-chat" --limit 5
```

---

**End of Plan** — Ready for review per AGENTS.md Section "Always Do: MUST pause and present the plan to the user before editing"