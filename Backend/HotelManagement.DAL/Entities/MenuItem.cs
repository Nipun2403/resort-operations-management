namespace HotelManagement.DAL.Entities;
public class MenuItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }
}
