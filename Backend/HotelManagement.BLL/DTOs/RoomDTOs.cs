using System.ComponentModel.DataAnnotations;

namespace HotelManagement.BLL.DTOs;

public class RoomDTO
{
    public int Id { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string RoomTypeName { get; set; } = string.Empty;
    public int RoomTypeId { get; set; }
    public decimal BasePrice { get; set; }
    public int MaxOccupancy { get; set; }
    public bool IsAvailable { get; set; }
}

public class CreateUpdateRoomDTO
{
    [Required]
    [StringLength(100)]
    public string RoomNumber { get; set; } = string.Empty;
    public int RoomTypeId { get; set; }
    public bool? IsActive { get; set; }
}
