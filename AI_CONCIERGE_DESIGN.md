# AI Concierge Feature — Detailed Design Document

---

## 1. Executive Summary

The **AI Concierge** is a guest-facing conversational assistant embedded in the User Portal (and optionally the Public site). It goes beyond Q&A by **executing real actions** via existing BLL services — placing room-service orders, creating housekeeping/maintenance tickets, retrieving billing/booking info — all scoped to the authenticated guest's active booking through `ICurrentUserService`.

**Jaw-dropping demo**: Guest types one message → three departments (Kitchen, Housekeeping, Maintenance) instantly receive real-time SignalR alerts → AI replies with confirmation — all visible on projected staff dashboards.

---

## 2. Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              USER PORTAL (Angular)                          │
│  ┌──────────────────┐    ┌──────────────────┐    ┌──────────────────────┐  │
│  │  AI Concierge    │───▶│  ConciergeApi    │───▶│  SignalR (alerts,    │  │
│  │  Chat Component  │    │  Service (HTTP)  │    │   streaming tokens)  │  │
│  └──────────────────┘    └──────────────────┘    └──────────────────────┘  │
└─────────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                            API LAYER (ASP.NET Core)                         │
│  ┌──────────────────────────────────────────────────────────────────────┐   │
│  │  POST /api/v1/concierge/chat                                          │   │
│  │  • Auth: JWT (guest role)                                             │   │
│  │  • Input: { message, conversationId?, stream?: true }                 │   │
│  │  • Output: SSE stream or JSON { reply, toolCalls[], actions[] }       │   │
│  └──────────────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                          BLL — CONCierge ORCHESTRATOR                        │
│  ┌──────────────────────────────────────────────────────────────────────┐   │
│  │  IConciergeService                                                    │   │
│  │  • ProcessMessageAsync(userMessage, conversationHistory)              │   │
│  │  • ExecuteToolCallsAsync(toolCalls, bookingContext)                   │   │
│  │  • GetGuestContextAsync() → ActiveBookingDTO + Preferences            │   │
│  └──────────────────────────────────────────────────────────────────────┘   │
│                                    │                                        │
│         ┌────────────────────────────┼────────────────────────────┐        │
│         ▼                            ▼                            ▼        │
│  ┌───────────────┐           ┌───────────────┐           ┌───────────────┐ │
│  │ OrderService  │           │HousekeepingSvc│           │MaintenanceSvc │ │
│  │CreateOrderAsync│          │CreateGuestReq │           │CreateTicketAsy│ │
│  └───────────────┘           └───────────────┘           └───────────────┘ │
│                                    │                                        │
│  ┌───────────────┐           ┌───────────────┐           ┌───────────────┐ │
│  │ BookingService│           │ BillingService│           │  MenuItemRepo │ │
│  │GetBookingById │           │GenerateFolio  │           │ GetAvailable  │ │
│  └───────────────┘           └───────────────┘           └───────────────┘ │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 3. Data Models & DTOs

### 3.1 Request/Response (API Layer)

