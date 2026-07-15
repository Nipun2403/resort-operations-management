using System.Text.Json;

namespace HotelManagement.DAL.Entities;

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