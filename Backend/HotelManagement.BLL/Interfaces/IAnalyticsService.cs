using HotelManagement.BLL.DTOs;

namespace HotelManagement.BLL.Interfaces;

public interface IAnalyticsService
{
    Task<AnalyticsDashboardDTO> GetDashboardMetricsAsync(DateTime? startDate, DateTime? endDate);
}
