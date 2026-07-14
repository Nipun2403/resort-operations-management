namespace HotelManagement.DAL.Entities;
public class FeedbackReminder
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;
    public Guid Token { get; set; } = Guid.NewGuid();
    public DateTime TokenExpiresAt { get; set; }
    public DateTime? LastSentAt { get; set; }
    public int SentCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
