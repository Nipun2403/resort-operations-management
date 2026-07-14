using AutoMapper;
using HotelManagement.BLL.DTOs;
using HotelManagement.BLL.Interfaces;
using HotelManagement.DAL.Entities;
using HotelManagement.Repository.Interfaces;
using HotelManagement.Repository.Models;
using HotelManagement.Repository.Utilities;

namespace HotelManagement.BLL.Services;

public class FeedbackService : IFeedbackService
{
    private readonly IGenericRepository<Feedback> _feedbackRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public FeedbackService(IGenericRepository<Feedback> feedbackRepository, IBookingRepository bookingRepository, IMapper mapper, ICurrentUserService currentUserService)
    {
        _feedbackRepository = feedbackRepository;
        _bookingRepository = bookingRepository;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<PaginatedResult<FeedbackDTO>> GetFeedbackAsync(int pageNumber, int pageSize, bool includeHidden = false, string? sortBy = null, bool sortDescending = false)
    {
        System.Linq.Expressions.Expression<Func<Feedback, bool>>? filter = includeHidden ? null : f => !f.IsHidden;
        
        Func<IQueryable<Feedback>, IOrderedQueryable<Feedback>>? orderBy = null;
        if (!string.IsNullOrEmpty(sortBy))
        {
            orderBy = q => q.OrderByDynamic(sortBy, sortDescending);
        }

        var pagedFeedback = await _feedbackRepository.GetPaginatedResultAsync(pageNumber, pageSize, filter, orderBy);
        var dtos = _mapper.Map<IEnumerable<FeedbackDTO>>(pagedFeedback.Data);
        
        return new PaginatedResult<FeedbackDTO>
        {
            TotalCount = pagedFeedback.TotalCount,
            PageNumber = pagedFeedback.PageNumber,
            PageSize = pagedFeedback.PageSize,
            Data = dtos
        };
    }

    public async Task<IEnumerable<FeedbackDTO>> GetFeedbackForBookingAsync(int bookingId)
    {
        var feedback = await _feedbackRepository.FindAsync(f => f.BookingId == bookingId);
        return _mapper.Map<IEnumerable<FeedbackDTO>>(feedback);
    }

    public async Task<FeedbackDTO> SubmitFeedbackAsync(CreateFeedbackDTO dto)
    {
        var booking = await _bookingRepository.GetByIdAsync(dto.BookingId);
        if (booking == null) throw new KeyNotFoundException("Booking not found.");

        if (_currentUserService.IsInRole("RegisteredUser") && !_currentUserService.IsInRole("Admin") && !_currentUserService.IsInRole("FrontDesk"))
        {
            if (booking.GuestEmail != _currentUserService.GetUserEmail())
                throw new UnauthorizedAccessException("You can only submit feedback for your own bookings.");
        }

        var existingFeedback = await _feedbackRepository.FindAsync(f => f.BookingId == dto.BookingId);
        if (existingFeedback.Any())
            throw new ArgumentException("Feedback already exists for this booking.");

        if (dto.Rating < 1 || dto.Rating > 5) throw new ArgumentException("Rating must be between 1 and 5.");

        var feedback = _mapper.Map<Feedback>(dto);
        feedback.CreatedAt = DateTime.UtcNow;

        await _feedbackRepository.AddAsync(feedback);
        await _feedbackRepository.SaveChangesAsync();

        return _mapper.Map<FeedbackDTO>(feedback);
    }

    public async Task<FeedbackDTO> SubmitFeedbackCoreAsync(int bookingId, int rating, string comments)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId);
        if (booking == null) throw new KeyNotFoundException("Booking not found.");

        var existingFeedback = await _feedbackRepository.FindAsync(f => f.BookingId == bookingId);
        if (existingFeedback.Any())
            throw new ArgumentException("Feedback already exists for this booking.");

        if (rating < 1 || rating > 5) throw new ArgumentException("Rating must be between 1 and 5.");

        var feedback = new Feedback
        {
            BookingId = bookingId,
            Rating = rating,
            Comments = comments,
            CreatedAt = DateTime.UtcNow
        };

        await _feedbackRepository.AddAsync(feedback);
        await _feedbackRepository.SaveChangesAsync();

        return _mapper.Map<FeedbackDTO>(feedback);
    }

    public async Task<FeedbackDTO> ModerateFeedbackAsync(int feedbackId, bool isHidden)
    {
        var feedback = await _feedbackRepository.GetByIdAsync(feedbackId);
        if (feedback == null) throw new KeyNotFoundException("Feedback not found.");

        feedback.IsHidden = isHidden;
        _feedbackRepository.Update(feedback);
        await _feedbackRepository.SaveChangesAsync();

        return _mapper.Map<FeedbackDTO>(feedback);
    }
}
