# AI Concierge Feature — Detailed Design Document

---

## 1. Executive Summary

The **AI Concierge** is a guest-facing conversational assistant embedded in the User Portal (and optionally the Public site). It goes beyond Q&A by **executing real actions** via existing BLL services — placing room-service orders, creating housekeeping/maintenance tickets, retrieving billing/booking info — all scoped to the authenticated guest's active booking through `ICurrentUserService`.

**Jaw-dropping demo**: Guest types one message → three departments (Kitchen, Housekeeping, Maintenance) instantly receive real-time SignalR alerts → AI replies with confirmation — all visible on projected staff dashboards.

---

## 2. Architecture Overview

```
┌──────────────────────────────────────────────────────────────────────────────┐
│                              USER PORTAL (Angular)                           │
│  ┌───────────────────┐    ┌───────────────────┐    ┌──────────────────────┐ │
│  │  AI Concierge      │───▶│  ConciergeApi     │    │  SignalR (receive    │ │
│  │  Chat Component    │    │  Service (JSON)   │    │   staff alerts)      │ │
│  └───────────────────┘    └───────────────────┘    └──────────────────────┘ │
└──────────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌──────────────────────────────────────────────────────────────────────────────┐
│                           API LAYER (ASP.NET Core)                            │
│  ┌───────────────────────────────────────────────────────────────────────┐   │
│  │  POST /api/v1/concierge/chat          — process message + proposals   │   │
│  │  POST /api/v1/concierge/confirm       — confirm proposed actions      │   │
│  │  GET  /api/v1/concierge/proposals     — get pending proposals         │   │
│  │  GET  /api/v1/concierge/context       — get guest context             │   │
│  │                                                                        │   │
│  │  • Auth: JWT (guest role)                                              │   │
│  │  • Input: { message, conversationId? }                                 │   │
│  │  • Output: JSON { reply, proposals[], actions[], isComplete }          │   │
│  │  • Inline regex sanitization at controller entry point                 │   │
│  └───────────────────────────────────────────────────────────────────────┘   │
└──────────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌──────────────────────────────────────────────────────────────────────────────┐
│                          BLL — CONCIERGE ORCHESTRATOR                         │
│  ┌───────────────────────────────────────────────────────────────────────┐   │
│  │  IConciergeService                                                     │   │
│  │  • ProcessMessageAsync(userMessage, conversationHistory, ctx)          │   │
│  │  • ConfirmProposalsAsync(proposalIds, conversationId)                  │   │
│  │  • GetPendingProposalsAsync(conversationId)                            │   │
│  │  • GetGuestContextAsync() → GuestContextDTO                            │   │
│  └───────────────────────────────────────────────────────────────────────┘   │
│                                    │                                         │
│         ┌────────────────────────────┼────────────────────────────┐         │
│         ▼                            ▼                            ▼         │
│  ┌───────────────┐           ┌───────────────┐           ┌───────────────┐  │
│  │ OrderService  │           │HousekeepingSvc│           │MaintenanceSvc │  │
│  │CreateOrderAsync│           │CreateGuestReq │           │CreateTicketAsy│  │
│  └───────────────┘           └───────────────┘           └───────────────┘  │
│                                    │                                         │
│  ┌───────────────┐           ┌───────────────┐           ┌───────────────┐  │
│  │ BookingService│           │ BillingService│           │  MenuItemRepo │  │
│  │GetBookingById │           │GenerateFolio  │           │ GetAvailable  │  │
│  └───────────────┘           └───────────────┘           └───────────────┘  │
│                                                                              │
│  ┌───────────────────────────────────────────────────────────────────────┐   │
│  │  NEW: IProposalStore (PostgreSQL) — pending proposals awaiting        │   │
│  │  user confirmation. Expire after 5 min TTL.                           │   │
│  └───────────────────────────────────────────────────────────────────────┘   │
│                                                                              │
│  ┌───────────────────────────────────────────────────────────────────────┐   │
│  │  NEW: IConversationStore (PostgreSQL) — conversation_messages table   │   │
│  │  Token-window retrieval (~6-8 recent messages).                       │   │
│  │  Key scoped: concierge:conv:{userId}:{conversationId}                 │   │
│  └───────────────────────────────────────────────────────────────────────┘   │
│                                                                              │
│  ┌───────────────────────────────────────────────────────────────────────┐   │
│  │  NEW: ConciergeActionLog entity — audit trail for every tool call,    │   │
│  │  stored in its own table (not extending AuditLog).                    │   │
│  └───────────────────────────────────────────────────────────────────────┘   │
└──────────────────────────────────────────────────────────────────────────────┘
```

### Key Architectural Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| **Streaming** | Dropped for MVP | Plain JSON request/response. SSE or SignalR streaming deferred to P3 |
| **Tool pattern** | Two-step (propose → confirm) | Side-effecting actions require explicit user confirmation within the chat |
| **Conversation store** | PostgreSQL (`conversation_messages` table) | Not in-memory. Token-window retrieval (~6-8 recent messages), store all historically |
| **Conversation key** | `concierge:conv:{userId}:{conversationId}` | Prevents cross-spilling — ensures user A cannot access user B's conversation |
| **Idempotency** | Per-turn via existing `[Idempotent]` + `IdempotentRequest` | Key format: `concierge:turn:{conversationId}:{turnNumber}` |
| **Sanitization** | Inline regex in controller | Strip `ignore previous instructions`, `system:`, `assistant:` patterns at entry point |
| **Audit logging** | New `ConciergeActionLog` table | Separate from `AuditLog` to avoid schema coupling; logs every tool call + outcome |
| **Model name** | From `_openAIOptions.Value.Model` | Config-driven, not hardcoded |

---

## 3. Data Models & DTOs

### 3.1 Request/Response (API Layer)

```csharp
// Backend/HotelManagement.BLL/DTOs/ConciergeDTOs.cs

public class ConciergeChatRequestDTO
{
    public string Message { get; set; } = string.Empty;
    public string? ConversationId { get; set; }
}

public class ConciergeChatResponseDTO
{
    public string Reply { get; set; } = string.Empty;
    public string ConversationId { get; set; } = string.Empty;
    public List<ConciergeProposalDTO> Proposals { get; set; } = new();   // actions needing confirmation
    public List<ConciergeActionResultDTO> Actions { get; set; } = new(); // auto-executed read-only results
    public bool IsComplete { get; set; } = true;
}

public class ConciergeConfirmRequestDTO
{
    [Required] public string ConversationId { get; set; } = string.Empty;
    [Required, MinLength(1)] public List<string> ProposalIds { get; set; } = new();
}

public class ConciergeProposalDTO
{
    public string ProposalId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;      // "create_food_order"
    public string Summary { get; set; } = string.Empty;     // Human-readable: "Order a Burger x1 and Fries x1"
    public string ArgumentsJson { get; set; } = "{}";
    public DateTime ExpiresAt { get; set; }
}

public class ConciergeActionResultDTO
{
    public string ToolCallId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;      // "food_order_created"
    public bool Success { get; set; }
    public string? ResultSummary { get; set; }              // "Order #42 placed: Burger ×1, Fries ×1"
    public string? Error { get; set; }
}
```

### 3.2 Tool Definitions (OpenAI Function Calling Schema)

