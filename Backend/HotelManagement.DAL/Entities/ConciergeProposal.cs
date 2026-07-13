namespace HotelManagement.DAL.Entities;

public class ConciergeProposal
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ConversationId { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string ToolName { get; set; } = string.Empty;
    public string ArgumentsJson { get; set; } = "{}";
    public string Summary { get; set; } = string.Empty;
    public string Status { get; set; } = "pending"; // pending | confirmed | expired
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ConfirmedAt { get; set; }
}