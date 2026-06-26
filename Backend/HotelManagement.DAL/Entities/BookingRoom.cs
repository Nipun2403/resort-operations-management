using System.ComponentModel.DataAnnotations;

namespace HotelManagement.DAL.Entities;

public class BookingRoom
{
    public int Id { get; set; }

    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;

    public int RoomTypeId { get; set; }
    public RoomType RoomType { get; set; } = null!;

    public int? RoomId { get; set; }
    public Room? Room { get; set; }

    public decimal LockedInPrice { get; set; }
}