```csharp
// Backend/HotelManagement.BLL/DTOs/ConciergeDTOs.cs

public class ConciergeChatRequestDTO
{
    public string Message { get; set; } = string.Empty;
    public string? ConversationId { get; set; }
    public bool Stream { get; set; } = true;           // SSE streaming
}

public class ConciergeChatResponseDTO
{
    public string Reply { get; set; } = string.Empty;
    public string ConversationId { get; set; } = string.Empty;
    public List<ConciergeToolCallDTO> ToolCalls { get; set; } = new();
    public List<ConciergeActionResultDTO> Actions { get; set; } = new();
    public bool IsComplete { get; set; } = true;
}

public class ConciergeToolCallDTO
{
    public string Name { get; set; } = string.Empty;        // e.g. "create_food_order"
    public string ArgumentsJson { get; set; } = "{}";       // OpenAI function-calling format
    public string CallId { get; set; } = string.Empty;
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

public static class ConciergeTools
{
    public static readonly List<FunctionDefinition> Definitions = new()
    {
        new FunctionDefinition
        {
            Name = "create_food_order",
            Description = "Place a room-service order for the guest's active booking",
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
}

// Tool Argument Types
public class CreateFoodOrderToolArgs
{
    [JsonPropertyName("items")]
    [Required] public List<FoodOrderItemToolArg> Items { get; set; } = new();
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
│   │   └── ConversationStore.cs
│   └── (existing services...)
├── DTOs/
│   └── ConciergeDTOs.cs
└── Interfaces/
    └── IConciergeService.cs
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
        bool stream = false,
        CancellationToken ct = default);

    Task<ConciergeChatResponseDTO> ContinueWithToolResultsAsync(
        string conversationId,
        List<ConciergeToolResultDTO> toolResults,
        CancellationToken ct = default);

    Task<GuestContextDTO> GetGuestContextAsync(CancellationToken ct = default);
}

public class GuestContextDTO
{
    public int? BookingId { get; set; }
    public int? RoomId { get; set; }
    public string? RoomNumber { get; set; }
    public DateTime? CheckInDate { get; set; }
    public DateTime? CheckOutDate { get; set; }
    public BookingStatus BookingStatus { get; set; }
    public List<MenuItemSummaryDTO> RecentOrders { get; set; } = new();
    public List<GuestPreferenceDTO> Preferences { get; set; } = new();
}

public class ConciergeToolResultDTO
{
    public string CallId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ResultJson { get; set; }
    public string? Error { get; set; }
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
        _openAIOptions = openAIOptions;
        _logger = logger;

        _chatClient = new ChatClient("gpt-4o-mini", _openAIOptions.Value.ApiKey);
        _tools = BuildTools();
    }

    public async Task<ConciergeChatResponseDTO> ProcessMessageAsync(
        string userMessage, string? conversationId, bool stream, CancellationToken ct)
    {
        // 1. Resolve or create conversation
        var convId = conversationId ?? Guid.NewGuid().ToString();
        var history = await _conversations.GetAsync(convId, ct);

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
        var actions = new List<ConciergeActionResultDTO>();

        if (toolCalls.Any())
        {
            foreach (var call in toolCalls)
            {
                var result = await ToolExecutor.ExecuteAsync(call, context, this, ct);
                actions.Add(result);
            }

            // 6. Feed tool results back to LLM for final natural-language reply
            var toolMessages = toolCalls.Zip(actions, (call, action) =>
                new ChatMessage(ChatMessageRole.Tool, action.ResultJson ?? action.Error ?? "{}")
                { ToolCallId = call.Id });

            var finalMessages = messages.Concat(toolMessages).Append(
                new ChatMessage(ChatMessageRole.System, "Summarize what was accomplished in a friendly, concise way."));

            var finalCompletion = await _chatClient.CompleteChatAsync(finalMessages, ct: ct);
            var finalReply = finalCompletion.Value.Content[0].Text;

            // 7. Persist conversation
            await _conversations.AppendAsync(convId, userMessage, finalReply, actions, ct);

            return new ConciergeChatResponseDTO
            {
                Reply = finalReply,
                ConversationId = convId,
                ToolCalls = toolCalls.Select(tc => new ConciergeToolCallDTO
                {
                    Name = tc.FunctionName,
                    ArgumentsJson = tc.FunctionArguments,
                    CallId = tc.Id
                }).ToList(),
                Actions = actions,
                IsComplete = true
            };
        }

        // No tool calls — just a conversational reply
        var reply = response.Content[0].Text;
        await _conversations.AppendAsync(convId, userMessage, reply, new(), ct);

        return new ConciergeChatResponseDTO
        {
            Reply = reply,
            ConversationId = convId,
            Actions = new(),
            IsComplete = true
        };
    }

    // Tool implementations — each maps to a BLL service call
    public async Task<ConciergeActionResultDTO> CreateFoodOrderAsync(CreateFoodOrderToolArgs args, GuestContextDTO ctx, CancellationToken ct)
    {
        if (ctx.BookingId == null || ctx.RoomId == null)
            return Fail("No active booking found. Please check in first.");

        if (ctx.BookingStatus != BookingStatus.CheckedIn)
            return Fail("Room service is only available for checked-in guests.");

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

    private async Task<GuestContextDTO> BuildGuestContextAsync(CancellationToken ct)
    {
        var email = _currentUser.GetUserEmail();
        if (string.IsNullOrEmpty(email)) return new GuestContextDTO();

        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null) return new GuestContextDTO();

        // Find active booking (CheckedIn or Booked with today's check-in)
        var today = DateTime.UtcNow.Date;
        var bookings = await _bookingRepository.GetPaginatedBookingsWithDetailsAsync(1, 5, new List<Expression<Func<Booking, bool>>>
        {
            b => b.UserId == user.Id &&
                 (b.BookingStatus == BookingStatus.CheckedIn ||
                  (b.BookingStatus == BookingStatus.Booked && b.CheckInDate.Date == today))
        });

        var active = bookings.Data.FirstOrDefault();
        if (active == null) return new GuestContextDTO();

        var roomId = active.BookingRooms.FirstOrDefault(br => br.RoomId.HasValue)?.RoomId;
        var roomNumber = active.BookingRooms.FirstOrDefault(br => br.RoomId.HasValue)?.Room?.RoomNumber;

        return new GuestContextDTO
        {
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

        // Add conversation history (last 10 turns)
        foreach (var turn in history.TakeLast(10))
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
        sb.AppendLine("• create_food_order: Place room-service orders. Requires guest to be checked in.");
        sb.AppendLine("• create_housekeeping_request: Extra towels, cleaning, amenities, etc.");
        sb.AppendLine("• create_maintenance_ticket: Broken AC, leaky faucet, TV issues, etc. Use isEmergency=true for urgent safety issues.");
        sb.AppendLine("• get_booking_info: Answer questions about check-in/out times, room number, stay dates.");
        sb.AppendLine("• get_folio_balance: Current bill total, payment status.");
        sb.AppendLine("• get_housekeeping_status: Has room been cleaned? Any pending requests?");
        sb.AppendLine("• get_menu_items: Browse menu. Supports category filter (breakfast, lunch, dinner, drinks, snacks).");
        sb.AppendLine("• get_active_orders: Show pending/delivered room-service orders.");
        sb.AppendLine();
        sb.AppendLine("--- RULES ---");
        sb.AppendLine("1. NEVER ask for booking ID, room number, or guest name — you have them from context.");
        sb.AppendLine("2. If guest is not checked in, politely explain what's available (pre-arrival questions, booking info).");
        sb.AppendLine("3. For food orders: confirm items & quantities before calling tool. Mention prices.");
        sb.AppendLine("4. For maintenance: if safety issue (fire, flood, gas), set isEmergency=true and tell guest help is coming immediately.");
        sb.AppendLine("5. Keep replies under 3 sentences unless explaining menu or folio details.");
        sb.AppendLine("6. If multiple requests in one message, call ALL relevant tools in parallel.");

        return sb.ToString();
    }
}
```

