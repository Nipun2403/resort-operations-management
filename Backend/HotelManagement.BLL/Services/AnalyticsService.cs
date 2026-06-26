using HotelManagement.BLL.DTOs;
using HotelManagement.BLL.Interfaces;
using HotelManagement.Repository.Interfaces;

namespace HotelManagement.BLL.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly IAnalyticsRepository _analyticsRepository;

    public AnalyticsService(IAnalyticsRepository analyticsRepository)
    {
        _analyticsRepository = analyticsRepository;
    }

    public async Task<AnalyticsDashboardDTO> GetDashboardMetricsAsync(DateTime? startDate, DateTime? endDate)
    {
        // 1. Fetch raw aggregates from DB (Executed sequentially to avoid DbContext concurrent issues)
        var bookingsCount = await _analyticsRepository.GetTotalBookingsCountAsync(startDate, endDate);
        var canceledBookingsCount = await _analyticsRepository.GetCanceledBookingsCountAsync(startDate, endDate);
        var roomNightsSold = await _analyticsRepository.GetTotalRoomNightsSoldAsync(startDate, endDate);
        var availableRoomNights = await _analyticsRepository.GetTotalAvailableRoomNightsAsync(startDate, endDate);
        
        var foodRevenue = await _analyticsRepository.GetFoodRevenueAsync(startDate, endDate);
        var amenityRevenue = await _analyticsRepository.GetAmenityRevenueAsync(startDate, endDate);
        var grossTurnover = await _analyticsRepository.GetGrossTurnoverAsync(startDate, endDate);
        
        // 1b. Call Stored Procedures for complex metrics
        DateTime spStart = startDate ?? DateTime.UtcNow.AddDays(-30);
        DateTime spEnd = endDate ?? DateTime.UtcNow;

        var revPar = await _analyticsRepository.GetCalculatedRevPARAsync(spStart, spEnd);
        var occupancyRate = await _analyticsRepository.GetCalculatedOccupancyRateAsync(spStart, spEnd);
        var avgFeedback = await _analyticsRepository.GetGuestHappinessIndexAsync();
        var avgHousekeeping = await _analyticsRepository.GetAverageHousekeepingTurnaroundTimeAsync();

        // 2. Perform business logic calculations
        decimal adr = roomNightsSold > 0 
            ? Math.Round(await _analyticsRepository.GetRoomRevenueAsync(startDate, endDate) / roomNightsSold, 2) 
            : 0;

        double alos = bookingsCount > 0 
            ? Math.Round((double)roomNightsSold / bookingsCount, 2) 
            : 0;

        decimal cancellationRate = bookingsCount > 0 
            ? Math.Round(((decimal)canceledBookingsCount / bookingsCount) * 100, 2) 
            : 0;

        decimal totalRevenue = (await _analyticsRepository.GetRoomRevenueAsync(startDate, endDate)) + foodRevenue + amenityRevenue;

        string highestSpendCategory = "Equal";
        if (foodRevenue > amenityRevenue) highestSpendCategory = "Food";
        else if (amenityRevenue > foodRevenue) highestSpendCategory = "Amenities";
        else if (foodRevenue == 0 && amenityRevenue == 0) highestSpendCategory = "None";

        // 3. Construct and return DTO
        return new AnalyticsDashboardDTO
        {
            OccupancyRate = Math.Round((decimal)occupancyRate, 2),
            AverageDailyRate = adr,
            RevPAR = Math.Round(revPar, 2),
            TotalRevenue = totalRevenue,
            GrossTurnover = grossTurnover,
            AverageLengthOfStay = alos,
            CancellationRate = cancellationRate,
            GuestSatisfactionScore = Math.Round(avgFeedback, 2),
            AverageHousekeepingTurnaroundMinutes = Math.Round(avgHousekeeping, 2),
            NonRoomExpenditure = new ExpenditureBreakdownDTO
            {
                TotalFoodSpend = foodRevenue,
                TotalAmenitySpend = amenityRevenue,
                HighestSpendCategory = highestSpendCategory
            }
        };
    }
}
