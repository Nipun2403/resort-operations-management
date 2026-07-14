using HotelManagement.BLL.DTOs;

namespace HotelManagement.BLL.Interfaces;

public interface IFeedbackReminderService
{
    Task ProcessDueRemindersAsync(CancellationToken ct);
    Task<FeedbackReminderValidationDTO> ValidateTokenAsync(Guid token);
    Task<FeedbackDTO> SubmitFeedbackWithTokenAsync(Guid token, int rating, string comments);
}
