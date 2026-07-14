using HotelManagement.BLL.DTOs;
using HotelManagement.BLL.Interfaces;
using HotelManagement.DAL.Context;
using HotelManagement.DAL.Entities;
using HotelManagement.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HotelManagement.BLL.Services;

public class FeedbackReminderService : IFeedbackReminderService
{
    private const int InitialDelayDays = 2;
    private const int ReminderIntervalDays = 30;
    private const int MaxReminders = 3;
    private const int TokenExpiryDays = 30;

    private readonly ApplicationDbContext _db;
    private readonly IEmailService _emailService;
    private readonly IFeedbackService _feedbackService;
    private readonly ILogger<FeedbackReminderService> _logger;

    public FeedbackReminderService(ApplicationDbContext db, IEmailService emailService, IFeedbackService feedbackService, ILogger<FeedbackReminderService> logger)
    {
        _db = db;
        _emailService = emailService;
        _feedbackService = feedbackService;
        _logger = logger;
    }

    public async Task ProcessDueRemindersAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var checkoutCutoff = now.AddDays(-InitialDelayDays);

        var candidates = await _db.Bookings
            .Where(b => b.BookingStatus == BookingStatus.CheckedOut && b.Feedback == null && b.CheckOutDate <= checkoutCutoff)
            .Select(b => new
            {
                Booking = b,
                Reminder = _db.FeedbackReminders.FirstOrDefault(r => r.BookingId == b.Id)
            })
            .ToListAsync(ct);

        var reminderCutoff = now.AddDays(-ReminderIntervalDays);

        foreach (var candidate in candidates)
        {
            var isDue = candidate.Reminder == null
                || (candidate.Reminder.SentCount < MaxReminders
                    && candidate.Reminder.LastSentAt != null
                    && candidate.Reminder.LastSentAt.Value <= reminderCutoff);

            if (!isDue) continue;

            var reminder = candidate.Reminder;
            if (reminder == null)
            {
                reminder = new FeedbackReminder
                {
                    BookingId = candidate.Booking.Id,
                    Token = Guid.NewGuid(),
                    TokenExpiresAt = now.AddDays(TokenExpiryDays)
                };
                _db.FeedbackReminders.Add(reminder);
            }
            else if (reminder.TokenExpiresAt <= now)
            {
                reminder.Token = Guid.NewGuid();
                reminder.TokenExpiresAt = now.AddDays(TokenExpiryDays);
            }

            reminder.LastSentAt = now;
            reminder.SentCount += 1;

            try
            {
                await _db.SaveChangesAsync(ct);
                _ = _emailService.SendFeedbackReminderAsync(candidate.Booking.Id, candidate.Booking.GuestName, candidate.Booking.GuestEmail, reminder.Token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process feedback reminder for booking {BookingId}.", candidate.Booking.Id);
            }
        }
    }

    public async Task<FeedbackReminderValidationDTO> ValidateTokenAsync(Guid token)
    {
        var reminder = await _db.FeedbackReminders
            .Include(r => r.Booking)
            .FirstOrDefaultAsync(r => r.Token == token);

        if (reminder == null)
        {
            return new FeedbackReminderValidationDTO { IsValid = false, ErrorReason = "not_found" };
        }

        if (reminder.TokenExpiresAt <= DateTime.UtcNow)
        {
            return new FeedbackReminderValidationDTO { IsValid = false, BookingId = reminder.BookingId, ErrorReason = "expired" };
        }

        var alreadySubmitted = await _db.Feedbacks.AnyAsync(f => f.BookingId == reminder.BookingId);

        return new FeedbackReminderValidationDTO
        {
            IsValid = true,
            BookingId = reminder.BookingId,
            GuestName = reminder.Booking.GuestName,
            AlreadySubmitted = alreadySubmitted
        };
    }

    public async Task<FeedbackDTO> SubmitFeedbackWithTokenAsync(Guid token, int rating, string comments)
    {
        var reminder = await _db.FeedbackReminders.FirstOrDefaultAsync(r => r.Token == token);
        if (reminder == null) throw new KeyNotFoundException("Invalid feedback link.");

        if (reminder.TokenExpiresAt <= DateTime.UtcNow)
            throw new ArgumentException("This feedback link has expired.");

        return await _feedbackService.SubmitFeedbackCoreAsync(reminder.BookingId, rating, comments);
    }
}
