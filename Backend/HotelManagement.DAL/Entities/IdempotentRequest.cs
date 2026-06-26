namespace HotelManagement.DAL.Entities;

public class IdempotentRequest
{
    public string IdempotencyKey { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
