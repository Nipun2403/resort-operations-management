using System.ComponentModel.DataAnnotations;

namespace HotelManagement.DAL.Entities;

public class Receipt
{
    public int Id { get; set; }
    
    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;
    
    public decimal AmountPaid { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public DateTime PaidAt { get; set; } = DateTime.UtcNow;
}
