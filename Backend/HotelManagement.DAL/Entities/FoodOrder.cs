using HotelManagement.DAL.Enums;
namespace HotelManagement.DAL.Entities;
public class FoodOrder
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;

    public int? RoomId { get; set; }
    public Room? Room { get; set; }

    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public DateTime? FinishedAt { get; set; }
    public FoodOrderStatus OrderStatus { get; set; }
    public ICollection<FoodOrderItem> OrderItems { get;   set; } = new List<FoodOrderItem>();
}