```csharp
// Backend/HotelManagement.BLL/Services/Concierge/ToolDefinitions.cs
// Keep in sync with OpenAI function schema sent in chat completion requests

// Tools are split into two categories:
//   ReadOnly — executed immediately, results returned as Actions
//   SideEffect — returned as Proposals, executed only after user confirmation

public static class ConciergeTools
{
    public static readonly List<FunctionDefinition> Definitions = new()
    {
        // ── Side-Effect Tools (require user confirmation) ──
        new FunctionDefinition
        {
            Name = "create_food_order",
            Description = "Place a room-service order for the guest's active booking. Always confirm items & quantities with guest before calling.",
            Parameters = JsonSchema.FromType<CreateFoodOrderToolArgs>()
        },
        new FunctionDefinition
        {
            Name = "create_housekeeping_request",
            Description = "Request housekeeping (extra towels, cleaning, amenities)",
            Parameters = JsonSchema.FromType<CreateHousekeepingToolArgs>()
        },
        new FunctionDefinition
        {
            Name = "create_maintenance_ticket",
            Description = "Report a maintenance issue in the guest's room",
            Parameters = JsonSchema.FromType<CreateMaintenanceToolArgs>()
        },

        // ── Read-Only Tools (execute immediately) ──
        new FunctionDefinition
        {
            Name = "get_booking_info",
            Description = "Retrieve current booking details (check-in/out, room, status)",
            Parameters = JsonSchema.FromType<EmptyArgs>()
        },
        new FunctionDefinition
        {
            Name = "get_folio_balance",
            Description = "Get current folio/billing balance for the stay",
            Parameters = JsonSchema.FromType<EmptyArgs>()
        },
        new FunctionDefinition
        {
            Name = "get_housekeeping_status",
            Description = "Check if room has been cleaned / status of housekeeping requests",
            Parameters = JsonSchema.FromType<EmptyArgs>()
        },
        new FunctionDefinition
        {
            Name = "get_menu_items",
            Description = "Browse available menu items (filter by category, availability)",
            Parameters = JsonSchema.FromType<GetMenuItemsToolArgs>()
        },
        new FunctionDefinition
        {
            Name = "get_active_orders",
            Description = "List guest's current/pending room-service orders",
            Parameters = JsonSchema.FromType<EmptyArgs>()
        }
    };

    public static readonly HashSet<string> SideEffectToolNames = new()
    {
        "create_food_order", "create_housekeeping_request", "create_maintenance_ticket"
    };

    public const int MaxToolCallsPerTurn = 5;
}

// Tool Argument Types
public class CreateFoodOrderToolArgs
{
    [JsonPropertyName("items")]
    [Required, MinLength(1), MaxLength(20)]
    public List<FoodOrderItemToolArg> Items { get; set; } = new();
    // bookingId & roomId resolved from guest context — NOT exposed to LLM
}

public class FoodOrderItemToolArg
{
    [JsonPropertyName("menuItemId")] [Required] public int MenuItemId { get; set; }
    [JsonPropertyName("quantity")]   [Required, Range(1, 20)] public int Quantity { get; set; }
}

public class CreateHousekeepingToolArgs
{
    [JsonPropertyName("description")] [Required, MaxLength(500)] public string Description { get; set; } = "";
    [JsonPropertyName("isEmergency")] public bool IsEmergency { get; set; } = false;
}

public class CreateMaintenanceToolArgs
{
    [JsonPropertyName("description")] [Required, MaxLength(500)] public string Description { get; set; } = "";
    [JsonPropertyName("isEmergency")] public bool IsEmergency { get; set; } = false;
}

public class GetMenuItemsToolArgs
{
    [JsonPropertyName("category")] public string? Category { get; set; }
    [JsonPropertyName("search")] public string? Search { get; set; }
    [JsonPropertyName("availableOnly")] public bool AvailableOnly { get; set; } = true;
}

public class EmptyArgs { }
```

### 3.3 Audit Log Entity

