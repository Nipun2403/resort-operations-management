using System.ComponentModel.DataAnnotations;

namespace HotelManagement.BLL.DTOs;

public class FeedbackDTO
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public int Rating { get; set; }
    public string Comments { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
    public bool IsHidden { get; set; }
}

public class CreateFeedbackDTO
{
    [Required]
    public int BookingId { get; set; }
    
    [Required, Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
    public int Rating { get; set; }
    
    [StringLength(500)]
    public string Comments { get; set; } = string.Empty;
}

public class ModerateFeedbackRequestDTO
{
    [Required]
    public bool IsHidden { get; set; }
}
