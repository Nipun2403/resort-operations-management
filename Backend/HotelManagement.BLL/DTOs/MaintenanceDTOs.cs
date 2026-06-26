using HotelManagement.DAL.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace HotelManagement.BLL.DTOs;

public class MaintenanceTaskDTO
{
    public int Id { get; set; }
    public int? RoomId { get; set; }
    public string? Location { get; set; }
    public string OriginType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
    public string? StartedAt { get; set; }
    public string? FinishedAt { get; set; }
}

public class CreateInternalMaintenanceTaskDTO
{
    [Required]
    [MaxLength(200)]
    public string Location { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
}

public class CreateMaintenanceTaskDTO
{
    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
}

public class UpdateMaintenanceStatusDTO
{
    [Required]
    public MaintenanceStatus Status { get; set; }
}
