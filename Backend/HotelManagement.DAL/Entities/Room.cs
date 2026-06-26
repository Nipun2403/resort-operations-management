using System.ComponentModel.DataAnnotations;

namespace HotelManagement.DAL.Entities;
public class Room
{
    public int Id { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public int RoomTypeId { get; set; }
    public RoomType RoomType { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public ICollection<BookingRoom> BookingRooms { get;   set; } = new List<BookingRoom>();
    public ICollection<Housekeeping> HousekeepingTasks { get;   set; } = new List<Housekeeping>();

    [Timestamp]
    public byte[] RowVersion { get; set; } = null!;
}
