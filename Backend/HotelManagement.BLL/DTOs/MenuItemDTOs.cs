using System.ComponentModel.DataAnnotations;

namespace HotelManagement.BLL.DTOs;

public class MenuItemDTO
{
    public int Id { get; set; }

    [Required(AllowEmptyStrings = false)]
    public string Name { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [Required(AllowEmptyStrings = false)]
    public string Category { get; set; } = string.Empty;

    public bool IsAvailable { get; set; }
}

public class CreateMenuItemDTO
{
    [Required(AllowEmptyStrings = false)]
    public string Name { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [Required(AllowEmptyStrings = false)]
    public string Category { get; set; } = string.Empty;

    public bool IsAvailable { get; set; }
}
