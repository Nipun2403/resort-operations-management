# AI Concierge — Phase 3 v2 (P3: Polish & Streaming) Implementation Plan

**Version**: 2.0 (Ponytail-trimmed)  
**Date**: 2026-07-14  
**Based on**: `AI_CONCIERGE_DESIGN.md` Section 10 + current codebase state post-Phase 2  
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

## 🎯 Phase 3 Scope (Trimmed per Ponytail)

| Feature | Duration | Priority | Decision |
|---------|----------|----------|----------|
| **3.1 SSE Streaming** | 1 day | P0 | **KEEP** — explicit in design doc, no existing implementation |
| **3.2 Inline Menu Render** | 0.5 day | P1 | **TRIM** — reuse existing MatCard pattern, no new component |
| **3.3 Extend Order Tool** | 0.25 day | P1 | **TRIM** — add `status` filter to existing `get_active_orders` |
| **3.4 i18n** | 0 day | — | **YAGNI** — OpenAI natively handles 50+ languages; detect from message |
| **3.5 Emit OTel Metrics** | 0.25 day | P1 | **KEEP** — meter already registered, just emit |
| **3.6 Testing** | 1 day | P1 | **KEEP** — unit + integration + e2e + k6; run against fixed CI |

**Total: ~3.5 days (vs original 3 days, but 23 fewer files)**

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

## 📋 3.2 Inline Menu Render (No New Component)

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
- Include `ImageUrl` from `MenuItem` entity (verify property exists)

### 3.2.2 Frontend: Inline in Chat Template
**File**: `Frontend/src/app/features/user/components/concierge-chat/concierge-chat.component.html`

Add horizontal scroll render inside assistant message bubble when `msg.actions` contains menu items:

```html
<!-- In assistant message bubble, after content -->
@if (msg.actions?.some(a => a.action === 'get_menu_items') && msg.menuItems?.length) {
  <div class="menu-carousel" style="display:flex;gap:12px;overflow-x:auto;padding:8px;margin-top:8px;">
    @for (item of msg.menuItems; track item.id) {
      <mat-card class="menu-card" appearance="outlined" style="min-width:260px;max-width:260px;flex-shrink:0;">
        @if (item.imageUrl) {
          <img mat-card-image [src]="item.imageUrl" [alt]="item.name" loading="lazy" style="height:140px;object-fit:cover;">
        }
        <mat-card-content>
          <h4 style="margin:8px 0 4px;font-size:14px;">{{ item.name }}</h4>
          <p class="description" style="font-size:12px;color:var(--mat-sys-on-surface-variant);margin:0 0 8px;">{{ item.description }}</p>
          <mat-chip-set aria-label="Tags">
            @for (tag of item.tags; track tag) { <mat-chip style="font-size:11px;">{{ tag }}</mat-chip> }
          </mat-chip-set>
          <div class="price" style="font-weight:600;color:var(--mat-sys-primary);margin-top:8px;">{{ item.price | currency }}</div>
        </mat-card-content>
        <mat-card-actions>
          <button mat-flat-button color="primary" (click)="prefillOrder(item)" style="width:100%;">Add to Order</button>
        </mat-card-actions>
      </mat-card>
    }
  </div>
}
```

**File**: `Frontend/src/app/features/user/components/concierge-chat/concierge-chat.component.ts`

