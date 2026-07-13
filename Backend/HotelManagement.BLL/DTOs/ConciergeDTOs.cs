using System.Text.Json;

namespace HotelManagement.BLL.DTOs;

public class ConciergeChatRequestDTO
{
    public string Message { get; set; } = string.Empty;
    public string? ConversationId { get; set; }
}

public class ConciergeConfirmRequestDTO
{
    public string ConversationId { get; set; } = string.Empty;
    public List<string> ProposalIds { get; set; } = new();
}

public class ConciergeChatResponseDTO
{
    public string Reply { get; set; } = string.Empty;
    public string ConversationId { get; set; } = string.Empty;
    public List<ConciergeProposalDTO> Proposals { get; set; } = new();
    public List<ConciergeActionResultDTO> Actions { get; set; } = new();
    public bool IsComplete { get; set; } = true;
}

public class ConciergeProposalDTO
{
    public string ProposalId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string ArgumentsJson { get; set; } = "{}";
    public DateTime ExpiresAt { get; set; }
}

public class ConciergeActionResultDTO
{
    public string ToolCallId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ResultSummary { get; set; }
    public string? Error { get; set; }
}

public class GuestContextDTO
{
    public int? BookingId { get; set; }
    public int? RoomId { get; set; }
    public string? RoomNumber { get; set; }
    public int UserId { get; set; }
    public DateTime? CheckInDate { get; set; }
    public DateTime? CheckOutDate { get; set; }
    public string BookingStatus { get; set; } = string.Empty;
    public List<MenuItemSummaryDTO> RecentOrders { get; set; } = new();
    public List<GuestPreferenceDTO> Preferences { get; set; } = new();
}

public class MenuItemSummaryDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class GuestPreferenceDTO
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class ConversationTurn
{
    public string UserMessage { get; set; } = "";
    public string AssistantReply { get; set; } = "";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class ConciergeErrorResponseDTO
{
    public string ErrorCode { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, string[]>? Details { get; set; }
    public string? TraceId { get; set; }
}