### 4.6 Conversation Store (In-Memory + Redis Ready)

```csharp
// HotelManagement.BLL/Services/Concierge/ConversationStore.cs
public interface IConversationStore
{
    Task<List<ConversationTurn>> GetAsync(string conversationId, CancellationToken ct);
    Task AppendAsync(string conversationId, string userMsg, string assistantMsg, List<ConciergeActionResultDTO> actions, CancellationToken ct);
}

public class ConversationTurn
{
    public string UserMessage { get; set; } = "";
    public string AssistantReply { get; set; } = "";
    public List<ConciergeActionResultDTO> Actions { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

// Simple in-memory implementation (swap for Redis in production)
public class InMemoryConversationStore : IConversationStore
{
    private readonly ConcurrentDictionary<string, List<ConversationTurn>> _store = new();

    public Task<List<ConversationTurn>> GetAsync(string id, CancellationToken ct)
        => Task.FromResult(_store.GetValueOrDefault(id, new List<ConversationTurn>()));

    public Task AppendAsync(string id, string user, string assistant, List<ConciergeActionResultDTO> actions, CancellationToken ct)
    {
        _store.AddOrUpdate(id,
            _ => new List<ConversationTurn> { new() { UserMessage = user, AssistantReply = assistant, Actions = actions } },
            (_, list) => { list.Add(new ConversationTurn { UserMessage = user, AssistantReply = assistant, Actions = actions }); return list; });
        return Task.CompletedTask;
    }
}
```

### 4.7 OpenAI Options & DI Registration

```csharp
// HotelManagement.BLL/Options/OpenAIOptions.cs
public class OpenAIOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gpt-4o-mini";
}
```