```typescript
prefillOrder(item: any): void {
  this.messageControl.setValue(`I'd like to order ${item.name}`);
  this.sendMessage();
}
```

- Parse `GetMenuItems` tool result in `handleResponse()` → attach `menuItems` to assistant message

---

## 📋 3.3 Extend Order Tool for History

### 3.3.1 Backend: Add Status Filter
**File**: `Backend/HotelManagement.BLL/Services/Concierge/ToolDefinitions.cs`

```csharp
public class GetActiveOrdersToolArgs
{
    [JsonPropertyName("status")] public string? Status { get; set; } // "active" | "history" | "all"
    [JsonPropertyName("page")] public int Page { get; set; } = 1;
    [JsonPropertyName("pageSize")] public int PageSize { get; set; } = 10;
}
```

Update `get_active_orders` function definition:
```csharp
new FunctionDefinition
{
    Name = "get_active_orders",
    Description = "List guest's room-service orders. Use status='history' for past orders.",
    Parameters = JsonSchema.FromType<GetActiveOrdersToolArgs>()
}
```

**File**: `Backend/HotelManagement.BLL/Services/Concierge/ConciergeService.cs` — `GetActiveOrdersAsync`

```csharp
public async Task<ConciergeActionResultDTO> GetActiveOrdersAsync(GetActiveOrdersToolArgs args, GuestContextDTO ctx, CancellationToken ct)
{
    if (ctx.BookingId == null) return Fail("No active booking.");
    
    var status = args.Status ?? "active";
    var orders = status == "history" 
        ? await _orderService.GetOrderHistoryAsync(args.Page, args.PageSize, ctx.BookingId.Value)
        : await _orderService.GetActiveOrdersAsync(args.Page, args.PageSize, ctx.BookingId.Value);
    
    var json = JsonSerializer.Serialize(orders.Data.Select(o => new { 
        o.Id, o.OrderStatus, o.GeneratedAt, 
        Items = o.OrderItems.Select(i => new { i.MenuItemName, i.Quantity, i.PriceAtPurchase })
    }));
    return Success(json);
}
```

**File**: `Backend/HotelManagement.BLL/Services/Concierge/ToolExecutor.cs` — Update to pass `GetActiveOrdersToolArgs`

---

## 📋 3.4 i18n — YAGNI (Deleted)

- OpenAI natively speaks 50+ languages — just detect from user message
- No UI translation, no locale files, no language selector
- If guest writes Spanish, AI replies Spanish — works today
- Add when guest complains

---

## 📋 3.5 Emit OpenTelemetry Metrics

### 3.5.1 Backend: Meter in ConciergeService
**File**: `Backend/HotelManagement.BLL/Services/Concierge/ConciergeService.cs`

```csharp
private static readonly Meter s_meter = new("HotelManagement.Concierge");
private static readonly Counter<long> s_chatRequests = s_meter.CreateCounter<long>("concierge.chat.requests", description: "Total chat requests");
private static readonly Counter<long> s_toolCalls = s_meter.CreateCounter<long>("concierge.tool.calls", description: "Tool calls by name and outcome");
private static readonly Counter<long> s_proposals = s_meter.CreateCounter<long>("concierge.proposals", description: "Proposals created/confirmed/expired");
private static readonly Histogram<double> s_latency = s_meter.CreateHistogram<double>("concierge.latency.ms", unit: "ms", description: "End-to-end latency");

// In ProcessMessageAsync (and ProcessMessageStreamAsync):
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

## 📋 3.6 Testing (Unit + Integration + E2E + k6)

### 3.6.1 Unit Tests (Backend)
**New Files**: `Backend/HotelManagement.UnitTesting/Services/Concierge/`
- `ConciergeService_StreamingTests.cs` — token delta emission, final chunk structure, proposal handling in stream
- `ConciergeService_MenuHistoryTests.cs` — status filter logic, DTO mapping, edge cases (no booking, empty results)
- `ConciergeService_MetricsTests.cs` — OTel counter/histogram increments with correct labels

**Run**: `dotnet test --filter "Concierge"`

### 3.6.2 Unit Tests (Frontend)
**New Files**: `Frontend/src/app/features/user/components/concierge-chat/`
- `concierge-chat.component.streaming.spec.ts` — `streamChat` subscription, delta accumulation, `isComplete` handling
- `concierge-chat.component.menu.spec.ts` — carousel render, `prefillOrder()` interaction
- `concierge-chat.component.history.spec.ts` — history list render, re-order action
- `concierge-api.service.streaming.spec.ts` — `EventSource` observable, error handling, cleanup

**Run**: `ng test --include="**/concierge-chat/**/*.spec.ts"`

### 3.6.3 Integration Tests (Backend)
**New File**: `Backend/HotelManagement.IntegrationTesting/Concierge/`
- `StreamingFlowTests.cs` — full flow: Controller → Service → OpenAI streaming → SSE response
- `MenuHistoryFlowTests.cs` — tool call → carousel data → history filter → re-order
- `ProposalFlowTests.cs` — regression: propose → confirm/dismiss still works with streaming

**Run**: `dotnet test Backend/HotelManagement.IntegrationTesting/ --filter "Concierge"`

