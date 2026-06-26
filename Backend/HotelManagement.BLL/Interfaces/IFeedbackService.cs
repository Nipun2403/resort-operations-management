using HotelManagement.BLL.DTOs;
using HotelManagement.Repository.Models;

namespace HotelManagement.BLL.Interfaces;

public interface IFeedbackService
{
    Task<FeedbackDTO> SubmitFeedbackAsync(CreateFeedbackDTO dto);
    Task<IEnumerable<FeedbackDTO>> GetFeedbackForBookingAsync(int bookingId);
    Task<PaginatedResult<FeedbackDTO>> GetFeedbackAsync(int pageNumber, int pageSize, bool includeHidden = false, string? sortBy = null, bool sortDescending = false);
    Task<FeedbackDTO> ModerateFeedbackAsync(int feedbackId, bool isHidden);
}
