using System;
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
public class HousekeepingControllerTests
{
    private Mock<IHousekeepingService> _mockHousekeepingService;
    private Mock<IBookingService> _mockBookingService;
    private HousekeepingController _controller;

    [SetUp]
    public void Setup()
    {
        _mockHousekeepingService = new Mock<IHousekeepingService>();
        _mockBookingService = new Mock<IBookingService>();
        _controller = new HousekeepingController(_mockHousekeepingService.Object, _mockBookingService.Object);
    }

    [Test]
    public async Task GetAllTasks_ShouldReturnOk()
    {
        var resultDto = new PaginatedResult<HousekeepingDTO>();
        _mockHousekeepingService.Setup(s => s.GetAllAsync(1, 10, null, null, false)).ReturnsAsync(resultDto);

        var result = await _controller.GetAllTasks(1, 10) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task GetPendingTasks_ShouldReturnOk()
    {
        var resultDto = new PaginatedResult<HousekeepingDTO>();
        _mockHousekeepingService.Setup(s => s.GetActiveAsync(1, 10, null, false)).ReturnsAsync(resultDto);

        var result = await _controller.GetPendingTasks(1, 10) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task CreateGuestTrigger_ShouldReturnOk_WhenSuccessful()
    {
        var dto = new CreateHousekeepingTaskDTO();
        _mockHousekeepingService.Setup(s => s.CreateGuestTriggerAsync(1, dto)).Returns(Task.CompletedTask);

        var result = await _controller.CreateGuestTrigger(1, dto) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task CreateGuestTrigger_ShouldReturnBadRequest_OnArgumentException()
    {
        var dto = new CreateHousekeepingTaskDTO();
        _mockHousekeepingService.Setup(s => s.CreateGuestTriggerAsync(1, dto)).ThrowsAsync(new ArgumentException());

        var result = await _controller.CreateGuestTrigger(1, dto) as BadRequestObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task CreateGuestTrigger_ShouldReturnUnauthorized_OnUnauthorizedAccessException()
    {
        var dto = new CreateHousekeepingTaskDTO();
        _mockHousekeepingService.Setup(s => s.CreateGuestTriggerAsync(1, dto)).ThrowsAsync(new UnauthorizedAccessException());

        var result = await _controller.CreateGuestTrigger(1, dto) as UnauthorizedObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(401));
    }

    [Test]
    public async Task CreateInternalTrigger_ShouldReturnOk_WhenSuccessful()
    {
        var dto = new CreateInternalHousekeepingTaskDTO();
        _mockHousekeepingService.Setup(s => s.CreateInternalTriggerAsync(dto)).Returns(Task.CompletedTask);

        var result = await _controller.CreateInternalTrigger(dto) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task CreateInternalTrigger_ShouldReturnBadRequest_OnArgumentException()
    {
        var dto = new CreateInternalHousekeepingTaskDTO();
        _mockHousekeepingService.Setup(s => s.CreateInternalTriggerAsync(dto)).ThrowsAsync(new ArgumentException());

        var result = await _controller.CreateInternalTrigger(dto) as BadRequestObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task UpdateTaskStatus_ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        _controller.ModelState.AddModelError("Status", "Required");
        var result = await _controller.UpdateTaskStatus(1, new UpdateHousekeepingStatusDTO()) as BadRequestObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task UpdateTaskStatus_ShouldReturnOk_WhenSuccessful()
    {
        var dto = new UpdateHousekeepingStatusDTO { Status = HousekeepingStatus.InProgress };
        _mockHousekeepingService.Setup(s => s.UpdateStatusAsync(1, dto.Status)).Returns(Task.CompletedTask);

        var result = await _controller.UpdateTaskStatus(1, dto) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task UpdateTaskStatus_ShouldReturnNotFound_OnArgumentException()
    {
        var dto = new UpdateHousekeepingStatusDTO { Status = HousekeepingStatus.InProgress };
        _mockHousekeepingService.Setup(s => s.UpdateStatusAsync(1, dto.Status)).ThrowsAsync(new ArgumentException());

        var result = await _controller.UpdateTaskStatus(1, dto) as NotFoundObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(404));
    }
}