```csharp
// HotelManagement.DAL/Entities/ConciergeActionLog.cs

public class ConciergeActionLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int UserId { get; set; }
    public string ConversationId { get; set; } = string.Empty;
    public string UserMessage { get; set; } = string.Empty;
    public string ToolName { get; set; } = string.Empty;
    public string ArgumentsJson { get; set; } = "{}";
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

### 3.4 Conversation Persistence Schema

```sql
-- PostgreSQL migration
CREATE TABLE conversation_messages (
    id              BIGSERIAL PRIMARY KEY,
    user_id         INTEGER NOT NULL REFERENCES users(id),
    conversation_id TEXT NOT NULL,
    role            TEXT NOT NULL,   -- 'user' | 'assistant' | 'tool'
    content         TEXT NOT NULL,
    metadata_json   TEXT,            -- tool call IDs, proposal IDs, etc.
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_conv_messages_lookup
    ON conversation_messages(user_id, conversation_id, created_at);
```

### 3.5 Proposal Store Schema (PostgreSQL)

```sql
CREATE TABLE concierge_proposals (
    id              UUID PRIMARY KEY,
    conversation_id TEXT NOT NULL,
    user_id         INTEGER NOT NULL REFERENCES users(id),
    tool_name       TEXT NOT NULL,
    arguments_json  TEXT NOT NULL,
    summary         TEXT NOT NULL,
    status          TEXT NOT NULL DEFAULT 'pending',  -- pending | confirmed | expired
    expires_at      TIMESTAMPTZ NOT NULL,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    confirmed_at    TIMESTAMPTZ
);

CREATE INDEX idx_proposals_pending
    ON concierge_proposals(user_id, conversation_id, status)
    WHERE status = 'pending';
```

---

## 4. Backend Implementation

### 4.1 New Project Structure

```
HotelManagement.BLL/
├── Services/
│   ├── Concierge/
│   │   ├── IConciergeService.cs
│   │   ├── ConciergeService.cs
│   │   ├── ToolExecutor.cs
│   │   ├── ToolDefinitions.cs
│   │   ├── PromptBuilder.cs
│   │   ├── IConversationStore.cs
│   │   ├── IProposalStore.cs
│   │   └── InputSanitizer.cs        (static helper, inline regex)
│   └── (existing services...)
├── DTOs/
│   └── ConciergeDTOs.cs
└── Interfaces/
    └── IConciergeService.cs

HotelManagement.DAL/
├── Entities/
│   └── ConciergeActionLog.cs
└── Configurations/
    └── ConciergeActionLogConfiguration.cs
```

### 4.2 Core Interface

```csharp
// HotelManagement.BLL/Interfaces/IConciergeService.cs
using HotelManagement.BLL.DTOs;

namespace HotelManagement.BLL.Interfaces;

public interface IConciergeService
{
    Task<ConciergeChatResponseDTO> ProcessMessageAsync(
        string userMessage,
        string? conversationId = null,
        CancellationToken ct = default);

    Task<ConciergeChatResponseDTO> ConfirmProposalsAsync(
        string conversationId,
        List<string> proposalIds,
        CancellationToken ct = default);

    Task<List<ConciergeProposalDTO>> GetPendingProposalsAsync(
        string conversationId,
        CancellationToken ct = default);

    Task<GuestContextDTO> GetGuestContextAsync(CancellationToken ct = default);
}

public class GuestContextDTO
{
    public int? BookingId { get; set; }
    public int? RoomId { get; set; }
    public string? RoomNumber { get; set; }
    public int UserId { get; set; }
    public DateTime? CheckInDate { get; set; }
    public DateTime? CheckOutDate { get; set; }
    public BookingStatus BookingStatus { get; set; }
    public List<MenuItemSummaryDTO> RecentOrders { get; set; } = new();
    public List<GuestPreferenceDTO> Preferences { get; set; } = new();
}
```

### 4.3 ConciergeService — Orchestration Logic

```csharp
// HotelManagement.BLL/Services/Concierge/ConciergeService.cs
using System.Text.Json;
using HotelManagement.BLL.DTOs;
using HotelManagement.BLL.Interfaces;
using HotelManagement.DAL.Entities;
using HotelManagement.DAL.Enums;
using Microsoft.Extensions.Options;
using OpenAI.Chat;

namespace HotelManagement.BLL.Services.Concierge;

public class ConciergeService : IConciergeService
{
    private readonly ICurrentUserService _currentUser;
    private readonly IBookingService _bookingService;
    private readonly IOrderService _orderService;
    private readonly IHousekeepingService _housekeepingService;
    private readonly IMaintenanceService _maintenanceService;
    private readonly IBillingService _billingService;
    private readonly IMenuItemRepository _menuItemRepository;
    private readonly IFoodOrderRepository _foodOrderRepository;
    private readonly IConversationStore _conversations;
    private readonly IProposalStore _proposalStore;
    private readonly IConciergeActionLogRepository _auditLog;
    private readonly IOptions<OpenAIOptions> _openAIOptions;
    private readonly ILogger<ConciergeService> _logger;

    private readonly ChatClient _chatClient;
    private readonly List<ChatTool> _tools;

    public ConciergeService(
        ICurrentUserService currentUser,
        IBookingService bookingService,
        IOrderService orderService,
        IHousekeepingService housekeepingService,
        IMaintenanceService maintenanceService,
        IBillingService billingService,
        IMenuItemRepository menuItemRepository,
        IFoodOrderRepository foodOrderRepository,
        IConversationStore conversations,
        IProposalStore proposalStore,
        IConciergeActionLogRepository auditLog,
        IOptions<OpenAIOptions> openAIOptions,
        ILogger<ConciergeService> logger)
    {
        _currentUser = currentUser;
        _bookingService = bookingService;
        _orderService = orderService;
        _housekeepingService = housekeepingService;
        _maintenanceService = maintenanceService;
        _billingService = billingService;
        _menuItemRepository = menuItemRepository;
        _foodOrderRepository = foodOrderRepository;
        _conversations = conversations;
        _proposalStore = proposalStore;
        _auditLog = auditLog;
        _openAIOptions = openAIOptions;
        _logger = logger;

        _chatClient = new ChatClient(_openAIOptions.Value.Model, _openAIOptions.Value.ApiKey);
        _tools = BuildTools();
    }

    public async Task<ConciergeChatResponseDTO> ProcessMessageAsync(
        string userMessage, string? conversationId, CancellationToken ct)
    {
        var userId = _currentUser.GetUserId();

        // 1. Resolve or create conversation (key scoped to user)
        var convId = conversationId ?? Guid.NewGuid().ToString();
        var convKey = $"concierge:conv:{userId}:{convId}";
        var history = await _conversations.GetAsync(convKey, userId, ct);

        // 2. Build guest context (active booking, room, preferences)
        var context = await BuildGuestContextAsync(ct);

        // 3. Build system prompt with context + tool definitions
        var messages = PromptBuilder.BuildMessages(context, history, userMessage);

        // 4. Call LLM with tools
        var completion = await _chatClient.CompleteChatAsync(messages, new ChatCompletionOptions
        {
            Tools = _tools,
            ToolChoice = ChatToolChoice.Auto,
            Temperature = 0.3f,
            MaxTokens = 1000
        }, ct);

        var response = completion.Value;

        // 5. Handle tool calls
        var toolCalls = response.ToolCalls.ToList();

        if (toolCalls.Count > ConciergeTools.MaxToolCallsPerTurn)
        {
            // Hard cap — truncate to max allowed
            toolCalls = toolCalls.Take(ConciergeTools.MaxToolCallsPerTurn).ToList();
        }

        var proposals = new List<ConciergeProposalDTO>();
        var actions = new List<ConciergeActionResultDTO>();

        foreach (var call in toolCalls)
        {
            if (ConciergeTools.SideEffectToolNames.Contains(call.FunctionName))
            {
                // Step 1: Create a proposal (pending confirmation)
                var proposal = await CreateProposalAsync(convId, call, context, ct);
                proposals.Add(proposal);
            }
            else
            {
                // Read-only: execute immediately
                var result = await ToolExecutor.ExecuteAsync(call, context, this, ct);
                actions.Add(result);
                await LogActionAsync(convId, context.UserId, userMessage, call, result, ct);
            }
        }

        // 6. Feed results back to LLM for final natural-language reply
        var finalMessages = new List<ChatMessage>(messages);

        // Add tool/proposal summaries for the LLM to respond to
        var summaryParts = new List<string>();
        if (proposals.Any())
        {
            summaryParts.Add($"Proposals created (pending confirmation): {string.Join(", ", proposals.Select(p => $"{p.Action}: {p.Summary}"))}. Tell the user what you're proposing and ask them to confirm.");
        }
        if (actions.Any())
        {
            foreach (var action in actions)
            {
                summaryParts.Add($"Executed: {(action.Success ? "OK" : "FAIL")} — {action.ResultSummary ?? action.Error}");
            }
        }
        if (summaryParts.Any())
        {
            finalMessages.Add(new ChatMessage(ChatMessageRole.System, string.Join("\n", summaryParts)));
        }

        var finalCompletion = await _chatClient.CompleteChatAsync(finalMessages, ct: ct);
        var finalReply = finalCompletion.Value.Content[0].Text;

        // 7. Persist conversation
        await _conversations.AppendAsync(convKey, userId, userMessage, finalReply, ct);

        return new ConciergeChatResponseDTO
        {
            Reply = finalReply,
            ConversationId = convId,
            Proposals = proposals,
            Actions = actions,
            IsComplete = true
        };
    }

    public async Task<ConciergeChatResponseDTO> ConfirmProposalsAsync(
        string conversationId, List<string> proposalIds, CancellationToken ct)
    {
        var userId = _currentUser.GetUserId();

        // 1. Fetch and validate proposals
        var proposals = await _proposalStore.GetByIdsAsync(proposalIds, userId, conversationId, ct);
        var invalid = proposals.Where(p => p.Status != "pending" || p.ExpiresAt < DateTime.UtcNow).ToList();
        if (invalid.Any())
        {
            return new ConciergeChatResponseDTO
            {
                Reply = $"Some proposals have expired or are no longer valid. Please try again.",
                ConversationId = conversationId,
                IsComplete = false
            };
        }

        // 2. Validate tool arguments before execution
        var actions = new List<ConciergeActionResultDTO>();
        foreach (var proposal in proposals)
        {
            var validationError = await ValidateToolArgsAsync(proposal.ToolName, proposal.ArgumentsJson, ct);
            if (validationError != null)
            {
                actions.Add(new ConciergeActionResultDTO
                {
                    Action = proposal.ToolName,
                    Success = false,
                    Error = validationError
                });
                continue;
            }

            var context = await BuildGuestContextAsync(ct);
            var call = new ChatToolCall(proposal.Id.ToString(), proposal.ToolName, BinaryData.FromString(proposal.ArgumentsJson));
            var result = await ToolExecutor.ExecuteAsync(call, context, this, ct);
            actions.Add(result);

            await LogActionAsync(conversationId, userId, "(confirmed)", call, result, ct);
        }

        // 3. Mark proposals as confirmed
        await _proposalStore.MarkConfirmedAsync(proposalIds, userId, conversationId, ct);

        // 4. Get LLM summary of results
        var convKey = $"concierge:conv:{userId}:{conversationId}";
        var summaryParts = actions.Select(a => $"{(a.Success ? "OK" : "FAIL")}: {a.ResultSummary ?? a.Error}");
        var finalMessages = new List<ChatMessage>
        {
            new ChatMessage(ChatMessageRole.System, $"The following actions were executed:\n{string.Join("\n", summaryParts)}\n\nSummarize what was accomplished in a warm, friendly way.")
        };
        var completion = await _chatClient.CompleteChatAsync(finalMessages, ct: ct);
        var reply = completion.Value.Content[0].Text;

        await _conversations.AppendAsync(convKey, userId, "Confirmed proposals", reply, ct);

        return new ConciergeChatResponseDTO
        {
            Reply = reply,
            ConversationId = conversationId,
            Actions = actions,
            IsComplete = true
        };
    }

    // ── Tool Implementation Methods ──

    public async Task<ConciergeActionResultDTO> CreateFoodOrderAsync(CreateFoodOrderToolArgs args, GuestContextDTO ctx, CancellationToken ct)
    {
        if (ctx.BookingId == null || ctx.RoomId == null)
            return Fail("No active booking found. Please check in first.");

        if (ctx.BookingStatus != BookingStatus.CheckedIn)
            return Fail("Room service is only available for checked-in guests.");

        // Validate menu items exist and are available
        foreach (var item in args.Items)
        {
            var menuItem = await _menuItemRepository.GetByIdAsync(item.MenuItemId);
            if (menuItem == null)
                return Fail($"Menu item #{item.MenuItemId} not found.");
            if (!menuItem.IsAvailable)
                return Fail($"'{menuItem.Name}' is currently unavailable.");
        }

        var dto = new CreateFoodOrderDTO
        {
            BookingId = ctx.BookingId.Value,
            RoomId = ctx.RoomId.Value,
            Items = args.Items.Select(i => new CreateFoodOrderItemDTO
            {
                MenuItemId = i.MenuItemId,
                Quantity = i.Quantity
            }).ToList()
        };

        var result = await _orderService.CreateOrderAsync(dto);
        return Success($"Order #{result.Id} placed: {string.Join(", ", result.OrderItems.Select(i => $"{i.MenuItemName}×{i.Quantity}"))}");
    }

    public async Task<ConciergeActionResultDTO> CreateHousekeepingRequestAsync(CreateHousekeepingToolArgs args, GuestContextDTO ctx, CancellationToken ct)
    {
        if (ctx.RoomId == null) return Fail("No active room assignment.");

        var dto = new CreateHousekeepingTaskDTO { Description = args.Description, IsEmergency = args.IsEmergency };
        var result = await _housekeepingService.CreateGuestTriggerAsync(ctx.RoomId.Value, dto);
        return Success($"Housekeeping request created: {args.Description}");
    }

    public async Task<ConciergeActionResultDTO> CreateMaintenanceTicketAsync(CreateMaintenanceToolArgs args, GuestContextDTO ctx, CancellationToken ct)
    {
        if (ctx.RoomId == null) return Fail("No active room assignment.");

        var dto = new CreateMaintenanceTaskDTO { Description = args.Description, IsEmergency = args.IsEmergency };
        var result = await _maintenanceService.CreateTicketAsync(ctx.RoomId.Value, dto);
        return Success($"Maintenance ticket created: {args.Description}");
    }

    public async Task<ConciergeActionResultDTO> GetBookingInfoAsync(GuestContextDTO ctx, CancellationToken ct)
    {
        if (ctx.BookingId == null) return Fail("No active booking.");
        var booking = await _bookingService.GetBookingByIdAsync(ctx.BookingId.Value);
        var json = JsonSerializer.Serialize(new
        {
            booking.Id,
            booking.CheckInDate,
            booking.CheckOutDate,
            booking.BookingStatus,
            RoomNumber = ctx.RoomNumber,
            booking.GuestName
        });
        return Success(json);
    }

    public async Task<ConciergeActionResultDTO> GetFolioBalanceAsync(GuestContextDTO ctx, CancellationToken ct)
    {
        if (ctx.BookingId == null) return Fail("No active booking.");
        var folio = await _billingService.GenerateFolioAsync(ctx.BookingId.Value);
        var json = JsonSerializer.Serialize(new { folio.TotalBill, folio.PaymentStatus, folio.NightsStayed });
        return Success(json);
    }

    public async Task<ConciergeActionResultDTO> GetHousekeepingStatusAsync(GuestContextDTO ctx, CancellationToken ct)
    {
        if (ctx.RoomId == null) return Fail("No active room.");
        var tasks = await _housekeepingRepository.FindAsync(h => h.RoomId == ctx.RoomId && h.Status != HousekeepingStatus.Completed);
        var json = JsonSerializer.Serialize(tasks.Select(t => new { t.Id, t.Description, t.Status, t.CreatedAt }));
        return Success(json);
    }

    public async Task<ConciergeActionResultDTO> GetMenuItemsAsync(GetMenuItemsToolArgs args, CancellationToken ct)
    {
        var items = await _menuItemRepository.GetPaginatedMenuItemsAsync(1, 50, args.AvailableOnly, args.Category, args.Search);
        var json = JsonSerializer.Serialize(items.Data.Select(i => new { i.Id, i.Name, i.Description, i.Price, i.Category, i.IsAvailable }));
        return Success(json);
    }

    public async Task<ConciergeActionResultDTO> GetActiveOrdersAsync(GuestContextDTO ctx, CancellationToken ct)
    {
        if (ctx.BookingId == null) return Fail("No active booking.");
        var orders = await _orderService.GetActiveOrdersAsync(1, 20, ctx.BookingId.Value);
        var json = JsonSerializer.Serialize(orders.Data.Select(o => new { o.Id, o.OrderStatus, o.GeneratedAt, Items = o.OrderItems.Select(i => new { i.MenuItemName, i.Quantity, i.PriceAtPurchase }) }));
        return Success(json);
    }

    // ── Private Helpers ──

    private async Task<ConciergeProposalDTO> CreateProposalAsync(string convId, ChatToolCall call, GuestContextDTO ctx, CancellationToken ct)
    {
        var args = call.FunctionArguments.ToString();
        var summary = call.FunctionName switch
        {
            "create_food_order" => await SummarizeFoodOrderAsync(args, ct),
            "create_housekeeping_request" => await SummarizeHousekeepingAsync(args, ct),
            "create_maintenance_ticket" => await SummarizeMaintenanceAsync(args, ct),
            _ => $"Execute {call.FunctionName}"
        };

        var proposal = new ConciergeProposalDTO
        {
            ProposalId = Guid.NewGuid().ToString(),
            Action = call.FunctionName,
            Summary = summary,
            ArgumentsJson = args,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };

        await _proposalStore.SaveAsync(proposal, ctx.UserId, convId, ct);
        return proposal;
    }

    private async Task<string> SummarizeFoodOrderAsync(string argsJson, CancellationToken ct)
    {
        try
        {
            var args = JsonSerializer.Deserialize<CreateFoodOrderToolArgs>(argsJson);
            if (args?.Items == null || !args.Items.Any()) return "Order items";

            var descriptions = new List<string>();
            foreach (var item in args.Items)
            {
                var menuItem = await _menuItemRepository.GetByIdAsync(item.MenuItemId);
                var name = menuItem?.Name ?? $"Item #{item.MenuItemId}";
                descriptions.Add($"{name} ×{item.Quantity}");
            }
            return $"Order: {string.Join(", ", descriptions)}";
        }
        catch
        {
            return "Order items";
        }
    }

    private Task<string> SummarizeHousekeepingAsync(string argsJson, CancellationToken ct)
    {
        try
        {
            var args = JsonSerializer.Deserialize<CreateHousekeepingToolArgs>(argsJson);
            if (args == null) return Task.FromResult("Housekeeping request");
            var prefix = args.IsEmergency ? "URGENT: " : "";
            return Task.FromResult($"{prefix}{args.Description}");
        }
        catch
        {
            return Task.FromResult("Housekeeping request");
        }
    }

    private Task<string> SummarizeMaintenanceAsync(string argsJson, CancellationToken ct)
    {
        try
        {
            var args = JsonSerializer.Deserialize<CreateMaintenanceToolArgs>(argsJson);
            if (args == null) return Task.FromResult("Maintenance ticket");
            var prefix = args.IsEmergency ? "URGENT: " : "";
            return Task.FromResult($"{prefix}{args.Description}");
        }
        catch
        {
            return Task.FromResult("Maintenance ticket");
        }
    }

    private async Task<string?> ValidateToolArgsAsync(string toolName, string argsJson, CancellationToken ct)
    {
        try
        {
            return toolName switch
            {
                "create_food_order" => await ValidateFoodOrderArgsAsync(argsJson, ct),
                _ => null
            };
        }
        catch (JsonException)
        {
            return $"Invalid arguments for {toolName}";
        }
    }

    private async Task<string?> ValidateFoodOrderArgsAsync(string argsJson, CancellationToken ct)
    {
        var args = JsonSerializer.Deserialize<CreateFoodOrderToolArgs>(argsJson);
        if (args == null) return "Invalid food order arguments.";

        foreach (var item in args.Items)
        {
            var menuItem = await _menuItemRepository.GetByIdAsync(item.MenuItemId);
            if (menuItem == null)
                return $"Menu item #{item.MenuItemId} not found.";
            if (!menuItem.IsAvailable)
                return $"'{menuItem.Name}' is currently unavailable.";
        }

        return null;
    }

    private async Task LogActionAsync(string conversationId, int userId, string userMessage, ChatToolCall call, ConciergeActionResultDTO result, CancellationToken ct)
    {
        var log = new ConciergeActionLog
        {
            UserId = userId,
            ConversationId = conversationId,
            UserMessage = userMessage,
            ToolName = call.FunctionName,
            ArgumentsJson = call.FunctionArguments.ToString(),
            Success = result.Success,
            ErrorMessage = result.Error
        };

        await _auditLog.AddAsync(log, ct);
    }

    private async Task<GuestContextDTO> BuildGuestContextAsync(CancellationToken ct)
    {
        var email = _currentUser.GetUserEmail();
        if (string.IsNullOrEmpty(email)) return new GuestContextDTO();

        var userId = _currentUser.GetUserId();
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null) return new GuestContextDTO();

        var today = DateTime.UtcNow.Date;
        var bookings = await _bookingRepository.GetPaginatedBookingsWithDetailsAsync(1, 5, new List<Expression<Func<Booking, bool>>>
        {
            b => b.UserId == user.Id &&
                 (b.BookingStatus == BookingStatus.CheckedIn ||
                  (b.BookingStatus == BookingStatus.Booked && b.CheckInDate.Date == today))
        });

        var active = bookings.Data.FirstOrDefault();
        if (active == null) return new GuestContextDTO { UserId = userId };

        var roomId = active.BookingRooms.FirstOrDefault(br => br.RoomId.HasValue)?.RoomId;
        var roomNumber = active.BookingRooms.FirstOrDefault(br => br.RoomId.HasValue)?.Room?.RoomNumber;

        return new GuestContextDTO
        {
            UserId = userId,
            BookingId = active.Id,
            RoomId = roomId,
            RoomNumber = roomNumber,
            CheckInDate = active.CheckInDate,
            CheckOutDate = active.CheckOutDate,
            BookingStatus = active.BookingStatus
        };
    }

    private ConciergeActionResultDTO Success(string summary) => new() { Success = true, ResultSummary = summary };
    private ConciergeActionResultDTO Fail(string error) => new() { Success = false, Error = error };
}
```

### 4.4 ToolExecutor — Centralized Tool Dispatch

```csharp
// HotelManagement.BLL/Services/Concierge/ToolExecutor.cs
public static class ToolExecutor
{
    public static async Task<ConciergeActionResultDTO> ExecuteAsync(
        ChatToolCall toolCall, GuestContextDTO ctx, IConciergeService service, CancellationToken ct)
    {
        try
        {
            return toolCall.FunctionName switch
            {
                "create_food_order" => await service.CreateFoodOrderAsync(
                    JsonSerializer.Deserialize<CreateFoodOrderToolArgs>(toolCall.FunctionArguments)!, ctx, ct),

                "create_housekeeping_request" => await service.CreateHousekeepingRequestAsync(
                    JsonSerializer.Deserialize<CreateHousekeepingToolArgs>(toolCall.FunctionArguments)!, ctx, ct),

                "create_maintenance_ticket" => await service.CreateMaintenanceTicketAsync(
                    JsonSerializer.Deserialize<CreateMaintenanceToolArgs>(toolCall.FunctionArguments)!, ctx, ct),

                "get_booking_info" => await service.GetBookingInfoAsync(ctx, ct),
                "get_folio_balance" => await service.GetFolioBalanceAsync(ctx, ct),
                "get_housekeeping_status" => await service.GetHousekeepingStatusAsync(ctx, ct),
                "get_menu_items" => await service.GetMenuItemsAsync(
                    JsonSerializer.Deserialize<GetMenuItemsToolArgs>(toolCall.FunctionArguments) ?? new(), ct),
                "get_active_orders" => await service.GetActiveOrdersAsync(ctx, ct),

                _ => new ConciergeActionResultDTO { Success = false, Error = $"Unknown tool: {toolCall.FunctionName}" }
            };
        }
        catch (Exception ex)
        {
            return new ConciergeActionResultDTO { Success = false, Error = ex.Message };
        }
    }
}
```

### 4.5 PromptBuilder — System Prompt Construction

```csharp
// HotelManagement.BLL/Services/Concierge/PromptBuilder.cs
public static class PromptBuilder
{
    public static List<ChatMessage> BuildMessages(GuestContextDTO ctx, List<ConversationTurn> history, string userMessage)
    {
        var messages = new List<ChatMessage>
        {
            new ChatMessage(ChatMessageRole.System, BuildSystemPrompt(ctx))
        };

        // Add conversation history (last 8 turns — token-window)
        foreach (var turn in history.TakeLast(8))
        {
            messages.Add(new ChatMessage(ChatMessageRole.User, turn.UserMessage));
            messages.Add(new ChatMessage(ChatMessageRole.Assistant, turn.AssistantReply));
        }

        messages.Add(new ChatMessage(ChatMessageRole.User, userMessage));
        return messages;
    }

