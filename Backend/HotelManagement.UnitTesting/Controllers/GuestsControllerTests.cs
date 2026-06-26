using System.Threading.Tasks;
using HotelManagement.API.Controllers;
using HotelManagement.BLL.DTOs;
using HotelManagement.BLL.Interfaces;
using HotelManagement.Repository.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace HotelManagement.UnitTesting.Controllers;

[TestFixture]
public class GuestsControllerTests
{
    private Mock<IGuestService> _mockGuestService;
    private GuestsController _controller;

    [SetUp]
    public void Setup()
    {
        _mockGuestService = new Mock<IGuestService>();
        _controller = new GuestsController(_mockGuestService.Object);
    }

    [Test]
    public async Task GetGuests_ShouldReturnIncoming_WhenStatusIsIncoming()
    {
        var resultDto = new PaginatedResult<BookingDTO>();
        _mockGuestService.Setup(s => s.GetIncomingGuestsAsync(1, 10)).ReturnsAsync(resultDto);

        var result = await _controller.GetGuests(null, "incoming", 1, 10) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
        _mockGuestService.Verify(s => s.GetIncomingGuestsAsync(1, 10), Times.Once);
    }

    [Test]
    public async Task GetGuests_ShouldReturnSearch_WhenStatusIsNotIncoming()
    {
        var resultDto = new PaginatedResult<GuestSearchDTO>();
        _mockGuestService.Setup(s => s.SearchGuestsAsync("John", 1, 10)).ReturnsAsync(resultDto);

        var result = await _controller.GetGuests("John", "other", 1, 10) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
        _mockGuestService.Verify(s => s.SearchGuestsAsync("John", 1, 10), Times.Once);
    }
}
