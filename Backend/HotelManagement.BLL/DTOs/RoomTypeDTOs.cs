using System.ComponentModel.DataAnnotations;

namespace HotelManagement.BLL.DTOs;

public class RoomTypeDTO
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required, Range(0, double.MaxValue)]
    public decimal BasePrice { get; set; }

    public int MaxOccupancy { get; set; }

    public List<string>? ImageUrls { get; set; }

    public int? SquareFootage { get; set; }

    public Dictionary<string, int>? BedConfiguration { get; set; }
    public bool IsActive { get; set; }
}

public class RoomTypeAvailabilityDTO
{
    public int RoomTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public int MaxOccupancy { get; set; }
    public string? Description { get; set; }
    public List<string>? ImageUrls { get; set; }
    public int? SquareFootage { get; set; }
    public Dictionary<string, int>? BedConfiguration { get; set; }

    public int AvailableCount { get; set; }
}

public class UpdateRoomTypeDTO
{
    [StringLength(100)]
    public string? Name { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? BasePrice { get; set; }

    public int? MaxOccupancy { get; set; }
    public List<string>? ImageUrls { get; set; }
    public int? SquareFootage { get; set; }

    public Dictionary<string, int>? BedConfiguration { get; set; }
    public bool? IsActive { get; set; }
}

public class CreateRoomTypeDTO
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal BasePrice { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int MaxOccupancy { get; set; }

    public List<string>? ImageUrls { get; set; }

    public int? SquareFootage { get; set; }

    public Dictionary<string, int>? BedConfiguration { get; set; }
}
