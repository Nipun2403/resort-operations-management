using HotelManagement.DAL.Enums;
using System.ComponentModel.DataAnnotations;

namespace HotelManagement.DAL.Entities;
public class Housekeeping
{
    public int Id { get; set; }
    public int? RoomId { get; set; }
    public Room? Room { get; set; }
    
    [MaxLength(200)]
    public string? Location { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public HousekeepingOriginType OriginType { get; set; }
    public HousekeepingStatus Status { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsEmergency { get; set; }
}
