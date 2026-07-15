using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelManagement.API.Controllers;
using HotelManagement.BLL.DTOs;
using HotelManagement.BLL.Interfaces;
using HotelManagement.DAL.Enums;
using HotelManagement.Repository.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace HotelManagement.UnitTesting.Controllers;

[TestFixture]
public class BookingsControllerTests
{
    private Mock<IBookingService> _mockBookingService;
    private Mock<IBillingService> _mockBillingService;
    private Mock<IAmenityService> _mockAmenityService;
    private BookingsController _controller;

    [SetUp]
    public void Setup()
    {
        _mockBookingService = new Mock<IBookingService>();
        _mockBillingService = new Mock<IBillingService>();
        _mockAmenityService = new Mock<IAmenityService>();
        _controller = new BookingsController(_mockBookingService.Object, _mockBillingService.Object, _mockAmenityService.Object);
    }

    [Test]
    public async Task GetBookings_ShouldReturnOk_WhenSuccessful()
    {
        var resultDto = new PaginatedResult<BookingDTO>();
        _mockBookingService.Setup(s => s.GetBookingsAsync(null, null, 1, 10, null, false, null, null)).ReturnsAsync(resultDto);

        var result = await _controller.GetBookings(null, null, 1, 10) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task GetBookings_ShouldReturnUnauthorized_WhenUnauthorizedAccessExceptionThrown()
    {
        _mockBookingService.Setup(s => s.GetBookingsAsync(null, null, 1, 10, null, false, null, null)).ThrowsAsync(new UnauthorizedAccessException("No access"));

        var result = await _controller.GetBookings(null, null, 1, 10) as UnauthorizedObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(401));
    }