    private static string BuildSystemPrompt(GuestContextDTO ctx)
    {
        var sb = new StringBuilder();
        sb.AppendLine("You are the AI Concierge for a luxury hotel. You help guests with their stay.");
        sb.AppendLine("You can perform actions by calling tools. Always be warm, professional, and concise.");
        sb.AppendLine();

        if (ctx.BookingId.HasValue)
        {
            sb.AppendLine($"--- GUEST CONTEXT ---");
            sb.AppendLine($"Booking: #{ctx.BookingId}");
            sb.AppendLine($"Room: {ctx.RoomNumber ?? "Unassigned"}");
            sb.AppendLine($"Stay: {ctx.CheckInDate:MMM dd} – {ctx.CheckOutDate:MMM dd}");
            sb.AppendLine($"Status: {ctx.BookingStatus}");
            sb.AppendLine();
        }
        else
        {
            sb.AppendLine("--- GUEST CONTEXT ---");
            sb.AppendLine("No active booking found. Guest may not be checked in.");
            sb.AppendLine();
        }

        sb.AppendLine("--- TOOL USAGE GUIDELINES ---");
        sb.AppendLine("Side-effect tools (require user confirmation before executing):");
        sb.AppendLine("• create_food_order: Place room-service orders. Requires guest to be checked in.");
        sb.AppendLine("• create_housekeeping_request: Extra towels, cleaning, amenities, etc.");
        sb.AppendLine("• create_maintenance_ticket: Broken AC, leaky faucet, TV issues, etc. Use isEmergency=true for urgent safety issues.");
        sb.AppendLine();
        sb.AppendLine("Read-only tools (execute immediately):");
        sb.AppendLine("• get_booking_info: Answer questions about check-in/out times, room number, stay dates.");
        sb.AppendLine("• get_folio_balance: Current bill total, payment status.");
        sb.AppendLine("• get_housekeeping_status: Has room been cleaned? Any pending requests?");
        sb.AppendLine("• get_menu_items: Browse menu. Supports category filter (breakfast, lunch, dinner, drinks, snacks).");
        sb.AppendLine("• get_active_orders: Show pending/delivered room-service orders.");
        sb.AppendLine();
        sb.AppendLine("--- TWO-STEP ACTION PATTERN ---");
        sb.AppendLine("For side-effect tools you MUST follow this exact pattern:");
        sb.AppendLine("1. Call the tool with the guest's request details.");
        sb.AppendLine("2. The system will create a proposal. Tell the guest what you're proposing.");
        sb.AppendLine("3. Ask the guest to confirm by saying something like 'yes', 'confirm', or 'go ahead'.");
        sb.AppendLine("4. Once confirmed, the action will be executed and the guest will see the result.");
        sb.AppendLine();
        sb.AppendLine("--- RULES ---");
        sb.AppendLine("1. NEVER ask for booking ID, room number, or guest name — you have them from context.");
        sb.AppendLine("2. If guest is not checked in, politely explain what's available (pre-arrival questions, booking info).");
        sb.AppendLine("3. For food orders: confirm items & quantities before calling tool. Mention prices.");
        sb.AppendLine("4. For maintenance: if safety issue (fire, flood, gas), set isEmergency=true and tell guest help is coming immediately.");
        sb.AppendLine("5. Keep replies under 3 sentences unless explaining menu or folio details.");
        sb.AppendLine("6. Max 5 tool calls per turn. If the guest makes more requests, prioritize the most important ones.");
        sb.AppendLine("7. NEVER include bookingId, roomId, or userId in tool arguments — they come from context.");

        return sb.ToString();
    }
}
```

### 4.6 Input Sanitizer (Inline)

```csharp
// HotelManagement.BLL/Services/Concierge/InputSanitizer.cs
public static class InputSanitizer
{
    // ponytail: inline regex approach, no dedicated service for MVP
    private static readonly string[] BlockedPatterns =
    {
        @"(?i)ignore\s+(all\s+)?previous\s+instructions",
        @"(?i)system\s*:",
        @"(?i)assistant\s*:",
        @"(?i)you\s+are\s+(not\s+)?(a\s+)?(concierge|assistant)",
        @"(?i)forget\s+(everything|all)",
    };

