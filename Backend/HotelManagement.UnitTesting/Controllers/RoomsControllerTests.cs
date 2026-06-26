using System;
using System.Collections.Generic;
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
public class RoomsControllerTests
{
    private Mock<IRoomService> _mockRoomService;

    private RoomsController _controller;

    [SetUp]
    public void Setup()
    {
        _mockRoomService = new Mock<IRoomService>();
        _controller = new RoomsController(_mockRoomService.Object);
    }

    [Test]
    public async Task GetRooms_ShouldReturnOk()
    {
        var resultDto = new PaginatedResult<RoomDTO>();
        _mockRoomService.Setup(s => s.GetRoomsAsync(1, 10, null, false, null, false)).ReturnsAsync(resultDto);

        var result = await _controller.GetRooms(1, 10) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task CreateRoom_ShouldReturnOk_WhenSuccessful()
    {
        var dto = new CreateUpdateRoomDTO();
        var resultDto = new RoomDTO { Id = 1, RoomNumber = "101" };
        _mockRoomService.Setup(s => s.CreateRoomAsync(dto)).ReturnsAsync(resultDto);

        var result = await _controller.CreateRoom(dto) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task CreateRoom_ShouldReturnBadRequest_OnArgumentException()
    {
        var dto = new CreateUpdateRoomDTO();
        _mockRoomService.Setup(s => s.CreateRoomAsync(dto)).ThrowsAsync(new ArgumentException());

        var result = await _controller.CreateRoom(dto) as BadRequestObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task UpdateRoom_ShouldReturnOk_WhenSuccessful()
    {
        var dto = new CreateUpdateRoomDTO();
        _mockRoomService.Setup(s => s.UpdateRoomAsync(1, dto)).ReturnsAsync(new RoomDTO());

        var result = await _controller.UpdateRoom(1, dto) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task UpdateRoom_ShouldReturnBadRequest_OnArgumentException()
    {
        var dto = new CreateUpdateRoomDTO();
        _mockRoomService.Setup(s => s.UpdateRoomAsync(1, dto)).ThrowsAsync(new ArgumentException());

        var result = await _controller.UpdateRoom(1, dto) as BadRequestObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task DeleteRoom_ShouldReturnOk_WhenSuccessful()
    {
        _mockRoomService.Setup(s => s.DeleteRoomAsync(1)).Returns(Task.CompletedTask);

        var result = await _controller.DeleteRoom(1) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task DeleteRoom_ShouldReturnNotFound_OnArgumentException()
    {
        _mockRoomService.Setup(s => s.DeleteRoomAsync(1)).ThrowsAsync(new ArgumentException());

        var result = await _controller.DeleteRoom(1) as NotFoundObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(404));
    }

    [Test]
    public async Task GetRoomStatusDashboard_ShouldReturnOk()
    {
        var resultDto = new PaginatedResult<RoomStatusDashboardDTO>();
        _mockRoomService.Setup(s => s.GetRoomStatusDashboardAsync(1, 10, null, null, false)).ReturnsAsync(resultDto);

        var result = await _controller.GetRoomStatusDashboard(1, 10) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task GetAvailableRoomsForBooking_ShouldReturnOk_WhenSuccessful()
    {
        var rooms = new List<RoomDTO>();
        _mockRoomService.Setup(s => s.GetAvailableRoomsForCheckInAsync(1)).ReturnsAsync(rooms);

        var result = await _controller.GetAvailableRoomsForBooking(1) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task GetAvailableRoomsForBooking_ShouldReturnBadRequest_OnArgumentException()
    {
        _mockRoomService.Setup(s => s.GetAvailableRoomsForCheckInAsync(1)).ThrowsAsync(new ArgumentException());

        var result = await _controller.GetAvailableRoomsForBooking(1) as BadRequestObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
    }
}
