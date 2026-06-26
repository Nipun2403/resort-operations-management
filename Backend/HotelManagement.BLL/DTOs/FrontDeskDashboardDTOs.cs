namespace HotelManagement.BLL.DTOs;

public class GuestSearchDTO
{
    public string GuestName { get; set; } = string.Empty;
    public string GuestEmail { get; set; } = string.Empty;
    public int TotalStays { get; set; }
    public DateTime? LastCheckInDate { get; set; }
}

public class RoomStatusDashboardDTO
{
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string RoomTypeName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // "Available", "Occupied", "Reserved", "Maintenance"
    public string? CurrentGuestName { get; set; }
    public DateTime? NextCheckInDate { get; set; }
}
