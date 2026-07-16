using System.ComponentModel.DataAnnotations;

namespace HotelManagement.BLL.DTOs;

public class FeedbackReminderValidationDTO
{
    public bool IsValid { get; set; }
    public int BookingId { get; set; }
    public string GuestName { get; set; } = string.Empty;
    public bool AlreadySubmitted { get; set; }
    public string? ErrorReason { get; set; }
}

public class SubmitFeedbackByTokenDTO
{
    [Required]
    public Guid Token { get; set; }

    [Required, Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
    public int Rating { get; set; }

    [StringLength(500)]
    public string Comments { get; set; } = string.Empty;
}