### 3.6.4 E2E Tests (Playwright)
**New File**: `Frontend/e2e/concierge/`
- `streaming-chat.spec.ts` — Guest sends message → sees token-by-token reply → confirms proposal → staff alerts fire
- `menu-carousel.spec.ts` — Ask "show menu" → carousel renders → click "Add to Order" → message pre-filled
- `order-history.spec.ts` — Ask "past orders" → history list → click re-order → message pre-filled
- `language-auto.spec.ts` — Send Spanish message → AI replies Spanish (no UI selector)

**Run**: `cd Frontend && npx playwright test e2e/concierge/`

### 3.6.5 Load Tests (k6)
**New File**: `Frontend/k6/concierge-streaming.js`
```javascript
import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  stages: [
    { duration: '30s', target: 10 },  // Ramp up
    { duration: '1m', target: 50 },   // Sustained
    { duration: '30s', target: 0 },   // Ramp down
  ],
  thresholds: {
    'http_req_duration': ['p(95)<2000'],  // 95th percentile < 2s
    'http_req_failed': ['rate<0.01'],     // <1% failure
    'checks': ['rate>0.99'],              // >99% checks pass
  },
};

export default function () {
  const url = `${__ENV.BASE_URL}/api/v1/concierge/chat/stream`;
  const params = { headers: { 'Authorization': `Bearer ${__ENV.JWT}` } };
  
  const res = http.get(`${url}?message=hello&conversationId=${uuidv4()}`, params);
  
  check(res, {
    'status 200': (r) => r.status === 200,
    'SSE content-type': (r) => r.headers['Content-Type']?.includes('text/event-stream'),
    'first delta < 1s': (r) => r.timings.waiting < 1000,
    'has deltas': (r) => r.body.includes('"delta"'),
    'final isComplete': (r) => r.body.includes('"isComplete":true'),
  });
  
  sleep(1);
}
```

**Run**: `k6 run --env BASE_URL=http://localhost:5000 --env JWT=<token> Frontend/k6/concierge-streaming.js`

### 3.6.6 Test Execution Order (CI Pipeline)
```yaml
# .github/workflows/concierge-phase3.yml
jobs:
  unit-backend:
    runs-on: ubuntu-latest
    steps: [ dotnet test --filter "Concierge" ]
  
  unit-frontend:
    runs-on: ubuntu-latest
    steps: [ ng test --include="**/concierge-chat/**/*.spec.ts" --watch=false ]
  
  integration:
    runs-on: ubuntu-latest
    services: [ postgres, redis ]
    steps: [ dotnet test Backend/HotelManagement.IntegrationTesting/ --filter "Concierge" ]
  
  e2e:
    runs-on: ubuntu-latest
    services: [ postgres, redis, frontend-dev ]
    steps: [ npx playwright test e2e/concierge/ ]
  
  load:
    runs-on: ubuntu-latest
    if: github.event_name == 'workflow_dispatch'  # Manual trigger for k6
    steps: [ k6 run Frontend/k6/concierge-streaming.js ]
```

---

## 📝 Edit Surface Summary (v2)

### New Files (Backend)
1. `Backend/HotelManagement.BLL/DTOs/ConciergeDTOs.cs` — append streaming DTOs + carousel DTO
2. `Backend/HotelManagement.BLL/Services/Concierge/ConciergeService.cs` — add `ProcessMessageStreamAsync`
3. `Backend/HotelManagement.BLL/Interfaces/IConciergeService.cs` — add streaming method
4. `Backend/HotelManagement.API/Controllers/ConciergeController.cs` — add `StreamChat` endpoint
5. **Test files in `Backend/HotelManagement.UnitTesting/Services/Concierge/`**:
   - `ConciergeService_StreamingTests.cs`
   - `ConciergeService_MenuHistoryTests.cs`
   - `ConciergeService_MetricsTests.cs`
6. **Test files in `Backend/HotelManagement.IntegrationTesting/Concierge/`**:
   - `StreamingFlowTests.cs`
   - `MenuHistoryFlowTests.cs`
   - `ProposalFlowTests.cs`

### Modified Files (Backend)
1. `Backend/HotelManagement.BLL/DTOs/ConciergeDTOs.cs` — append new DTOs
2. `Backend/HotelManagement.BLL/Services/Concierge/ToolDefinitions.cs` — update `get_active_orders` args
3. `Backend/HotelManagement.BLL/Services/Concierge/ToolExecutor.cs` — dispatch updated args
4. `Backend/HotelManagement.BLL/Services/Concierge/ConciergeService.cs` — streaming impl + OTel metrics
5. `Backend/HotelManagement.BLL/Services/Concierge/PromptBuilder.cs` — no change (language detection implicit)