    public static string Sanitize(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return input;

        var result = input;
        foreach (var pattern in BlockedPatterns)
        {
            result = Regex.Replace(result, pattern, "[REDACTED]", RegexOptions.Multiline);
        }

        return result.Trim();
    }
}
```

### 4.7 Conversation Store (PostgreSQL)

```csharp
// HotelManagement.BLL/Services/Concierge/IConversationStore.cs
public interface IConversationStore
{
    Task<List<ConversationTurn>> GetAsync(string scopedKey, int userId, CancellationToken ct);
    Task AppendAsync(string scopedKey, int userId, string userMsg, string assistantMsg, CancellationToken ct);
}

public class ConversationTurn
{
    public string UserMessage { get; set; } = "";
    public string AssistantReply { get; set; } = "";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

// PostgreSQL-backed implementation
public class PostgresConversationStore : IConversationStore
{
    private readonly DbContext _db;

    public PostgresConversationStore(DbContext db) => _db = db;

    public async Task<List<ConversationTurn>> GetAsync(string scopedKey, int userId, CancellationToken ct)
    {
        // Token-window: fetch last 8 messages
        var messages = await _db.Set<ConversationMessage>()
            .Where(m => m.ConversationId == scopedKey && m.UserId == userId)
            .OrderByDescending(m => m.CreatedAt)
            .Take(8)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(ct);

        var turns = new List<ConversationTurn>();
        for (int i = 0; i < messages.Count; i += 2)
        {
            if (i + 1 < messages.Count && messages[i].Role == "user" && messages[i + 1].Role == "assistant")
            {
                turns.Add(new ConversationTurn
                {
                    UserMessage = messages[i].Content,
                    AssistantReply = messages[i + 1].Content,
                    Timestamp = messages[i].CreatedAt
                });
            }
        }
        return turns;
    }

