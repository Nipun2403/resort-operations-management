using System;
using System.Threading.Tasks;
using HotelManagement.API.Controllers;
using HotelManagement.BLL.DTOs;
using HotelManagement.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace HotelManagement.UnitTesting.Controllers;

[TestFixture]
public class AnalyticsControllerTests
{
    private Mock<IAnalyticsService> _mockAnalyticsService;
    private AnalyticsController _controller;

    [SetUp]
    public void Setup()
    {
        _mockAnalyticsService = new Mock<IAnalyticsService>();
        _controller = new AnalyticsController(_mockAnalyticsService.Object);
    }

    [Test]
    public async Task GetDashboardMetrics_ShouldReturnBadRequest_WhenStartDateIsAfterEndDate()
    {
        var startDate = DateTime.UtcNow.AddDays(1);
        var endDate = DateTime.UtcNow;

        var actionResult = await _controller.GetDashboardMetrics(startDate, endDate);
        var result = actionResult.Result as BadRequestObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task GetDashboardMetrics_ShouldReturnOk_WhenDatesAreValid()
    {
        var startDate = DateTime.UtcNow;
        var endDate = DateTime.UtcNow.AddDays(1);
        var metrics = new AnalyticsDashboardDTO();

        _mockAnalyticsService.Setup(s => s.GetDashboardMetricsAsync(startDate, endDate)).ReturnsAsync(metrics);

        var actionResult = await _controller.GetDashboardMetrics(startDate, endDate);
        var result = actionResult.Result as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
        Assert.That(result.Value, Is.EqualTo(metrics));
    }

    [Test]
    public async Task GetDashboardMetrics_ShouldReturnOk_WhenDatesAreNull()
    {
        var metrics = new AnalyticsDashboardDTO();
        _mockAnalyticsService.Setup(s => s.GetDashboardMetricsAsync(null, null)).ReturnsAsync(metrics);

        var actionResult = await _controller.GetDashboardMetrics(null, null);
        var result = actionResult.Result as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
        Assert.That(result.Value, Is.EqualTo(metrics));
    }
}
