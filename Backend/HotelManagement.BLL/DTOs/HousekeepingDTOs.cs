using HotelManagement.DAL.Enums;
using System.ComponentModel.DataAnnotations;
namespace HotelManagement.BLL.DTOs;

public class HousekeepingDTO
{
    public int Id { get; set; }
    public int? RoomId { get; set; }
    public string? Location { get; set; }
    public string? Description { get; set; }
    public HousekeepingOriginType OriginType { get; set; }
    public HousekeepingStatus Status { get; set; }
    public string? StartedAt { get; set; }
    public string? FinishedAt { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
    public bool IsEmergency { get; set; }
}

public class CreateHousekeepingTaskDTO
{
    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public bool IsEmergency { get; set; }
}

public class CreateInternalHousekeepingTaskDTO
{
    [Required]
    [MaxLength(200)]
    public string Location { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public bool IsEmergency { get; set; }
}

public class UpdateHousekeepingStatusDTO
{
    public HousekeepingStatus Status { get; set; }
}
