namespace HotelManagement.DAL.Entities;
public class Feedback
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;
    public int Rating { get; set; }
    public string Comments { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsHidden { get; set; }
}
