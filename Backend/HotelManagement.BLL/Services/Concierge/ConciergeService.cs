using System.ClientModel;
using System.Text.Json;
using HotelManagement.BLL.DTOs;
using HotelManagement.BLL.Exceptions;
using HotelManagement.BLL.Interfaces;
using HotelManagement.BLL.Options;
using HotelManagement.DAL.Entities;
using HotelManagement.DAL.Enums;
using HotelManagement.Repository.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;
using OpenAI;

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
    private readonly IConversationRepository _conversationRepo;
    private readonly IConciergeProposalRepository _proposalRepo;
    private readonly HotelManagement.Repository.Interfaces.IConciergeActionLogRepository _auditLog;
    private readonly IOptions<OpenAIOptions> _openAIOptions;
    private readonly ILogger<ConciergeService> _logger;

    private readonly ChatClient _chatClient;

    private static readonly string[] s_invalidFoodOrderError = new[] { "Invalid food order arguments." };

    public ConciergeService(
        ICurrentUserService currentUser,
        IBookingService bookingService,
        IOrderService orderService,
        IHousekeepingService housekeepingService,
        IMaintenanceService maintenanceService,
        IBillingService billingService,
        IMenuItemRepository menuItemRepository,
        IFoodOrderRepository foodOrderRepository,
        IConversationRepository conversationRepo,
        IConciergeProposalRepository proposalRepo,
        HotelManagement.Repository.Interfaces.IConciergeActionLogRepository auditLog,
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
        _conversationRepo = conversationRepo;
        _proposalRepo = proposalRepo;
        _auditLog = auditLog;
        _openAIOptions = openAIOptions;
        _logger = logger;

        _chatClient = string.IsNullOrWhiteSpace(_openAIOptions.Value.Endpoint)
            ? new ChatClient(_openAIOptions.Value.Model, new ApiKeyCredential(_openAIOptions.Value.ApiKey))
            : new ChatClient(
                _openAIOptions.Value.Model,
                new ApiKeyCredential(_openAIOptions.Value.ApiKey),
                new OpenAIClientOptions { Endpoint = new Uri(_openAIOptions.Value.Endpoint) });
    }

    public async Task<ConciergeChatResponseDTO> ProcessMessageAsync(string userMessage, string? conversationId, CancellationToken ct)
    {
        var userId = _currentUser.GetUserId() ?? 0;
        if (userId == 0) 
        {
            _logger.LogWarning("Concierge chat attempted by unauthenticated user");
            return new ConciergeChatResponseDTO { Reply = "Please log in to use the concierge.", IsComplete = false };
        }

        var convId = conversationId ?? Guid.NewGuid().ToString();
        var convKey = $"concierge:conv:{userId}:{convId}";
        var historyMessages = await _conversationRepo.GetRecentAsync(userId, convKey, 16);
        var history = historyMessages.ToList();

        _logger.LogInformation("Concierge chat started: user={UserId}, conv={ConvId}, messageLength={Length}", 
            userId, convId, userMessage.Length);

        var context = await BuildGuestContextAsync(ct);
        var sanitized = InputSanitizer.Sanitize(userMessage);

        var systemPrompt = PromptBuilder.BuildSystemPrompt(context);

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemPrompt)
        };

        // Build conversation history from entities (alternating user/assistant)
        for (int i = 0; i < history.Count - 1; i += 2)
        {
            if (history[i].Role == "user" && history[i + 1].Role == "assistant")
            {
                messages.Add(new UserChatMessage(history[i].Content));
                messages.Add(new AssistantChatMessage(history[i + 1].Content));
            }
        }

        messages.Add(new UserChatMessage(sanitized));

        var options = new ChatCompletionOptions
        {
            ToolChoice = ChatToolChoice.CreateAutoChoice(),
            Temperature = 0.3f
        };
        foreach (var t in ConciergeTools.Definitions)
            options.Tools.Add(t);

        var proposals = new List<ConciergeProposalDTO>();
        var actions = new List<ConciergeActionResultDTO>();

        var loopCount = 0;
        var maxLoops = 5;
        var requiresAnotherCompletion = true;
        string finalReply = string.Empty;

        while (requiresAnotherCompletion && loopCount < maxLoops)
        {
            loopCount++;
            var completion = await _chatClient.CompleteChatAsync(messages, options, ct);
            var response = completion.Value;

            var toolCalls = response.ToolCalls.ToList();
            string cleanedContent = response.Content.Count > 0 ? (response.Content[0]?.Text ?? string.Empty) : string.Empty;

            if (toolCalls.Count == 0 && cleanedContent.Contains("<tool_call", StringComparison.Ordinal))
            {
                toolCalls = ParseTextToolCalls(cleanedContent, out var cleaned);
                cleanedContent = cleaned;
            }

            if (toolCalls.Count == 0)
            {
                finalReply = cleanedContent;
                requiresAnotherCompletion = false;
            }
            else
            {
                if (toolCalls.Count > ConciergeTools.MaxToolCallsPerTurn)
                {
                    toolCalls = toolCalls.Take(ConciergeTools.MaxToolCallsPerTurn).ToList();
                }

                AssistantChatMessage assistantMsg = response.ToolCalls.Count > 0
                    ? new AssistantChatMessage(response)
                    : new AssistantChatMessage(toolCalls);

                messages.Add(assistantMsg);

                foreach (var call in toolCalls)
                {
                    _logger.LogDebug("Tool call: {FunctionName}", call.FunctionName);
                    if (ConciergeTools.SideEffectToolNames.Contains(call.FunctionName))
                    {
                        var proposal = await CreateProposalAsync(convId, call, context, ct);
                        proposals.Add(proposal);
                        _logger.LogInformation("Created proposal: {ProposalId} for {FunctionName}", 
                            proposal.ProposalId, call.FunctionName);
                        
                        messages.Add(new ToolChatMessage(call.Id, $"Proposal created (pending confirmation): {proposal.Summary}."));
                    }
                    else
                    {
                        var result = await ToolExecutor.ExecuteAsync(call, context, this, ct);
                        actions.Add(result);
                        await LogActionAsync(convId, userId, sanitized, call, result, ct);
                        _logger.LogInformation("Executed read-only tool: {FunctionName}, success={Success}", 
                            call.FunctionName, result.Success);

                        messages.Add(new ToolChatMessage(call.Id, $"{(result.Success ? "OK" : "FAIL")}: {result.ResultSummary ?? result.Error}"));
                    }
                }
            }
        }

        if (string.IsNullOrEmpty(finalReply))
        {
            finalReply = "I have processed your requests. Please let me know how you'd like to proceed.";
        }

        await _conversationRepo.AddRangeAsync(userId, convKey, new[]
        {
            new ConversationMessage { UserId = userId, ConversationId = convKey, Role = "user", Content = sanitized, CreatedAt = DateTime.UtcNow },
            new ConversationMessage { UserId = userId, ConversationId = convKey, Role = "assistant", Content = finalReply, CreatedAt = DateTime.UtcNow }
        });

        return new ConciergeChatResponseDTO
        {
            Reply = finalReply,
            ConversationId = convId,
            Proposals = proposals,
            Actions = actions,
            IsComplete = true
        };
    }

    public async Task<ConciergeChatResponseDTO> ConfirmProposalsAsync(string conversationId, List<string> proposalIds, CancellationToken ct)
    {
        var userId = _currentUser.GetUserId() ?? 0;
        if (userId == 0) return new ConciergeChatResponseDTO { Reply = "Please log in.", IsComplete = false };

        _logger.LogInformation("Confirming proposals: user={UserId}, conv={ConversationId}, proposalCount={Count}", 
            userId, conversationId, proposalIds.Count);

        var convKey = $"concierge:conv:{userId}:{conversationId}";

        var actions = new List<ConciergeActionResultDTO>();

        var guids = proposalIds.Select(Guid.Parse).ToList();
        var proposals = await _proposalRepo.GetByIdsAsync(guids, userId, conversationId);

        if (proposals.Count < guids.Count)
        {
            throw new KeyNotFoundException("One or more proposals were not found.");
        }

        var invalid = proposals.Where(p => p.Status != "pending" || p.ExpiresAt < DateTime.UtcNow).ToList();
        if (invalid.Any())
        {
            _logger.LogWarning("Proposal validation failed: user={UserId}, conv={ConversationId}, invalidCount={Count}", 
                userId, conversationId, invalid.Count);
            throw new ConciergeProposalExpiredException("One or more proposals have expired or are no longer valid.");
        }

        foreach (var proposal in proposals)
        {
            try
            {
                await ValidateToolArgsAsync(proposal.ToolName, proposal.ArgumentsJson, ct);
            }
            catch (ConciergeValidationException ex)
            {
                _logger.LogWarning("Proposal validation failed: user={UserId}, tool={ToolName}, error={Error}", 
                    userId, proposal.ToolName, ex.Message);
                actions.Add(new ConciergeActionResultDTO
                {
                    Action = proposal.ToolName,
                    Success = false,
                    Error = ex.Message
                });
                continue;
            }

            var context = await BuildGuestContextAsync(ct);
            var call = ChatToolCall.CreateFunctionToolCall(proposal.Id.ToString(), proposal.ToolName, BinaryData.FromString(proposal.ArgumentsJson));
            var result = await ToolExecutor.ExecuteAsync(call, context, this, ct);
            actions.Add(result);

            await LogActionAsync(conversationId, userId, "(confirmed)", call, result, ct);
        }

        _logger.LogInformation("Proposals confirmed: user={UserId}, conv={ConversationId}, actionsCount={Count}, successCount={SuccessCount}", 
            userId, conversationId, actions.Count, actions.Count(a => a.Success));

        await _proposalRepo.MarkConfirmedAsync(proposalIds, userId, conversationId);

        var summaryParts = actions.Select(a => $"{(a.Success ? "OK" : "FAIL")}: {a.ResultSummary ?? a.Error}");
        var summaryPrompt = $"The following actions were executed:\n{string.Join("\n", summaryParts)}\n\nSummarize what was accomplished in a warm, friendly way.";
var summaryCompletion = await _chatClient.CompleteChatAsync(
            new[] { new SystemChatMessage(summaryPrompt) }, 
            options: new ChatCompletionOptions { Temperature = 0.3f }, 
            cancellationToken: ct);

        var reply = summaryCompletion.Value.Content[0].Text;

        await _conversationRepo.AddRangeAsync(userId, convKey, new[]
        {
            new ConversationMessage { UserId = userId, ConversationId = convKey, Role = "user", Content = "Confirmed proposals", CreatedAt = DateTime.UtcNow },
            new ConversationMessage { UserId = userId, ConversationId = convKey, Role = "assistant", Content = reply, CreatedAt = DateTime.UtcNow }
        });

        return new ConciergeChatResponseDTO
        {
            Reply = reply,
            ConversationId = conversationId,
            Actions = actions,
            IsComplete = true
        };
    }

    public async Task<List<ConciergeProposalDTO>> GetPendingProposalsAsync(string conversationId, CancellationToken ct)
    {
        var userId = _currentUser.GetUserId() ?? 0;
        if (userId == 0) return new List<ConciergeProposalDTO>();
        var entities = await _proposalRepo.GetByIdsAsync(new(), userId, conversationId);
        return entities.Select(e => new ConciergeProposalDTO
        {
            ProposalId = e.Id.ToString(),
            Action = e.ToolName,
            Summary = e.Summary,
            ArgumentsJson = e.ArgumentsJson,
            ExpiresAt = e.ExpiresAt
        }).ToList();
    }

    public async Task<GuestContextDTO> GetGuestContextAsync(CancellationToken ct) => await BuildGuestContextAsync(ct);

    public async Task<ConciergeActionResultDTO> CreateFoodOrderAsync(CreateFoodOrderToolArgs args, GuestContextDTO ctx, CancellationToken ct)
    {
        if (ctx.BookingId == null || ctx.RoomId == null)
            return Fail("No active booking found. Please check in first.");

        if (ctx.BookingStatus != "CheckedIn")
            return Fail("Room service is only available for checked-in guests.");

        foreach (var item in args.Items)
        {
            var menuItem = await _menuItemRepository.GetByIdAsync(item.MenuItemId);
            if (menuItem == null) return Fail($"Menu item #{item.MenuItemId} not found.");
            if (!menuItem.IsAvailable) return Fail($"'{menuItem.Name}' is currently unavailable.");
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
        await _housekeepingService.CreateGuestTriggerAsync(ctx.RoomId.Value, dto);
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
        if (booking == null) return Fail("Booking not found.");
        var json = JsonSerializer.Serialize(new
        {
            booking.Id, booking.CheckInDate, booking.CheckOutDate, booking.BookingStatus,
            RoomNumber = ctx.RoomNumber, booking.GuestName
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
        var tasks = await _housekeepingService.GetActiveTasksAsync(1, 20, ctx.RoomId.Value);
        var json = JsonSerializer.Serialize(tasks.Data.Select(t => new { t.Id, t.Description, t.Status, t.CreatedAt }));
        return Success(json);
    }

    public async Task<ConciergeActionResultDTO> GetMenuItemsAsync(GetMenuItemsToolArgs args, CancellationToken ct)
    {
        var items = await _menuItemRepository.GetPaginatedMenuItemsAsync(1, 50, args.AvailableOnly, args.Category, args.Search, "", false);
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

    private async Task<ConciergeProposalDTO> CreateProposalAsync(string convId, ChatToolCall call, GuestContextDTO ctx, CancellationToken ct)
    {
        var args = call.FunctionArguments.ToString();
        var summary = call.FunctionName switch
        {
            "CreateFoodOrder" => await SummarizeFoodOrderAsync(args, ct),
            "CreateHousekeepingRequest" => await SummarizeHousekeepingAsync(args, ct),
            "CreateMaintenanceTicket" => await SummarizeMaintenanceAsync(args, ct),
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

        var entity = new ConciergeProposal
        {
            Id = Guid.Parse(proposal.ProposalId),
            ConversationId = convId,
            UserId = ctx.UserId,
            ToolName = proposal.Action,
            ArgumentsJson = proposal.ArgumentsJson,
            Summary = proposal.Summary,
            Status = "pending",
            ExpiresAt = proposal.ExpiresAt,
            CreatedAt = DateTime.UtcNow
        };

        await _proposalRepo.SaveAsync(entity, ctx.UserId, convId);
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
        catch { return "Order items"; }
    }

    private static Task<string> SummarizeHousekeepingAsync(string argsJson, CancellationToken ct)
    {
        try
        {
            var args = JsonSerializer.Deserialize<CreateHousekeepingToolArgs>(argsJson);
            if (args == null) return Task.FromResult("Housekeeping request");
            var prefix = args.IsEmergency ? "URGENT: " : "";
            return Task.FromResult($"{prefix}{args.Description}");
        }
        catch { return Task.FromResult("Housekeeping request"); }
    }

    private static Task<string> SummarizeMaintenanceAsync(string argsJson, CancellationToken ct)
    {
        try
        {
            var args = JsonSerializer.Deserialize<CreateMaintenanceToolArgs>(argsJson);
            if (args == null) return Task.FromResult("Maintenance ticket");
            var prefix = args.IsEmergency ? "URGENT: " : "";
            return Task.FromResult($"{prefix}{args.Description}");
        }
        catch { return Task.FromResult("Maintenance ticket"); }
    }

    private async Task ValidateToolArgsAsync(string toolName, string argsJson, CancellationToken ct)
    {
        try
        {
            if (toolName == "CreateFoodOrder")
                await ValidateFoodOrderArgsAsync(argsJson, ct);
        }
        catch (JsonException)
        {
            throw new ConciergeValidationException($"Invalid arguments for {toolName}.");
        }
    }

    private async Task ValidateFoodOrderArgsAsync(string argsJson, CancellationToken ct)
    {
        var args = JsonSerializer.Deserialize<CreateFoodOrderToolArgs>(argsJson);
        if (args == null)
            throw new ConciergeValidationException("Invalid food order arguments.", 
                new Dictionary<string, string[]> { ["items"] = s_invalidFoodOrderError });

        var errors = new Dictionary<string, string[]>();
        var itemErrors = new List<string>();

        foreach (var item in args.Items)
        {
            var menuItem = await _menuItemRepository.GetByIdAsync(item.MenuItemId);
            if (menuItem == null)
                itemErrors.Add($"Menu item #{item.MenuItemId} not found.");
            else if (!menuItem.IsAvailable)
                itemErrors.Add($"'{menuItem.Name}' is currently unavailable.");
        }

        if (itemErrors.Count > 0)
        {
            errors["items"] = itemErrors.ToArray();
        }

        if (errors.Count > 0)
            throw new ConciergeValidationException("Food order validation failed.", errors);
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

        var userId = _currentUser.GetUserId() ?? 0;
        var user = await _bookingService.GetUserByEmailAsync(email);
        if (user == null) return new GuestContextDTO();

        var today = DateTime.UtcNow.Date;
        var bookings = await _bookingService.GetPaginatedBookingsWithDetailsAsync(1, 5, new List<System.Linq.Expressions.Expression<Func<Booking, bool>>>
        {
            b => b.UserId == user.Id &&
                 (b.BookingStatus == BookingStatus.CheckedIn ||
                  (b.BookingStatus == BookingStatus.Booked && b.CheckInDate.Date == today))
        });

        var active = bookings.Data.FirstOrDefault();
        if (active == null) return new GuestContextDTO { UserId = userId };

        var roomDto = active.Rooms.FirstOrDefault(r => r.RoomId.HasValue);
        var roomId = roomDto?.RoomId;
        var roomNumber = roomDto?.RoomNumber;

        return new GuestContextDTO
        {
            UserId = userId,
            BookingId = active.Id,
            RoomId = roomId,
            RoomNumber = roomNumber,
            CheckInDate = active.CheckInDate,
            CheckOutDate = active.CheckOutDate,
            BookingStatus = active.BookingStatus.ToString()
        };
    }

    private static ConciergeActionResultDTO Success(string summary) => new() { Success = true, ResultSummary = summary };
    private static ConciergeActionResultDTO Fail(string error) => new() { Success = false, Error = error };

    private static List<ChatToolCall> ParseTextToolCalls(string text, out string cleanedText)
    {
        var toolCalls = new List<ChatToolCall>();
        cleanedText = text;

        if (string.IsNullOrEmpty(text)) return toolCalls;

        var startIndex = text.IndexOf("<tool_call", StringComparison.Ordinal);
        if (startIndex == -1) return toolCalls;

        cleanedText = text.Substring(0, startIndex).Trim();

        var toolCallRegex = new System.Text.RegularExpressions.Regex(
            @"<tool_call:(?<id>[^>]+)>(?<name>[a-zA-Z0-9_]+)",
            System.Text.RegularExpressions.RegexOptions.Compiled);

        var matches = toolCallRegex.Matches(text);
        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            var id = match.Groups["id"].Value;
            var name = match.Groups["name"].Value;

            var afterDeclaration = text.Substring(match.Index + match.Length);
            var jsonArgs = "{}";

            var nextTagIndex = afterDeclaration.IndexOf('<');
            var firstBraceIndex = afterDeclaration.IndexOf('{');
            var firstParenIndex = afterDeclaration.IndexOf('(');

            if (firstBraceIndex != -1 && (nextTagIndex == -1 || firstBraceIndex < nextTagIndex))
            {
                var braceCount = 0;
                var braceEnd = -1;
                for (int i = firstBraceIndex; i < afterDeclaration.Length; i++)
                {
                    if (afterDeclaration[i] == '{') braceCount++;
                    else if (afterDeclaration[i] == '}')
                    {
                        braceCount--;
                        if (braceCount == 0)
                        {
                            braceEnd = i;
                            break;
                        }
                    }
                }

                if (braceEnd != -1)
                {
                    jsonArgs = afterDeclaration.Substring(firstBraceIndex, braceEnd - firstBraceIndex + 1);
                }
            }
            else if (firstParenIndex != -1 && (nextTagIndex == -1 || firstParenIndex < nextTagIndex))
            {
                var parenEnd = afterDeclaration.IndexOf(')');
                if (parenEnd != -1 && parenEnd > firstParenIndex)
                {
                    var argsContent = afterDeclaration.Substring(firstParenIndex + 1, parenEnd - firstParenIndex - 1);
                    var dict = new Dictionary<string, object>();
                    var pairs = argsContent.Split(',');
                    foreach (var pair in pairs)
                    {
                        var kv = pair.Split('=');
                        if (kv.Length == 2)
                        {
                            var key = kv[0].Trim();
                            var valStr = kv[1].Trim();
                            if (valStr.StartsWith('\'') && valStr.EndsWith('\'') && valStr.Length > 1)
                            {
                                dict[key] = valStr.Substring(1, valStr.Length - 2);
                            }
                            else if (valStr.StartsWith('"') && valStr.EndsWith('"') && valStr.Length > 1)
                            {
                                dict[key] = valStr.Substring(1, valStr.Length - 2);
                            }
                            else if (bool.TryParse(valStr, out var bVal))
                            {
                                dict[key] = bVal;
                            }
                            else if (int.TryParse(valStr, out var iVal))
                            {
                                dict[key] = iVal;
                            }
                            else if (decimal.TryParse(valStr, out var dVal))
                            {
                                dict[key] = dVal;
                            }
                            else
                            {
                                dict[key] = valStr;
                            }
                        }
                    }
                    jsonArgs = JsonSerializer.Serialize(dict);
                }
            }

            var call = ChatToolCall.CreateFunctionToolCall(id, name, BinaryData.FromString(jsonArgs));
            toolCalls.Add(call);
        }

        return toolCalls;
    }
}