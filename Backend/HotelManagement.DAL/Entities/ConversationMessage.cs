namespace HotelManagement.DAL.Entities;

public class ConversationMessage
{
    public long Id { get; set; }
    public int UserId { get; set; }
    public string ConversationId { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // user | assistant | tool
    public string Content { get; set; } = string.Empty;
    public string? MetadataJson { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}