    public async Task AppendAsync(string scopedKey, int userId, string userMsg, string assistantMsg, CancellationToken ct)
    {
        _db.Set<ConversationMessage>().AddRange(
            new ConversationMessage
            {
                UserId = userId,
                ConversationId = scopedKey,
                Role = "user",
                Content = userMsg,
                CreatedAt = DateTime.UtcNow
            },
            new ConversationMessage
            {
                UserId = userId,
                ConversationId = scopedKey,
                Role = "assistant",
                Content = assistantMsg,
                CreatedAt = DateTime.UtcNow
            });
        await _db.SaveChangesAsync(ct);
    }
}
```

### 4.8 Proposal Store

```csharp
// HotelManagement.BLL/Services/Concierge/IProposalStore.cs
public interface IProposalStore
{
    Task SaveAsync(ConciergeProposalDTO proposal, int userId, string conversationId, CancellationToken ct);
    Task<List<ConciergeProposalDTO>> GetByIdsAsync(List<string> ids, int userId, string conversationId, CancellationToken ct);
    Task MarkConfirmedAsync(List<string> ids, int userId, string conversationId, CancellationToken ct);
    Task CleanupExpiredAsync(CancellationToken ct);
}
```

### 4.9 OpenAI Options & DI Registration

```csharp
// HotelManagement.BLL/Options/OpenAIOptions.cs
public class OpenAIOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gpt-4o-mini";
}
```

```csharp
// HotelManagement.BLL/DependencyInjection.cs
public static class BllServiceCollectionExtensions
{
    public static IServiceCollection AddBllServices(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<OpenAIOptions>(config.GetSection("OpenAI"));
        services.AddScoped<IConversationStore, PostgresConversationStore>();
        services.AddScoped<IProposalStore, PostgresProposalStore>();
        services.AddScoped<IConciergeActionLogRepository, ConciergeActionLogRepository>();
        services.AddScoped<IConciergeService, ConciergeService>();

        return services;
    }
}
```

---

## 5. API Controller

```csharp
// HotelManagement.API/Controllers/ConciergeController.cs
[ApiController]
[Route("api/v1/concierge")]
[Authorize(Roles = "RegisteredUser")]  // Only authenticated guests
public class ConciergeController : ControllerBase
{
    private readonly IConciergeService _concierge;

    public ConciergeController(IConciergeService concierge) => _concierge = concierge;

    [HttpPost("chat")]
    [Idempotent(KeyPrefix = "concierge:turn")]  // per-turn idempotency
    public async Task<ActionResult<ConciergeChatResponseDTO>> Chat(
        [FromBody] ConciergeChatRequestDTO request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest("Message is required.");

        // Inline sanitization
        var sanitized = InputSanitizer.Sanitize(request.Message);

        var response = await _concierge.ProcessMessageAsync(
            sanitized, request.ConversationId, ct);

        return Ok(response);
    }

    [HttpPost("confirm")]
    [Idempotent(KeyPrefix = "concierge:confirm")]
    public async Task<ActionResult<ConciergeChatResponseDTO>> Confirm(
        [FromBody] ConciergeConfirmRequestDTO request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ConversationId))
            return BadRequest("ConversationId is required.");
        if (request.ProposalIds == null || request.ProposalIds.Count == 0)
            return BadRequest("At least one proposal ID is required.");

        var response = await _concierge.ConfirmProposalsAsync(
            request.ConversationId, request.ProposalIds, ct);

        return Ok(response);
    }

    [HttpGet("proposals")]
    public async Task<ActionResult<List<ConciergeProposalDTO>>> GetPendingProposals(
        [FromQuery] string conversationId,
        CancellationToken ct)
    {
        var proposals = await _concierge.GetPendingProposalsAsync(conversationId, ct);
        return Ok(proposals);
    }

    [HttpGet("context")]
    public async Task<ActionResult<GuestContextDTO>> GetContext(CancellationToken ct)
    {
        var context = await _concierge.GetGuestContextAsync(ct);
        return Ok(context);
    }
}
```

---

## 6. Frontend Implementation

### 6.1 API Service

```typescript
// Frontend/src/app/features/user/services/concierge-api.service.ts
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

  getContext(): Observable<GuestContext> {
    return this.http.get<GuestContext>(`${this.baseUrl}/context`);
  }
}
```

### 6.2 Concierge Chat Component

```typescript
// Frontend/src/app/features/user/components/concierge-chat/concierge-chat.component.ts
import { Component, inject, signal, computed, OnInit, DestroyRef, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl, Validators } from '@angular/forms';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs/operators';

import {
  ConciergeApiService, ConciergeChatRequest, ConciergeChatResponse,
  ConciergeProposal, ConciergeActionResult
} from '../../services/concierge-api.service';
import { AuthService } from '../../../core/services/auth.service';

interface ChatMessage {
  role: 'user' | 'assistant' | 'system';
  content: string;
  proposals?: ConciergeProposal[];
  actions?: ConciergeActionResult[];
  timestamp: Date;
}

@Component({
  selector: 'app-concierge-chat',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule,
    MatInputModule, MatButtonModule, MatIconModule,
    MatProgressSpinnerModule, MatCardModule, MatChipsModule
  ],
  templateUrl: './concierge-chat.component.html',
  styleUrls: ['./concierge-chat.component.scss']
})
export class ConciergeChatComponent implements OnInit {
  private readonly api = inject(ConciergeApiService);
  private readonly auth = inject(AuthService);
  private readonly destroyRef = inject(DestroyRef);

  @ViewChild('messagesContainer') messagesContainer!: ElementRef<HTMLDivElement>;

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

  ngOnInit(): void {
    this.loadContext();
    this.addWelcomeMessage();

    // Clear conversation state on logout
    this.auth.onLogout(() => {
      this.conversationId.set(null);
      this.messages.set([]);
      this.pendingProposals.set([]);
      localStorage.removeItem('concierge_conversation_id');
    });
  }

