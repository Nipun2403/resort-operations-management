using System.ComponentModel.DataAnnotations;

namespace HotelManagement.DAL.Entities;

public class Amenity
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    public decimal Price { get; set; }
    
    public bool IsAvailable { get; set; } = true;
}