```csharp
// HotelManagement.BLL/DependencyInjection.cs (or Program.cs registration)
public static class BllServiceCollectionExtensions
{
    public static IServiceCollection AddBllServices(this IServiceCollection services, IConfiguration config)
    {
        // ... existing registrations ...

        services.Configure<OpenAIOptions>(config.GetSection("OpenAI"));
        services.AddSingleton<IConversationStore, InMemoryConversationStore>();
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
    public async Task<ActionResult<ConciergeChatResponseDTO>> Chat(
        [FromBody] ConciergeChatRequestDTO request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest("Message is required.");

        var response = await _concierge.ProcessMessageAsync(
            request.Message, request.ConversationId, request.Stream, ct);

        if (request.Stream)
        {
            // SSE streaming implementation
            return new ConciergeController
            Response.Headers.ContentType = "text/event-stream";
            Response.Headers.CacheControl = "no-cache";
            Response.Headers.Connection = "keep-alive";

            // Stream initial reply + tool calls as they complete
            // (Implementation detail — see below)
            await StreamResponseAsync(response, ct);
            return new EmptyResult();
        }

        return Ok(response);
    }

    [HttpGet("context")]
    public async Task<ActionResult<GuestContextDTO>> GetContext(CancellationToken ct)
    {
        var context = await _concierge.GetGuestContextAsync(ct);
        return Ok(context);
    }

    private async Task StreamResponseAsync(ConciergeChatResponseDTO response, CancellationToken ct)
    {
        // Simplified: send full response as single SSE event
        // Production: stream tokens from OpenAI, then tool results, then final reply
        var json = JsonSerializer.Serialize(response);
        await Response.WriteAsync($"data: {json}\n\n", ct);
        await Response.Body.FlushAsync(ct);
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
import { Observable, Subject } from 'rxjs';
import { environment } from '../../../../environments/environment';

export interface ConciergeChatRequest {
  message: string;
  conversationId?: string;
  stream?: boolean;
}

export interface ConciergeToolCall {
  name: string;
  argumentsJson: string;
  callId: string;
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
  toolCalls: ConciergeToolCall[];
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

  // SSE streaming version
  chatStream(request: ConciergeChatRequest): Subject<ConciergeChatResponse> {
    const subject = new Subject<ConciergeChatResponse>();
    const url = `${this.baseUrl}/chat?stream=true`;

    const eventSource = new EventSource(url, {
      withCredentials: true
    });

    eventSource.onmessage = (event) => {
      try {
        const data = JSON.parse(event.data) as ConciergeChatResponse;
        subject.next(data);
      } catch (e) {
        console.error('Failed to parse SSE message', e);
      }
    };

    eventSource.onerror = () => {
      subject.error(new Error('SSE connection error'));
      eventSource.close();
    };

    // Send the message via POST (EventSource can't POST with body easily)
    // Alternative: use fetch + ReadableStream for true streaming
    this.http.post(url, request).subscribe({
      next: (res) => subject.next(res),
      error: (err) => subject.error(err),
      complete: () => subject.complete()
    });

    return subject;
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

import { ConciergeApiService, ConciergeChatRequest, ConciergeChatResponse, ConciergeActionResult } from '../../services/concierge-api.service';
import { AuthService } from '../../../core/services/auth.service';

interface ChatMessage {
  role: 'user' | 'assistant' | 'system';
  content: string;
  actions?: ConciergeActionResult[];
  timestamp: Date;
  isStreaming?: boolean;
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
  loading = signal(false);
  context = signal<ConciergeApiService.GuestContext | null>(null);

  messageControl = new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(1000)] });

  quickActions = [
    { label: '🍔 Order Food', prompt: 'I\'d like to order some food' },
    { label: '🛏️ Extra Pillows', prompt: 'Can I get extra pillows and blankets?' },
    { label: '🔧 Report Issue', prompt: 'There\'s a maintenance issue in my room' },
    { label: '💰 Check Bill', prompt: 'What\'s my current folio balance?' },
    { label: '🕐 Check-out Time', prompt: 'What time is check-out?' },
    { label: '🧹 Room Status', prompt: 'Has my room been cleaned yet?' }
  ];

  ngOnInit(): void {
    this.loadContext();
    this.addWelcomeMessage();
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
      content: `Hello ${name}! 👋 I'm your AI Concierge. I can help with room service, housekeeping, maintenance, billing questions, and more. What can I do for you today?`,
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
      conversationId: this.conversationId() || undefined,
      stream: false // Start with non-streaming for simplicity
    };

    this.api.chat(request).pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.loading.set(false))
    ).subscribe({
      next: (response) => this.handleResponse(response),
      error: (err) => this.handleError(err)
    });
  }

  private handleResponse(response: ConciergeChatResponse): void {
    this.conversationId.set(response.conversationId);

    // Show tool calls as "system" messages for transparency
    if (response.toolCalls.length > 0) {
      response.toolCalls.forEach(tc => {
        this.messages.update(msgs => [...msgs, {
          role: 'system',
          content: `🔧 Calling ${tc.name}...`,
          timestamp: new Date()
        }]);
      });
    }

    // Show action results
    if (response.actions.length > 0) {
      response.actions.forEach(action => {
        this.messages.update(msgs => [...msgs, {
          role: 'system',
          content: action.success
            ? `✅ ${action.resultSummary}`
            : `❌ ${action.error || 'Action failed'}`,
          timestamp: new Date()
        }]);
      });
    }

    // Final assistant reply
    this.messages.update(msgs => [...msgs, {
      role: 'assistant',
      content: response.reply,
      actions: response.actions,
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

### 6.3 Chat Component Template (Key Parts)

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
      <div class="message" [class]="'message-' + msg.role" [style.align-self]="msg.role === 'user' ? 'flex-end' : 'flex-start'">
        <div class="bubble" [style.background]="msg.role === 'user' ? 'var(--mat-sys-primary)' : 'var(--mat-sys-surface-variant)'"
             [style.color]="msg.role === 'user' ? 'var(--mat-sys-on-primary)' : 'var(--mat-sys-on-surface-variant)'">
          <p style="margin:0;white-space:pre-wrap;">{{ msg.content }}</p>
          @if (msg.actions?.length) {
            <div class="actions" style="margin-top:8px;display:flex;flex-wrap:wrap;gap:4px;">
              @for (action of msg.actions; track action.toolCallId) {
                <mat-chip [color]="action.success ? 'primary' : 'warn'" size="small">
                  {{ action.success ? '✅' : '❌' }} {{ action.resultSummary || action.error }}
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

  <!-- Quick Actions (shown when no messages or at bottom) -->
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
      <input matInput [formControl]="messageControl" (keydown.enter)="sendMessage()" placeholder="e.g., 'I'd like a burger and extra towels'">
      <button mat-icon-button matSuffix (click)="sendMessage()" [disabled]="messageControl.invalid || loading()">
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

### 7.3 Rate Limiting & Abuse Prevention

| Layer | Mechanism |
|-------|-----------|
| API Gateway | Existing global rate limiter (100 req/10s) |
| Per-User | Add `AspNetCoreRateLimit` policy: 30 chat req/min per JWT |
| OpenAI | Built-in token limits; `maxTokens: 1000` per completion |
| Conversation | Auto-expire after 24h inactivity (cleanup job) |

---

## 8. Configuration

```json
// appsettings.json
{
  "OpenAI": {
    "ApiKey": "sk-...",
    "Model": "gpt-4o-mini"
  },
  "Concierge": {
    "MaxConversationTurns": 20,
    "ConversationTtlHours": 24,
    "RateLimitPerMinute": 30
  }
}
```

```csharp
// Program.cs additions
builder.Services.Configure<ConciergeOptions>(builder.Configuration.GetSection("Concierge"));
builder.Services.AddMemoryCache(); // for conversation store
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
| **Unit** | `ConciergeService.ProcessMessageAsync` with mocked BLL services; `ToolExecutor` dispatch; `PromptBuilder` context injection |
| **Integration** | Full flow: Controller → ConciergeService → OrderService → Repository → DB (Testcontainers PostgreSQL) |
| **E2E** | Cypress/Playwright: Guest logs in → opens chat → sends "burger + towels" → verifies Kitchen/Housekeeping dashboards receive SignalR alerts |
| **Load** | k6 script: 50 concurrent guests chatting → verify <2s p95 latency, no OpenAI quota exhaustion |

---

## 10. Rollout Plan (Phased)

| Phase | Scope | Duration |
|-------|-------|----------|
| **P0: Core Engine** | `IConciergeService`, tool definitions, 4 tools (food, housekeeping, maintenance, booking info), in-memory conversation store | 3 days |
| **P1: API + SSE** | `ConciergeController` with streaming, rate limiting, OpenAI DI | 1 day |
| **P2: Frontend Chat** | `ConciergeChatComponent`, quick actions, context bar, action chips | 2 days |
| **P3: Polish** | Menu browsing (carousel), order history, preference learning (store inferred prefs), multi-language (i18n keys) | 2 days |
| **P4: Demo Hardening** | Seed data scripts (3 guests with rich histories), staff dashboard projection mode, failure injection tests | 1 day |

**Total: ~9 working days (2 weeks) for production-ready MVP**

---

## 11. Demo Script (Jaw-Dropping Flow)

> **Setup**: Three screens projected — Guest Mobile, Kitchen Dashboard, Housekeeping/Maintenance Dashboard

| Time | Guest (Mobile) | Kitchen Screen | Housekeeping Screen | Maintenance Screen |
|------|----------------|----------------|---------------------|-------------------|
| 0:00 | Opens chat, sees context: "Booking #1042 • Room 304 • Checked In" | — | — | — |
| 0:05 | Types: *"I just checked in. Can you send up a burger, some extra pillows, and also my TV isn't working?"* | — | — | — |
| 0:07 | **AI replies instantly**: *"Absolutely! Placing your burger order, requesting extra pillows, and logging a maintenance ticket for the TV. All three teams have been notified."* | 🔔 **"New order: Room 304 — Burger ×1"** | 🔔 **"New task: Room 304 — Extra pillows"** | 🔔 **"URGENT: Room 304 — TV not working"** |
| 0:10 | Guest sees action chips: ✅ Order #57 placed • ✅ Housekeeping request created • ✅ Maintenance ticket #12 created | Chef taps "Preparing" | Staff taps "Assigned" | Tech taps "En route" |
| 0:15 | Guest: *"What's my checkout time?"* | — | — | — |
| 0:16 | AI: *"Check-out is 11:00 AM on March 20th. Your current folio balance is $342.50."* | — | — | — |

**No front desk call. No wait. Three departments mobilized in one sentence.**

---

## 12. Future Extensions (Post-MVP)

| Feature | Description |
|---------|-------------|
| **Proactive Nudges** | "It's 7:30 AM — your usual latte?" (uses Personalization Engine prefs) |
| **Voice Input** | Web Speech API → STT → same pipeline |
| **Multilingual** | Detect language → reply in guest's language (OpenAI supports 50+) |
| **Upsell Suggestions** | "Would you like to add our signature dessert for $8?" |
| **Integration with PMS** | Push folio to Opera/Cloudbeds, sync room status |
| **Analytics Dashboard** | Intent classification, resolution rate, guest satisfaction per conversation |

---

## 13. File Checklist (What to Create)

| Path | Purpose |
|------|---------|
| `Backend/HotelManagement.BLL/DTOs/ConciergeDTOs.cs` | Request/response/tool DTOs |
| `Backend/HotelManagement.BLL/Interfaces/IConciergeService.cs` | Service contract |
| `Backend/HotelManagement.BLL/Services/Concierge/ConciergeService.cs` | Main orchestration |
| `Backend/HotelManagement.BLL/Services/Concierge/ToolExecutor.cs` | Tool dispatch |
| `Backend/HotelManagement.BLL/Services/Concierge/ToolDefinitions.cs` | OpenAI function schemas |
| `Backend/HotelManagement.BLL/Services/Concierge/PromptBuilder.cs` | System prompt construction |
| `Backend/HotelManagement.BLL/Services/Concierge/ConversationStore.cs` | Conversation persistence |
| `Backend/HotelManagement.BLL/Options/OpenAIOptions.cs` | Config |
| `Backend/HotelManagement.API/Controllers/ConciergeController.cs` | HTTP endpoint |
| `Frontend/src/app/features/user/services/concierge-api.service.ts` | Angular API client |
| `Frontend/src/app/features/user/components/concierge-chat/concierge-chat.component.ts` | Chat UI |
| `Frontend/src/app/features/user/components/concierge-chat/concierge-chat.component.html` | Template |
| `Frontend/src/app/features/user/components/concierge-chat/concierge-chat.component.scss` | Styles |

---

## 14. Dependencies to Add

```xml
<!-- HotelManagement.BLL.csproj -->
<PackageReference Include="OpenAI" Version="2.0.0" />
<PackageReference Include="Microsoft.Extensions.Caching.Memory" Version="9.0.0" />
<PackageReference Include="AspNetCoreRateLimit" Version="5.0.0" />
```

---

**End of Design Document**
