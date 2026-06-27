namespace HotelManagement.DAL.Entities;

public class RoomType
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public int MaxOccupancy { get; set; }

    // Marketing & Metadata
    public string? Description { get; set; }
    public List<string>? ImageUrls { get; set; }
    public int? SquareFootage { get; set; }
    public string? BedConfigurationJson { get; set; }

    public bool IsActive { get; set; } = true;
    public ICollection<Room> Rooms { get; set; } = new List<Room>();
}
