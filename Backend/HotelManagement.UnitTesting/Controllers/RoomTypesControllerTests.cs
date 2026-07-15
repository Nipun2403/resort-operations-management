using System;
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
public class RoomTypesControllerTests
{
    private Mock<IRoomTypeService> _mockRoomTypeService;
    private RoomTypesController _controller;

    [SetUp]
    public void Setup()
    {
        _mockRoomTypeService = new Mock<IRoomTypeService>();
        _controller = new RoomTypesController(_mockRoomTypeService.Object);
    }

    [Test]
    public async Task GetRoomTypes_ShouldReturnOk()
    {
        var resultDto = new PaginatedResult<RoomTypeDTO>();
        _mockRoomTypeService.Setup(s => s.GetRoomTypesAsync(1, 10, false, null, null, false)).ReturnsAsync(resultDto);
        var result = await _controller.GetRoomTypes(false, 1, 10) as OkObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task GetAvailableRoomTypes_ShouldReturnBadRequest_IfCheckInInvalid()
    {
        var result = await _controller.GetAvailableRoomTypes("invalid", "02-01-2023") as BadRequestObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
        Assert.That(result.Value, Does.Contain("Invalid checkIn format"));
    }

    [Test]
    public async Task GetAvailableRoomTypes_ShouldReturnBadRequest_IfCheckOutInvalid()
    {
        var result = await _controller.GetAvailableRoomTypes("01-01-2023", "invalid") as BadRequestObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
        Assert.That(result.Value, Does.Contain("Invalid checkOut format"));
    }

    [Test]
    public async Task GetAvailableRoomTypes_ShouldReturnBadRequest_IfCheckInAfterCheckOut()
    {
        var result = await _controller.GetAvailableRoomTypes("02-01-2023", "01-01-2023") as BadRequestObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
        Assert.That(result.Value, Does.Contain("strictly after CheckIn"));
    }

    [Test]
    public async Task GetAvailableRoomTypes_ShouldReturnOk_WhenSuccessful()
    {
        var resultDto = new PaginatedResult<RoomTypeAvailabilityDTO>();
        _mockRoomTypeService.Setup(s => s.GetAvailableRoomTypesAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), 1, 10, "BasePrice", false)).ReturnsAsync(resultDto);
        var result = await _controller.GetAvailableRoomTypes("01-01-2023", "02-01-2023") as OkObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task CreateRoomType_ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        _controller.ModelState.AddModelError("Name", "Required");

        // FIXED: Swapped RoomTypeDTO to CreateRoomTypeDTO
        var result = await _controller.CreateRoomType(new CreateRoomTypeDTO()) as BadRequestObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task CreateRoomType_ShouldReturnOk_WhenSuccessful()
    {
        var dto = new CreateRoomTypeDTO();
        var resultDto = new RoomTypeDTO { Id = 1 };

        _mockRoomTypeService.Setup(s => s.CreateRoomTypeAsync(It.IsAny<CreateRoomTypeDTO>())).ReturnsAsync(resultDto);

        // Cast to generic ObjectResult to catch OkObjectResult OR CreatedAtActionResult
        var result = await _controller.CreateRoomType(dto) as ObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.AnyOf(200, 201));
    }

    [Test]
    public async Task UpdateRoomType_ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        _controller.ModelState.AddModelError("Name", "Required");
        var result = await _controller.UpdateRoomType(1, new UpdateRoomTypeDTO()) as BadRequestObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task UpdateRoomType_ShouldReturnOk_WhenSuccessful()
    {
        var dto = new UpdateRoomTypeDTO();
        var resultDto = new RoomTypeDTO();
        _mockRoomTypeService.Setup(s => s.UpdateRoomTypeAsync(1, dto)).ReturnsAsync(resultDto);
        var result = await _controller.UpdateRoomType(1, dto) as OkObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task UpdateRoomType_ShouldReturnNotFound_OnArgumentException()
    {
        var dto = new UpdateRoomTypeDTO();
        _mockRoomTypeService.Setup(s => s.UpdateRoomTypeAsync(1, dto)).ThrowsAsync(new ArgumentException());
        var result = await _controller.UpdateRoomType(1, dto) as NotFoundObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(404));
    }

    [Test]
    public async Task DeleteRoomType_ShouldReturnOk_WhenSuccessful()
    {
        _mockRoomTypeService.Setup(s => s.DeleteRoomTypeAsync(1)).Returns(Task.CompletedTask);
        var result = await _controller.DeleteRoomType(1) as OkObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task DeleteRoomType_ShouldReturnNotFound_OnArgumentException()
    {
        _mockRoomTypeService.Setup(s => s.DeleteRoomTypeAsync(1)).ThrowsAsync(new ArgumentException());
        var result = await _controller.DeleteRoomType(1) as NotFoundObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(404));
    }

    [Test]
    public async Task DeleteRoomType_ShouldReturnBadRequest_OnInvalidOperationException()
    {
        _mockRoomTypeService.Setup(s => s.DeleteRoomTypeAsync(1)).ThrowsAsync(new InvalidOperationException());
        var result = await _controller.DeleteRoomType(1) as BadRequestObjectResult;
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
    }
}