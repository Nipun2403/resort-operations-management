namespace HotelManagement.Repository.Interfaces;

public interface IAnalyticsRepository
{
    Task<int> GetTotalBookingsCountAsync(DateTime? startDate, DateTime? endDate);
    Task<int> GetCanceledBookingsCountAsync(DateTime? startDate, DateTime? endDate);
    Task<int> GetTotalRoomNightsSoldAsync(DateTime? startDate, DateTime? endDate);
    Task<int> GetTotalAvailableRoomNightsAsync(DateTime? startDate, DateTime? endDate);
    Task<decimal> GetRoomRevenueAsync(DateTime? startDate, DateTime? endDate);
    Task<decimal> GetFoodRevenueAsync(DateTime? startDate, DateTime? endDate);
    Task<decimal> GetAmenityRevenueAsync(DateTime? startDate, DateTime? endDate);
    Task<decimal> GetGrossTurnoverAsync(DateTime? startDate, DateTime? endDate);
    Task<double> GetAverageFeedbackScoreAsync(DateTime? startDate, DateTime? endDate);

    // Stored Procedure Executions
    Task<decimal> GetCalculatedRevPARAsync(DateTime startDate, DateTime endDate);
    Task<double> GetCalculatedOccupancyRateAsync(DateTime startDate, DateTime endDate);
    Task<double> GetGuestHappinessIndexAsync();
    Task<double> GetAverageHousekeepingTurnaroundTimeAsync();
}
