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
public class StaffControllerTests
{
    private Mock<IStaffService> _mockStaffService;
    private StaffController _controller;

    [SetUp]
    public void Setup()
    {
        _mockStaffService = new Mock<IStaffService>();
        _controller = new StaffController(_mockStaffService.Object);
    }

    [Test]
    public async Task GetStaff_ShouldReturnOk()
    {
        var resultDto = new PaginatedResult<StaffResponseDTO>();
        _mockStaffService.Setup(s => s.GetStaffAsync(1, 10, false, null, false)).ReturnsAsync(resultDto);

        var result = await _controller.GetStaff(1, 10) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task CreateStaff_ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        _controller.ModelState.AddModelError("Email", "Required");
        var result = await _controller.CreateStaff(new StaffRegisterRequestDTO()) as BadRequestObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task CreateStaff_ShouldReturnOk_WhenSuccessful()
    {
        var dto = new StaffRegisterRequestDTO();
        var resultDto = new StaffResponseDTO();
        _mockStaffService.Setup(s => s.CreateStaffAsync(dto)).ReturnsAsync(resultDto);

        var result = await _controller.CreateStaff(dto) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task CreateStaff_ShouldReturnBadRequest_OnArgumentException()
    {
        var dto = new StaffRegisterRequestDTO();
        _mockStaffService.Setup(s => s.CreateStaffAsync(dto)).ThrowsAsync(new ArgumentException());

        var result = await _controller.CreateStaff(dto) as BadRequestObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task CreateStaff_ShouldReturnConflict_OnInvalidOperationException()
    {
        var dto = new StaffRegisterRequestDTO();
        _mockStaffService.Setup(s => s.CreateStaffAsync(dto)).ThrowsAsync(new InvalidOperationException());

        var result = await _controller.CreateStaff(dto) as ConflictObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(409));
    }

    [Test]
    public async Task UpdateStaff_ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        _controller.ModelState.AddModelError("Email", "Required");
        var result = await _controller.UpdateStaff(1, new UpdateStaffDTO()) as BadRequestObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task UpdateStaff_ShouldReturnOk_WhenSuccessful()
    {
        var dto = new UpdateStaffDTO();
        _mockStaffService.Setup(s => s.UpdateStaffAsync(1, dto)).ReturnsAsync(new StaffResponseDTO());

        var result = await _controller.UpdateStaff(1, dto) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task UpdateStaff_ShouldReturnBadRequest_OnArgumentException()
    {
        var dto = new UpdateStaffDTO();
        _mockStaffService.Setup(s => s.UpdateStaffAsync(1, dto)).ThrowsAsync(new ArgumentException());

        var result = await _controller.UpdateStaff(1, dto) as BadRequestObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task FireStaff_ShouldReturnOk_WhenSuccessful()
    {
        _mockStaffService.Setup(s => s.DeleteStaffAsync(1)).Returns(Task.CompletedTask);

        var result = await _controller.FireStaff(1) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task FireStaff_ShouldReturnBadRequest_OnArgumentException()
    {
        _mockStaffService.Setup(s => s.DeleteStaffAsync(1)).ThrowsAsync(new ArgumentException());

        var result = await _controller.FireStaff(1) as BadRequestObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
    }
}