  private loadContext(): void {
    this.api.getContext().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (ctx) => this.context.set(ctx)
    });
  }

  private addWelcomeMessage(): void {
    const name = this.auth.fullName() || 'there';
    this.messages.set([{
      role: 'assistant',
      content: `Hello ${name}! I'm your AI Concierge. I can help with room service, housekeeping, maintenance, billing questions, and more. What can I do for you today?`,
      timestamp: new Date()
    }]);
  }

  sendMessage(): void {
    if (this.messageControl.invalid || this.loading()) return;

    const userMessage = this.messageControl.value;
    this.messageControl.reset();
    this.loading.set(true);

    const userMsg: ChatMessage = { role: 'user', content: userMessage, timestamp: new Date() };
    this.messages.update(msgs => [...msgs, userMsg]);
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

  confirmProposals(): void {
    if (this.pendingProposals().length === 0 || this.loading()) return;

    this.loading.set(true);

    this.api.confirm({
      conversationId: this.conversationId()!,
      proposalIds: this.pendingProposals().map(p => p.proposalId)
    }).pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.loading.set(false))
    ).subscribe({
      next: (response) => {
        this.pendingProposals.set([]);
        this.handleResponse(response);
      },
      error: (err) => this.handleError(err)
    });
  }

  private handleResponse(response: ConciergeChatResponse): void {
    this.conversationId.set(response.conversationId);
    localStorage.setItem('concierge_conversation_id', response.conversationId);

    // Show proposals as pending confirmation
    if (response.proposals.length > 0) {
      this.pendingProposals.set(response.proposals);

      this.messages.update(msgs => [...msgs, {
        role: 'system',
        content: `Proposals ready for confirmation:`,
        proposals: response.proposals,
        timestamp: new Date()
      }]);
    }

    // Show auto-executed action results
    if (response.actions.length > 0) {
      response.actions.forEach(action => {
        this.messages.update(msgs => [...msgs, {
          role: 'system',
          content: action.success
            ? `${action.resultSummary}`
            : `${action.error || 'Action failed'}`,
          timestamp: new Date()
        }]);
      });
    }

    // Final assistant reply
    this.messages.update(msgs => [...msgs, {
      role: 'assistant',
      content: response.reply,
      proposals: response.proposals.length > 0 ? response.proposals : undefined,
      actions: response.actions.length > 0 ? response.actions : undefined,
      timestamp: new Date()
    }]);

    this.scrollToBottom();
  }

  private handleError(err: any): void {
    const msg = err.error?.message || err.message || 'Something went wrong. Please try again.';
    this.messages.update(msgs => [...msgs, {
      role: 'assistant',
      content: `I'm sorry — ${msg}`,
      timestamp: new Date()
    }]);
    this.scrollToBottom();
  }

  useQuickAction(prompt: string): void {
    this.messageControl.setValue(prompt);
    this.sendMessage();
  }

  private scrollToBottom(): void {
    setTimeout(() => this.messagesContainer?.nativeElement?.scrollTop = this.messagesContainer.nativeElement.scrollHeight, 0);
  }
}
```

### 6.3 Chat Component Template

```html
<!-- Frontend/src/app/features/user/components/concierge-chat/concierge-chat.component.html -->
<div class="concierge-chat" style="display:flex;flex-direction:column;height:100%;">
  <!-- Context Bar -->
  @if (context(); as ctx) {
    <mat-card class="context-bar" appearance="outlined">
      <mat-chip-set>
        @if (ctx.bookingId) {
          <mat-chip>Booking #{{ ctx.bookingId }}</mat-chip>
        }
        @if (ctx.roomNumber) {
          <mat-chip>Room {{ ctx.roomNumber }}</mat-chip>
        }
        @if (ctx.bookingStatus) {
          <mat-chip [color]="ctx.bookingStatus === 'CheckedIn' ? 'primary' : 'accent'">{{ ctx.bookingStatus }}</mat-chip>
        }
      </mat-chip-set>
    </mat-card>
  }

  <!-- Messages -->
  <div #messagesContainer class="messages" style="flex:1;overflow-y:auto;padding:16px;display:flex;flex-direction:column;gap:12px;">
    @for (msg of messages(); track $index) {
      <div class="message" [class]="'message-' + msg.role"
           [style.align-self]="msg.role === 'user' ? 'flex-end' : 'flex-start'">
        <div class="bubble"
             [style.background]="msg.role === 'user' ? 'var(--mat-sys-primary)' : 'var(--mat-sys-surface-variant)'"
             [style.color]="msg.role === 'user' ? 'var(--mat-sys-on-primary)' : 'var(--mat-sys-on-surface-variant)'">
          <p style="margin:0;white-space:pre-wrap;">{{ msg.content }}</p>

          <!-- Proposals awaiting confirmation -->
          @if (msg.proposals?.length) {
            <div class="proposals" style="margin-top:8px;border:1px solid var(--mat-sys-outline);border-radius:8px;padding:12px;">
              <strong>Proposed actions:</strong>
              @for (prop of msg.proposals; track prop.proposalId) {
                <div style="display:flex;align-items:center;gap:8px;margin-top:4px;">
                  <mat-icon color="primary">info</mat-icon>
                  <span>{{ prop.summary }}</span>
                </div>
              }
              <button mat-flat-button color="primary" (click)="confirmProposals()"
                      [disabled]="loading()" style="margin-top:8px;">
                Confirm & Execute
              </button>
            </div>
          }

          <!-- Action results -->
          @if (msg.actions?.length) {
            <div class="actions" style="margin-top:8px;display:flex;flex-wrap:wrap;gap:4px;">
              @for (action of msg.actions; track action.toolCallId) {
                <mat-chip [color]="action.success ? 'primary' : 'warn'" size="small">
                  {{ action.success ? '' : '' }} {{ action.resultSummary || action.error }}
                </mat-chip>
              }
            </div>
          }
        </div>
        <div class="timestamp" style="font-size:11px;color:var(--mat-sys-on-surface-variant);margin-top:4px;">
          {{ msg.timestamp | date:'shortTime' }}
        </div>
      </div>
    }
  </div>

  <!-- Quick Actions -->
  <div class="quick-actions" style="padding:16px;display:flex;flex-wrap:wrap;gap:8px;border-top:1px solid var(--mat-sys-outline-variant);">
    @for (action of quickActions; track action.label) {
      <button mat-stroked-button (click)="useQuickAction(action.prompt)" [disabled]="loading()"
              style="font-size:13px;height:32px;">
        {{ action.label }}
      </button>
    }
  </div>

  <!-- Input -->
  <div class="input-area" style="padding:16px;border-top:1px solid var(--mat-sys-outline-variant);">
    <mat-form-field appearance="outline" style="width:100%;" [class.hidden]="loading()">
      <mat-label>Ask me anything...</mat-label>
      <input matInput [formControl]="messageControl" (keydown.enter)="sendMessage()"
             placeholder="e.g., 'I'd like a burger and extra towels'">
      <button mat-icon-button matSuffix (click)="sendMessage()" [disabled]="messageControl.invalid || loading() || pendingProposals().length > 0">
        <mat-icon>send</mat-icon>
      </button>
    </mat-form-field>
    @if (loading()) {
      <div class="loading" style="display:flex;align-items:center;gap:12px;padding:0 16px;">
        <mat-spinner diameter="24"></mat-spinner>
        <span>Thinking...</span>
      </div>
    }
  </div>
