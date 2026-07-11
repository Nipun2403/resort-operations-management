using HotelManagement.DAL.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace HotelManagement.DAL.Entities;

public class MaintenanceTask
{
    public int Id { get; set; }
    
    public int? RoomId { get; set; }
    public Room? Room { get; set; }

    [MaxLength(200)]
    public string? Location { get; set; }

    public MaintenanceOriginType OriginType { get; set; }
    public MaintenanceStatus Status { get; set; } = MaintenanceStatus.Pending;

    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsEmergency { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
}