### New Files (Frontend)
1. **Test files in `Frontend/src/app/features/user/components/concierge-chat/`**:
   - `concierge-chat.component.streaming.spec.ts`
   - `concierge-chat.component.menu.spec.ts`
   - `concierge-chat.component.history.spec.ts`
   - `concierge-api.service.streaming.spec.ts`
2. **E2E tests in `Frontend/e2e/concierge/`**:
   - `streaming-chat.spec.ts`
   - `menu-carousel.spec.ts`
   - `order-history.spec.ts`
   - `language-auto.spec.ts`
3. **Load test in `Frontend/k6/`**:
   - `concierge-streaming.js`

### Modified Files (Frontend)
1. `Frontend/src/app/features/user/components/concierge-chat/concierge-chat.component.ts` — streaming state, `prefillOrder()`, parse menu items in response
2. `Frontend/src/app/features/user/components/concierge-chat/concierge-chat.component.html` — inline carousel render
3. `Frontend/src/app/features/user/components/concierge-chat/concierge-chat.component.scss` — carousel styles
4. `Frontend/src/app/features/user/services/concierge-api.service.ts` — `streamChat()` observable

---

## ⚖️ Assumptions & Decisions (Per AGENTS.md)

| # | Decision | Rationale |
|---|----------|-----------|
| 1 | SSE over SignalR for streaming | Simpler HTTP, works through any proxy/LB, no sticky sessions |
| 2 | Reuse existing `ConciergeTools` for new tools | No new schema; just add function definition |
| 3 | Language detected from message (not passed) | OpenAI handles 50+ languages natively |
| 4 | Menu images from existing `MenuItem.ImageUrl` | Verify exists; no new storage |
| 5 | OTel metrics emitted from BLL | Captures service logic latency |
| 6 | Carousel inline (no quantity picker) | Keeps UX simple; quantity confirmed in chat |
| 7 | Order history via status filter | Reuses existing tool, no new endpoint |
| 8 | Full testing suite (unit + integration + e2e + k6) in CI | Add tests now; run against fixed CI pipeline |

---

## ✅ Acceptance Criteria Checklist (Per AGENTS.md)

| # | Criterion | Done? |
|---|-----------|-------|
| 1 | SSE endpoint returns token deltas as `data: {...}\n\n` | ☐ |
| 2 | Frontend accumulates deltas, shows typing animation | ☐ |
| 3 | Final chunk includes proposals/actions, triggers confirmation UI | ☐ |
| 4 | Inline carousel renders image, name, price, tags, "Add to Order" | ☐ |
| 5 | Clicking carousel item pre-fills message input | ☐ |
| 6 | `get_active_orders` with `status=history` returns past orders | ☐ |
| 7 | OTel metrics: `concierge.chat.requests`, `concierge.tool.calls`, `concierge.proposals.*`, `concierge.latency.ms` | ☐ |
| 8 | All existing Phase 2 flows work (no regressions) | ☐ |
| 9 | Manual verification checklist passes | ☐ |

---

## ⚠️ Risk Assessment (Per AGENTS.md)

| Risk | Level | Mitigation |
|------|-------|------------|
| SSE connection drops on mobile/network change | Medium | Frontend auto-reconnect with `EventSource` retry; fallback to non-streaming endpoint |
| OpenAI streaming rate limits | Low | Existing `MaxToolCallsPerTurn=5`, token budget `maxTokens=1000` |
| Menu images missing/broken | Low | `loading="lazy"`, fallback placeholder, graceful degradation |
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
2. **3.1 SSE Streaming Backend** (3 hrs) — core feature, test with curl first
3. **3.1 SSE Streaming Frontend** (2 hrs) — integrate EventSource, token accumulation
4. **3.2 Inline Menu Render** (1 hr) — horizontal scroll in chat when `GetMenuItems` returns
5. **3.3 Extend `get_active_orders`** (30 min) — add `status` param, reuse in history context
6. **3.6 Manual Verification** (1 hr) — run checklist, verify no regressions

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

**End of Plan v2** — Ready for execution per AGENTS.md. Testing included as manual verification + future unit tests when CI fixed.