</div>
```

---

## 7. Integration Points & Security

### 7.1 Role-Based Access (BLL Barrier Already Exists)

All BLL services already use `ICurrentUserService` for authorization:
- Guest role → queries filtered to their `UserId` / `Booking.UserId`
- ConciergeService inherits this — **no new auth logic needed**

### 7.2 SignalR Integration (Real-Time Staff Alerts)

Existing `INotificationService` already broadcasts to:
- `KitchenGroup` (food orders)
- `HousekeepingGroup` (housekeeping requests)
- `MaintenanceGroup` (maintenance tickets)

Concierge actions **reuse the exact same service calls** → staff dashboards light up instantly, zero new code.

### 7.3 Conversation Security

| Concern | Mitigation |
|---------|-----------|
| **Cross-spilling** | Conversation key scoped to user: `concierge:conv:{userId}:{conversationId}`. All store lookups require both user ID and conversation ID |
| **Auth logout** | Frontend clears `conversationId` from signal, removes `localStorage` key, and resets chat state via `onLogout()` callback |
| **Session replay** | Conversation auto-expire after 24h inactivity (cleanup job) |

### 7.4 Audit Trail

Every tool call is logged to the `ConciergeActionLog` table:
- User message, tool name, serialized arguments
- Success/failure status and error message
- Immutable audit trail separate from business entities

### 7.5 Rate Limiting & Abuse Prevention

| Layer | Mechanism |
|-------|-----------|
| API Gateway | Existing global rate limiter (100 req/10s) |
| Per-User | `AspNetCoreRateLimit` policy: 30 chat req/min per JWT |
| OpenAI | Built-in token limits; `maxTokens: 1000` per completion |
| Tool calls | Hard cap: max 5 tool calls per turn |
| Proposals | Auto-expire after 5 minutes if not confirmed |
| Input sanitization | Strip prompt injection patterns at controller entry point |

---

## 8. Configuration

```json
{
  "OpenAI": {
    "ApiKey": "sk-...",
    "Model": "gpt-4o-mini"
  },
  "Concierge": {
    "MaxConversationTurns": 20,
    "ConversationTtlHours": 24,
    "RateLimitPerMinute": 30,
    "MaxToolCallsPerTurn": 5,
    "ProposalTtlMinutes": 5
  }
}
```

```csharp
// Program.cs additions
builder.Services.Configure<ConciergeOptions>(builder.Configuration.GetSection("Concierge"));
builder.Services.AddMemoryCache();
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("ConciergePolicy", context =>
        RateLimitPartition.GetTokenBucketLimiter(
            context.User?.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "anon",
            _ => new TokenBucketRateLimiterOptions { TokenLimit = 30, QueueProcessingOrder = QueueProcessingOrder.OldestFirst, QueueLimit = 5, ReplenishmentPeriod = TimeSpan.FromMinutes(1), TokensPerPeriod = 30 }));
});
```

---

## 9. Testing Strategy

| Test Type | Coverage |
|-----------|----------|
| **Unit** | `ConciergeService.ProcessMessageAsync` with mocked BLL services; `ToolExecutor` dispatch; `PromptBuilder` context injection; `InputSanitizer` pattern stripping; proposal creation/confirmation flow |
| **Integration** | Full flow: Controller → ConciergeService → OrderService → Repository → DB (Testcontainers PostgreSQL); proposal TTL expiry; audit log writes |
| **E2E** | Cypress/Playwright: Guest logs in → opens chat → sends "burger + towels" → sees proposals → clicks confirm → verifies Kitchen/Housekeeping dashboards receive SignalR alerts |
| **Security** | Prompt injection attempts (`ignore previous instructions`, `system:` etc.) are sanitized; cross-conversation access returns empty; unauthenticated requests rejected |
| **Load** | k6 script: 50 concurrent guests chatting → verify <2s p95 latency, no OpenAI quota exhaustion |

---

## 10. Rollout Plan (Phased)

| Phase | Scope | Duration |
|-------|-------|----------|
| **P0: Core Engine** | `IConciergeService`, tool definitions, two-step proposal pattern, PostgreSQL conversation store, audit logging, inline sanitization | 4 days |
| **P1: API + Idempotency** | `ConciergeController` with chat + confirm endpoints, `[Idempotent]` per-turn, rate limiting, OpenAI DI | 1 day |
| **P2: Frontend Chat** | `ConciergeChatComponent`, confirmation gate UI (proposal card + confirm button), quick actions, context bar, logout cleanup | 2 days |
| **P3: Polish & Streaming** | SSE/SignalR streaming for token-by-token replies, menu browsing (carousel), order history, multi-language (i18n keys) | 2 days |
| **P4: Demo Hardening** | Seed data scripts (3 guests with rich histories), staff dashboard projection mode, failure injection tests | 1 day |

**Total: ~10 working days (2 weeks) for production-ready MVP**

---

## 11. Demo Script (Jaw-Dropping Flow)

> **Setup**: Three screens projected — Guest Mobile, Kitchen Dashboard, Housekeeping/Maintenance Dashboard

| Time | Guest (Mobile) | Kitchen Screen | Housekeeping Screen | Maintenance Screen |
|------|----------------|----------------|---------------------|-------------------|
| 0:00 | Opens chat, sees context: "Booking #1042 • Room 304 • Checked In" | — | — | — |
| 0:05 | Types: *"I just checked in. Can you send up a burger, some extra pillows, and also my TV isn't working?"* | — | — | — |
| 0:07 | **AI replies**: *"I'd be happy to help! Here's what I'm proposing: 1) Order: Classic Burger ×1, 2) Extra pillows for Room 304, 3) TV repair — maintenance ticket. Shall I go ahead?"* | — | — | — |
| 0:10 | Guest sees **Proposal Card** with "Confirm & Execute" button. Taps it. | 🔔 **"New order: Room 304 — Burger ×1"** | 🔔 **"New task: Room 304 — Extra pillows"** | 🔔 **"URGENT: Room 304 — TV not working"** |
| 0:12 | **AI confirms**: *"All done! Your burger is being prepared, pillows are on their way, and a technician has been dispatched for the TV. You'll see updates in real-time."* | Chef taps "Preparing" | Staff taps "Assigned" | Tech taps "En route" |
| 0:15 | Guest sees action chips: ✅ Order #57 placed • ✅ Housekeeping request created • ✅ Maintenance ticket #12 created | — | — | — |
| 0:20 | Guest: *"What's my checkout time?"* | — | — | — |
| 0:21 | AI: *"Check-out is 11:00 AM on March 20th. Your current folio balance is $342.50."* | — | — | — |

**No front desk call. No wait. Three departments mobilized in one sentence. Explicit confirmation gives the guest control.**

---

## 12. Future Extensions (Post-MVP)

| Feature | Description |
|---------|-------------|
| **Proactive Nudges** | "It's 7:30 AM — your usual latte?" (uses Personalization Engine prefs) |
| **Voice Input** | Web Speech API → STT → same pipeline |
| **Multilingual** | Detect language → reply in guest's language (OpenAI supports 50+) |
| **Streaming Responses** | SSE/SignalR streaming for token-by-token replies |
| **Upsell Suggestions** | "Would you like to add our signature dessert for $8?" |
| **Integration with PMS** | Push folio to Opera/Cloudbeds, sync room status |
| **Analytics Dashboard** | Intent classification, resolution rate, guest satisfaction per conversation |

---

## 13. File Checklist (What to Create)

| Path | Purpose |
|------|---------|
| `Backend/HotelManagement.BLL/DTOs/ConciergeDTOs.cs` | Request/response/proposal/action DTOs |
| `Backend/HotelManagement.BLL/Interfaces/IConciergeService.cs` | Service contract |
| `Backend/HotelManagement.BLL/Services/Concierge/ConciergeService.cs` | Main orchestration with two-step pattern |
| `Backend/HotelManagement.BLL/Services/Concierge/ToolExecutor.cs` | Tool dispatch |
| `Backend/HotelManagement.BLL/Services/Concierge/ToolDefinitions.cs` | OpenAI function schemas + tool classification |
| `Backend/HotelManagement.BLL/Services/Concierge/PromptBuilder.cs` | System prompt construction |
| `Backend/HotelManagement.BLL/Services/Concierge/IConversationStore.cs` | Conversation persistence interface |
| `Backend/HotelManagement.BLL/Services/Concierge/PostgresConversationStore.cs` | PostgreSQL conversation store |
| `Backend/HotelManagement.BLL/Services/Concierge/IProposalStore.cs` | Proposal store interface |
| `Backend/HotelManagement.BLL/Services/Concierge/PostgresProposalStore.cs` | PostgreSQL proposal store |
| `Backend/HotelManagement.BLL/Services/Concierge/InputSanitizer.cs` | Inline regex sanitization |
| `Backend/HotelManagement.BLL/Options/OpenAIOptions.cs` | Config |
| `Backend/HotelManagement.DAL/Entities/ConciergeActionLog.cs` | Audit log entity |
| `Backend/HotelManagement.DAL/Configurations/ConciergeActionLogConfiguration.cs` | EF configuration |
| `Backend/HotelManagement.DAL/Migrations/xxx_AddConciergeTables.cs` | DB migration |
| `Backend/HotelManagement.API/Controllers/ConciergeController.cs` | HTTP endpoints (chat, confirm, proposals, context) |
| `Frontend/src/app/features/user/services/concierge-api.service.ts` | Angular API client |
| `Frontend/src/app/features/user/components/concierge-chat/concierge-chat.component.ts` | Chat UI with confirmation gate |
| `Frontend/src/app/features/user/components/concierge-chat/concierge-chat.component.html` | Template |
| `Frontend/src/app/features/user/components/concierge-chat/concierge-chat.component.scss` | Styles |

---

## 14. Dependencies to Add

```xml
<!-- HotelManagement.BLL.csproj -->
<PackageReference Include="OpenAI" Version="2.0.0" />
<PackageReference Include="AspNetCoreRateLimit" Version="5.0.0" />

<!-- HotelManagement.API (if not already referenced) -->
<PackageReference Include="IdempotentRequest" Version="..." />
```

---

**End of Design Document**
