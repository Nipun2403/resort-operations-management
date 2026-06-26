using System;

namespace HotelManagement.DAL.Entities;

public class BookingAmenity
{
    public int Id { get; set; }
    
    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;
    
    public int AmenityId { get; set; }
    public Amenity Amenity { get; set; } = null!;
    
    public decimal PriceAtPurchase { get; set; }
    
    public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;
}
