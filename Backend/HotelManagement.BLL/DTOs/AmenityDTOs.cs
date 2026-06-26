using System.ComponentModel.DataAnnotations;

namespace HotelManagement.BLL.DTOs;

public class AmenityDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
}

public class CreateUpdateAmenityDTO
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required(AllowEmptyStrings = false)]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    [Range(0, 10000)]
    public decimal Price { get; set; }
}

public class SubscribeAmenityDTO
{
    [Required]
    public int AmenityId { get; set; }
}