    [Test]
    public async Task GetBooking_ShouldReturnNotFound_WhenNull()
    {
        _mockBookingService.Setup(s => s.GetBookingByIdAsync(1)).ReturnsAsync((BookingDTO?)null);

        var result = await _controller.GetBooking(1) as NotFoundObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(404));
    }

    [Test]
    public async Task GetBooking_ShouldReturnOk_WhenFound()
    {
        var dto = new BookingDTO { Id = 1 };
        _mockBookingService.Setup(s => s.GetBookingByIdAsync(1)).ReturnsAsync(dto);

        var result = await _controller.GetBooking(1) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task BookRoom_ShouldReturnCreatedAtAction_WhenSuccessful()
    {
        var request = new CreateBookingRequestDTO();
        var dto = new BookingDTO { Id = 1 };
        _mockBookingService.Setup(s => s.CreateBookingAsync(request)).ReturnsAsync(dto);

        var result = await _controller.BookRoom(request) as CreatedAtActionResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(201));
    }

    [Test]
    public async Task BookRoom_ShouldReturnBadRequest_OnGeneralException()
    {
        var request = new CreateBookingRequestDTO();
        _mockBookingService.Setup(s => s.CreateBookingAsync(request)).ThrowsAsync(new Exception("Error"));

        var result = await _controller.BookRoom(request) as BadRequestObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public void BookRoom_ShouldRethrow_KeyNotFoundException()
    {
        var request = new CreateBookingRequestDTO();
        _mockBookingService.Setup(s => s.CreateBookingAsync(request)).ThrowsAsync(new KeyNotFoundException());

        Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.BookRoom(request));
    }

    [Test]
    public async Task UpdateBooking_ShouldReturnOk_WhenSuccessful()
    {
        var dto = new UpdateBookingDTO { CheckOutDate = DateTime.UtcNow.AddDays(1), BookingStatus = BookingStatus.Cancelled };
        _mockBookingService.Setup(s => s.ExtendStayAsync(1, dto.CheckOutDate.Value)).Returns(Task.CompletedTask);
        _mockBookingService.Setup(s => s.UpdateBookingStatusAsync(1, dto.BookingStatus.Value)).Returns(Task.CompletedTask);

        var result = await _controller.UpdateBooking(1, dto) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task UpdateBooking_ShouldReturnBadRequest_IfStatusIsCheckIn()
    {
        var dto = new UpdateBookingDTO { BookingStatus = BookingStatus.CheckedIn };
        var result = await _controller.UpdateBooking(1, dto) as BadRequestObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task UpdateBooking_ShouldReturnConflict_OnArgumentException()
    {
        var dto = new UpdateBookingDTO { CheckOutDate = DateTime.UtcNow };
        _mockBookingService.Setup(s => s.ExtendStayAsync(1, It.IsAny<DateTime>())).ThrowsAsync(new ArgumentException("Conflict"));

        var result = await _controller.UpdateBooking(1, dto) as ConflictObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(409));
    }

    [Test]
    public async Task UpdateBooking_ShouldReturnBadRequest_OnGeneralException()
    {
        var dto = new UpdateBookingDTO { CheckOutDate = DateTime.UtcNow };
        _mockBookingService.Setup(s => s.ExtendStayAsync(1, It.IsAny<DateTime>())).ThrowsAsync(new Exception("Error"));

        var result = await _controller.UpdateBooking(1, dto) as BadRequestObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public void UpdateBooking_ShouldRethrow_KeyNotFoundException()
    {
        var dto = new UpdateBookingDTO { CheckOutDate = DateTime.UtcNow };
        _mockBookingService.Setup(s => s.ExtendStayAsync(1, It.IsAny<DateTime>())).ThrowsAsync(new KeyNotFoundException());

        Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.UpdateBooking(1, dto));
    }

    [Test]
    public async Task CheckIn_ShouldReturnOk_WhenSuccessful()
    {
        var dto = new BookingDTO();
        _mockBookingService.Setup(s => s.CheckInGuestAsync(1)).ReturnsAsync(dto);

        var result = await _controller.CheckIn(1) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task CheckIn_ShouldReturnConflict_OnInvalidOperationException()
    {
        _mockBookingService.Setup(s => s.CheckInGuestAsync(1)).ThrowsAsync(new InvalidOperationException());

        var result = await _controller.CheckIn(1) as ConflictObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(409));
    }

    [Test]
    public async Task CheckIn_ShouldReturnBadRequest_OnArgumentException()
    {
        _mockBookingService.Setup(s => s.CheckInGuestAsync(1)).ThrowsAsync(new ArgumentException());

        var result = await _controller.CheckIn(1) as BadRequestObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task CheckOut_ShouldReturnOk_WhenSuccessful()
    {
        var folio = new BillingFolioDTO();
        _mockBookingService.Setup(s => s.UnifiedCheckoutAsync(1)).ReturnsAsync(folio);

        var result = await _controller.CheckOut(1) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task CheckOut_ShouldReturnConflict_OnInvalidOperationException()
    {
        _mockBookingService.Setup(s => s.UnifiedCheckoutAsync(1)).ThrowsAsync(new InvalidOperationException());

        var result = await _controller.CheckOut(1) as ConflictObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(409));
    }

    [Test]
    public async Task CheckOut_ShouldReturnBadRequest_OnArgumentException()
    {
        _mockBookingService.Setup(s => s.UnifiedCheckoutAsync(1)).ThrowsAsync(new ArgumentException());

        var result = await _controller.CheckOut(1) as BadRequestObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task CancelBooking_ShouldReturnOk()
    {
        _mockBookingService.Setup(s => s.CancelBookingAsync(1)).Returns(Task.CompletedTask);

        var result = await _controller.CancelBooking(1) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task CancelBooking_ShouldReturnForbid_OnUnauthorizedAccessException()
    {
        _mockBookingService.Setup(s => s.CancelBookingAsync(1)).ThrowsAsync(new UnauthorizedAccessException());

        var result = await _controller.CancelBooking(1) as ForbidResult;

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task CancelBooking_ShouldReturnBadRequest_OnGeneralException()
    {
        _mockBookingService.Setup(s => s.CancelBookingAsync(1)).ThrowsAsync(new Exception());

        var result = await _controller.CancelBooking(1) as BadRequestObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public void CancelBooking_ShouldRethrow_KeyNotFoundException()
    {
        _mockBookingService.Setup(s => s.CancelBookingAsync(1)).ThrowsAsync(new KeyNotFoundException());

        Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.CancelBooking(1));
    }

    [Test]
    public async Task SubscribeAmenity_ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        _controller.ModelState.AddModelError("AmenityId", "Required");
        var result = await _controller.SubscribeAmenity(1, new SubscribeAmenityDTO()) as BadRequestObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task SubscribeAmenity_ShouldReturnOk()
    {
        var dto = new SubscribeAmenityDTO { AmenityId = 2 };
        _mockAmenityService.Setup(s => s.SubscribeAsync(1, 2)).Returns(Task.CompletedTask);

        var result = await _controller.SubscribeAmenity(1, dto) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task SubscribeAmenity_ShouldReturnBadRequest_OnInvalidOperation()
    {
        var dto = new SubscribeAmenityDTO { AmenityId = 2 };
        _mockAmenityService.Setup(s => s.SubscribeAsync(1, 2)).ThrowsAsync(new InvalidOperationException());

        var result = await _controller.SubscribeAmenity(1, dto) as BadRequestObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task SubscribeAmenity_ShouldReturnNotFound_OnArgumentException()
    {
        var dto = new SubscribeAmenityDTO { AmenityId = 2 };
        _mockAmenityService.Setup(s => s.SubscribeAsync(1, 2)).ThrowsAsync(new ArgumentException());

        var result = await _controller.SubscribeAmenity(1, dto) as NotFoundObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(404));
    }

    [Test]
    public async Task SubscribeAmenity_ShouldReturnBadRequest_OnGeneralException()
    {
        var dto = new SubscribeAmenityDTO { AmenityId = 2 };
        _mockAmenityService.Setup(s => s.SubscribeAsync(1, 2)).ThrowsAsync(new Exception());

        var result = await _controller.SubscribeAmenity(1, dto) as BadRequestObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public void SubscribeAmenity_ShouldRethrow_KeyNotFoundException()
    {
        var dto = new SubscribeAmenityDTO { AmenityId = 2 };
        _mockAmenityService.Setup(s => s.SubscribeAsync(1, 2)).ThrowsAsync(new KeyNotFoundException());

        Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.SubscribeAmenity(1, dto));
    }
}
