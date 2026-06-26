using System;
using System.Threading.Tasks;
using HotelManagement.BLL.DTOs;
using HotelManagement.BLL.Services;
using HotelManagement.Repository.Interfaces;
using Moq;
using NUnit.Framework;

namespace HotelManagement.UnitTesting.Services;

[TestFixture]
public class AnalyticsServiceTests
{
    private Mock<IAnalyticsRepository> _mockAnalyticsRepo;
    private AnalyticsService _analyticsService;

    [SetUp]
    public void Setup()
    {
        _mockAnalyticsRepo = new Mock<IAnalyticsRepository>();
        _analyticsService = new AnalyticsService(_mockAnalyticsRepo.Object);
    }

    [Test]
    public async Task GetDashboardMetricsAsync_ShouldCalculateStandardMetrics()
    {
        // Arrange
        _mockAnalyticsRepo.Setup(r => r.GetTotalBookingsCountAsync(null, null)).ReturnsAsync(100);
        _mockAnalyticsRepo.Setup(r => r.GetCanceledBookingsCountAsync(null, null)).ReturnsAsync(10);
        _mockAnalyticsRepo.Setup(r => r.GetTotalRoomNightsSoldAsync(null, null)).ReturnsAsync(300);
        _mockAnalyticsRepo.Setup(r => r.GetTotalAvailableRoomNightsAsync(null, null)).ReturnsAsync(500);
        
        _mockAnalyticsRepo.Setup(r => r.GetRoomRevenueAsync(null, null)).ReturnsAsync(60000m);
        _mockAnalyticsRepo.Setup(r => r.GetFoodRevenueAsync(null, null)).ReturnsAsync(15000m);
        _mockAnalyticsRepo.Setup(r => r.GetAmenityRevenueAsync(null, null)).ReturnsAsync(5000m);
        _mockAnalyticsRepo.Setup(r => r.GetGrossTurnoverAsync(null, null)).ReturnsAsync(80000m);
        
        _mockAnalyticsRepo.Setup(r => r.GetAverageFeedbackScoreAsync(null, null)).ReturnsAsync(4.5);
        _mockAnalyticsRepo.Setup(r => r.GetCalculatedRevPARAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(120.00m);
        _mockAnalyticsRepo.Setup(r => r.GetCalculatedOccupancyRateAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(60.00);
        _mockAnalyticsRepo.Setup(r => r.GetGuestHappinessIndexAsync()).ReturnsAsync(4.5);
        _mockAnalyticsRepo.Setup(r => r.GetAverageHousekeepingTurnaroundTimeAsync()).ReturnsAsync(25.0);

        // Act
        var result = await _analyticsService.GetDashboardMetricsAsync(null, null);

        // Assert
        Assert.That(result.OccupancyRate, Is.EqualTo(60.00m)); // 300 / 500 * 100
        Assert.That(result.AverageDailyRate, Is.EqualTo(200.00m)); // 60000 / 300
        Assert.That(result.RevPAR, Is.EqualTo(120.00m)); // 60000 / 500
        Assert.That(result.TotalRevenue, Is.EqualTo(80000m)); // 60000 + 15000 + 5000
        Assert.That(result.GrossTurnover, Is.EqualTo(80000m));
        Assert.That(result.AverageLengthOfStay, Is.EqualTo(3.0)); // 300 / 100
        Assert.That(result.CancellationRate, Is.EqualTo(10.00m)); // 10 / 100 * 100
        Assert.That(result.GuestSatisfactionScore, Is.EqualTo(4.5));
        
        Assert.That(result.NonRoomExpenditure.TotalFoodSpend, Is.EqualTo(15000m));
        Assert.That(result.NonRoomExpenditure.TotalAmenitySpend, Is.EqualTo(5000m));
        Assert.That(result.NonRoomExpenditure.HighestSpendCategory, Is.EqualTo("Food"));
    }

    [Test]
    public async Task GetDashboardMetricsAsync_ShouldHandleDivideByZero()
    {
        // Arrange
        _mockAnalyticsRepo.Setup(r => r.GetTotalBookingsCountAsync(null, null)).ReturnsAsync(0);
        _mockAnalyticsRepo.Setup(r => r.GetCanceledBookingsCountAsync(null, null)).ReturnsAsync(0);
        _mockAnalyticsRepo.Setup(r => r.GetTotalRoomNightsSoldAsync(null, null)).ReturnsAsync(0);
        _mockAnalyticsRepo.Setup(r => r.GetTotalAvailableRoomNightsAsync(null, null)).ReturnsAsync(0);
        
        _mockAnalyticsRepo.Setup(r => r.GetRoomRevenueAsync(null, null)).ReturnsAsync(0m);
        _mockAnalyticsRepo.Setup(r => r.GetFoodRevenueAsync(null, null)).ReturnsAsync(0m);
        _mockAnalyticsRepo.Setup(r => r.GetAmenityRevenueAsync(null, null)).ReturnsAsync(0m);
        _mockAnalyticsRepo.Setup(r => r.GetGrossTurnoverAsync(null, null)).ReturnsAsync(0m);
        
        _mockAnalyticsRepo.Setup(r => r.GetAverageFeedbackScoreAsync(null, null)).ReturnsAsync(0);
        _mockAnalyticsRepo.Setup(r => r.GetCalculatedRevPARAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(0.00m);
        _mockAnalyticsRepo.Setup(r => r.GetCalculatedOccupancyRateAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(0.00);
        _mockAnalyticsRepo.Setup(r => r.GetGuestHappinessIndexAsync()).ReturnsAsync(0.0);
        _mockAnalyticsRepo.Setup(r => r.GetAverageHousekeepingTurnaroundTimeAsync()).ReturnsAsync(0.0);

        // Act
        var result = await _analyticsService.GetDashboardMetricsAsync(null, null);

        // Assert
        Assert.That(result.OccupancyRate, Is.EqualTo(0m)); 
        Assert.That(result.AverageDailyRate, Is.EqualTo(0m)); 
        Assert.That(result.RevPAR, Is.EqualTo(0m)); 
        Assert.That(result.AverageLengthOfStay, Is.EqualTo(0.0)); 
        Assert.That(result.CancellationRate, Is.EqualTo(0m)); 
        Assert.That(result.NonRoomExpenditure.HighestSpendCategory, Is.EqualTo("None"));
    }

    [Test]
    public async Task GetDashboardMetricsAsync_ShouldIdentifyAmenitiesAsHighestSpend()
    {
        // Arrange
        _mockAnalyticsRepo.Setup(r => r.GetFoodRevenueAsync(null, null)).ReturnsAsync(2000m);
        _mockAnalyticsRepo.Setup(r => r.GetAmenityRevenueAsync(null, null)).ReturnsAsync(5000m);

        // Act
        var result = await _analyticsService.GetDashboardMetricsAsync(null, null);

        // Assert
        Assert.That(result.NonRoomExpenditure.HighestSpendCategory, Is.EqualTo("Amenities"));
    }

    [Test]
    public async Task GetDashboardMetricsAsync_ShouldIdentifyEqualSpend()
    {
        // Arrange
        _mockAnalyticsRepo.Setup(r => r.GetFoodRevenueAsync(null, null)).ReturnsAsync(5000m);
        _mockAnalyticsRepo.Setup(r => r.GetAmenityRevenueAsync(null, null)).ReturnsAsync(5000m);

        // Act
        var result = await _analyticsService.GetDashboardMetricsAsync(null, null);

        // Assert
        Assert.That(result.NonRoomExpenditure.HighestSpendCategory, Is.EqualTo("Equal"));
    }

    [Test]
    public async Task GetDashboardMetricsAsync_ShouldUseProvidedDates()
    {
        var start = new DateTime(2023, 1, 1);
        var end = new DateTime(2023, 1, 31);

        _mockAnalyticsRepo.Setup(r => r.GetTotalBookingsCountAsync(start, end)).ReturnsAsync(10);
        _mockAnalyticsRepo.Setup(r => r.GetCalculatedRevPARAsync(start, end)).ReturnsAsync(100m);
        _mockAnalyticsRepo.Setup(r => r.GetCalculatedOccupancyRateAsync(start, end)).ReturnsAsync(50.0);

        var result = await _analyticsService.GetDashboardMetricsAsync(start, end);

        Assert.That(result.RevPAR, Is.EqualTo(100m));
        _mockAnalyticsRepo.Verify(r => r.GetCalculatedRevPARAsync(start, end), Times.Once);
    }
}
