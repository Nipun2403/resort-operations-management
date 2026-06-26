using HotelManagement.DAL.Entities;

namespace HotelManagement.Repository.Models;

public class RoomTypeAvailability
{
    public RoomType RoomType { get; set; } = null!;
    public int AvailableCount { get; set; }
}
