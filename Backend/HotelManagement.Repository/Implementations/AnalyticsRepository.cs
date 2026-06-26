using HotelManagement.DAL.Context;
using HotelManagement.DAL.Enums;
using HotelManagement.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Repository.Implementations;

public class AnalyticsRepository : IAnalyticsRepository
{
    private readonly ApplicationDbContext _context;

    public AnalyticsRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> GetTotalBookingsCountAsync(DateTime? startDate, DateTime? endDate)
    {
        var query = _context.Bookings.AsQueryable();
        if (startDate.HasValue) query = query.Where(b => b.BookedAt >= startDate.Value);
        if (endDate.HasValue) query = query.Where(b => b.BookedAt <= endDate.Value);
        return await query.CountAsync();
    }

    public async Task<int> GetCanceledBookingsCountAsync(DateTime? startDate, DateTime? endDate)
    {
        var query = _context.Bookings.Where(b => b.BookingStatus == BookingStatus.Cancelled);
        if (startDate.HasValue) query = query.Where(b => b.BookedAt >= startDate.Value);
        if (endDate.HasValue) query = query.Where(b => b.BookedAt <= endDate.Value);
        return await query.CountAsync();
    }

    public async Task<int> GetTotalRoomNightsSoldAsync(DateTime? startDate, DateTime? endDate)
    {
        var query = _context.Bookings.Where(b => b.BookingStatus != BookingStatus.Cancelled);
        if (startDate.HasValue) query = query.Where(b => b.CheckInDate >= startDate.Value);
        if (endDate.HasValue) query = query.Where(b => b.CheckOutDate <= endDate.Value);

        // Fetch minimal fields to calculate accurately in-memory and bypass EF Core PostgreSQL date translation quirks
        var bookings = await query.Select(b => new { b.CheckInDate, b.CheckOutDate }).ToListAsync();
        var totalNights = bookings.Sum(b => (b.CheckOutDate.Date - b.CheckInDate.Date).Days);
        
        return totalNights;
    }

    public async Task<int> GetTotalAvailableRoomNightsAsync(DateTime? startDate, DateTime? endDate)
    {
        var totalRooms = await _context.Rooms.CountAsync(r => r.IsActive);
        
        DateTime start = startDate ?? await _context.Bookings.MinAsync(b => (DateTime?)b.BookedAt) ?? DateTime.UtcNow;
        DateTime end = endDate ?? DateTime.UtcNow;

        if (end < start) return 0;
        
        int days = (end.Date - start.Date).Days;
        if (days == 0) days = 1; // at least 1 day

        return totalRooms * days;
    }

    public async Task<decimal> GetRoomRevenueAsync(DateTime? startDate, DateTime? endDate)
    {
        var query = _context.Bookings.Where(b => b.BookingStatus != BookingStatus.Cancelled);
        if (startDate.HasValue) query = query.Where(b => b.CheckInDate >= startDate.Value);
        if (endDate.HasValue) query = query.Where(b => b.CheckOutDate <= endDate.Value);

        // Fetch minimal fields to calculate accurately in-memory
        var bookings = await query.Select(b => new { b.CheckInDate, b.CheckOutDate, LockedInPrice = b.BookingRooms.Sum(br => br.LockedInPrice) }).ToListAsync();
        var roomRevenue = bookings.Sum(b => (b.CheckOutDate.Date - b.CheckInDate.Date).Days * b.LockedInPrice);
        
        return roomRevenue;
    }

    public async Task<decimal> GetFoodRevenueAsync(DateTime? startDate, DateTime? endDate)
    {
        var query = _context.FoodOrderItems.AsQueryable();
        if (startDate.HasValue) query = query.Where(f => f.FoodOrder.GeneratedAt >= startDate.Value);
        if (endDate.HasValue) query = query.Where(f => f.FoodOrder.GeneratedAt <= endDate.Value);

        return await query.SumAsync(f => f.Quantity * f.PriceAtPurchase);
    }

    public async Task<decimal> GetAmenityRevenueAsync(DateTime? startDate, DateTime? endDate)
    {
        var query = _context.BookingAmenities.AsQueryable();
        if (startDate.HasValue) query = query.Where(ba => ba.SubscribedAt >= startDate.Value);
        if (endDate.HasValue) query = query.Where(ba => ba.SubscribedAt <= endDate.Value);

        return await query.SumAsync(ba => ba.PriceAtPurchase);
    }

    public async Task<decimal> GetGrossTurnoverAsync(DateTime? startDate, DateTime? endDate)
    {
        var query = _context.Receipts.AsQueryable();
        if (startDate.HasValue) query = query.Where(r => r.PaidAt >= startDate.Value);
        if (endDate.HasValue) query = query.Where(r => r.PaidAt <= endDate.Value);

        return await query.SumAsync(r => r.AmountPaid);
    }

    public async Task<double> GetAverageFeedbackScoreAsync(DateTime? startDate, DateTime? endDate)
    {
        var query = _context.Feedbacks.Where(f => !f.IsHidden);
        if (startDate.HasValue) query = query.Where(f => f.CreatedAt >= startDate.Value);
        if (endDate.HasValue) query = query.Where(f => f.CreatedAt <= endDate.Value);

        var hasFeedbacks = await query.AnyAsync();
        if (!hasFeedbacks) return 0;

        return await query.AverageAsync(f => f.Rating);
    }

    public async Task<decimal> GetCalculatedRevPARAsync(DateTime startDate, DateTime endDate)
    {
        // Using ToListAsync().FirstOrDefault() is safer for scalar Raw SQL queries in EF Core Npgsql
        var result = await _context.Database.SqlQueryRaw<decimal>(
            "SELECT calculaterevpar({0}, {1}) AS \"Value\"", startDate, endDate)
            .ToListAsync();
            
        return result.FirstOrDefault();
    }

    public async Task<double> GetCalculatedOccupancyRateAsync(DateTime startDate, DateTime endDate)
    {
        var result = await _context.Database.SqlQueryRaw<double>(
            "SELECT calculateoccupancyrate({0}, {1}) AS \"Value\"", startDate, endDate)
            .ToListAsync();
            
        return result.FirstOrDefault();
    }

    public async Task<double> GetGuestHappinessIndexAsync()
    {
        var result = await _context.Database.SqlQueryRaw<double>(
            "SELECT calculateguesthappinessindex() AS \"Value\"")
            .ToListAsync();
            
        return result.FirstOrDefault();
    }

    public async Task<double> GetAverageHousekeepingTurnaroundTimeAsync()
    {
        var result = await _context.Database.SqlQueryRaw<double>(
            "SELECT calculateaveragehousekeepingturnaroundtime() AS \"Value\"")
            .ToListAsync();
            
        return result.FirstOrDefault();
    }
}
