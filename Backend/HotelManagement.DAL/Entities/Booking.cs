using HotelManagement.DAL.Enums;
using System.ComponentModel.DataAnnotations;

namespace HotelManagement.DAL.Entities;
public class Booking
{
    public int Id { get; set; }
    
    public ICollection<BookingRoom> BookingRooms { get;   set; } = new List<BookingRoom>();
    
    public int GuestCount { get; set; } = 1;
    
    public string GuestName { get; set; } = string.Empty;
    public string GuestEmail { get; set; } = string.Empty;
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public BookingStatus BookingStatus { get; set; }
    public ICollection<FoodOrder> FoodOrders { get;   set; } = new List<FoodOrder>();
    public ICollection<BookingAmenity> BookingAmenities { get;   set; } = new List<BookingAmenity>();
    public Feedback? Feedback { get; set; }

    public int? UserId { get; set; }
    public User? User { get; set; }
    public BookingOrigin Origin { get; set; }
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public PaymentStatus ServicePaymentStatus { get; set; } = PaymentStatus.Pending;
    public DateTime BookedAt { get; set; } = DateTime.UtcNow;

    [Timestamp]
    public byte[] RowVersion { get; set; } = null!;
}
