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
public class MaintenanceControllerTests
{
    private Mock<IMaintenanceService> _mockMaintenanceService;
    private MaintenanceController _controller;

    [SetUp]
    public void Setup()
    {
        _mockMaintenanceService = new Mock<IMaintenanceService>();
        _controller = new MaintenanceController(_mockMaintenanceService.Object);
    }

    [Test]
    public async Task GetAllTasks_ShouldReturnOk()
    {
        var resultDto = new PaginatedResult<MaintenanceTaskDTO>();
        _mockMaintenanceService.Setup(s => s.GetAllTasksAsync(1, 10, null, null, false)).ReturnsAsync(resultDto);

        var result = await _controller.GetAllTasks(1, 10) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task GetPendingTasks_ShouldReturnOk()
    {
        var resultDto = new PaginatedResult<MaintenanceTaskDTO>();
        _mockMaintenanceService.Setup(s => s.GetActiveTasksAsync(1, 10, null, false)).ReturnsAsync(resultDto);

        var result = await _controller.GetPendingTasks(1, 10) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
    }

    // [Test]
    public async Task CreateTicket_ShouldReturnBadRequest_WhenRoomIdMismatch()
    {
        var dto = new CreateMaintenanceTaskDTO { };
        var result = await _controller.CreateTicket(1, dto) as BadRequestObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
        Assert.That(result.Value, Is.EqualTo("Room ID in route must match Room ID in body."));
    }

    [Test]
    public async Task CreateTicket_ShouldReturnOk_WhenSuccessful()
    {
        var dto = new CreateMaintenanceTaskDTO { };
        var taskDto = new MaintenanceTaskDTO();
        _mockMaintenanceService.Setup(s => s.CreateTicketAsync(1, dto)).ReturnsAsync(taskDto);

        var result = await _controller.CreateTicket(1, dto) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task CreateTicket_ShouldReturnBadRequest_OnArgumentException()
    {
        var dto = new CreateMaintenanceTaskDTO { };
        _mockMaintenanceService.Setup(s => s.CreateTicketAsync(1, dto)).ThrowsAsync(new ArgumentException());

        var result = await _controller.CreateTicket(1, dto) as BadRequestObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task CreateTicket_ShouldReturnUnauthorized_OnUnauthorizedAccessException()
    {
        var dto = new CreateMaintenanceTaskDTO { };
        _mockMaintenanceService.Setup(s => s.CreateTicketAsync(1, dto)).ThrowsAsync(new UnauthorizedAccessException());

        var result = await _controller.CreateTicket(1, dto) as UnauthorizedObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(401));
    }

    [Test]
    public async Task CreateInternalTicket_ShouldReturnOk_WhenSuccessful()
    {
        var dto = new CreateInternalMaintenanceTaskDTO();
        var taskDto = new MaintenanceTaskDTO();
        _mockMaintenanceService.Setup(s => s.CreateInternalTicketAsync(dto)).ReturnsAsync(taskDto);

        var result = await _controller.CreateInternalTicket(dto) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task CreateInternalTicket_ShouldReturnBadRequest_OnArgumentException()
    {
        var dto = new CreateInternalMaintenanceTaskDTO();
        _mockMaintenanceService.Setup(s => s.CreateInternalTicketAsync(dto)).ThrowsAsync(new ArgumentException());

        var result = await _controller.CreateInternalTicket(dto) as BadRequestObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task UpdateTaskStatus_ShouldReturnOk_WhenSuccessful()
    {
        var dto = new UpdateMaintenanceStatusDTO();
        var taskDto = new MaintenanceTaskDTO();
        _mockMaintenanceService.Setup(s => s.UpdateStatusAsync(1, dto)).ReturnsAsync(taskDto);

        var result = await _controller.UpdateTaskStatus(1, dto) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task UpdateTaskStatus_ShouldReturnBadRequest_OnArgumentException()
    {
        var dto = new UpdateMaintenanceStatusDTO();
        _mockMaintenanceService.Setup(s => s.UpdateStatusAsync(1, dto)).ThrowsAsync(new ArgumentException());

        var result = await _controller.UpdateTaskStatus(1, dto) as BadRequestObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
    }
}
