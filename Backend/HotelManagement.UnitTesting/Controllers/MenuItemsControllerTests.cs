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
public class MenuItemsControllerTests
{
    private Mock<IMenuItemService> _mockMenuItemService;
    private MenuItemsController _controller;

    [SetUp]
    public void Setup()
    {
        _mockMenuItemService = new Mock<IMenuItemService>();
        _controller = new MenuItemsController(_mockMenuItemService.Object);
    }

    [Test]
    public async Task GetMenuItems_ShouldReturnOk()
    {
        var resultDto = new PaginatedResult<MenuItemDTO>();
        _mockMenuItemService.Setup(s => s.GetMenuItemsAsync(1, 10, false, null, null, false)).ReturnsAsync(resultDto);

        var result = await _controller.GetMenuItems(1, 10) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task CreateMenuItem_ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        _controller.ModelState.AddModelError("Name", "Required");
    }

    [Test]
    public async Task CreateMenuItem_ShouldReturnCreatedAtAction_WhenSuccessful()
    {
        var dto = new CreateMenuItemDTO();
        var resultDto = new MenuItemDTO { Id = 1 };
        _mockMenuItemService.Setup(s => s.CreateMenuItemAsync(dto)).ReturnsAsync(resultDto);

        var result = await _controller.CreateMenuItem(dto) as CreatedAtActionResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(201));
    }

    [Test]
    public async Task UpdateMenuItem_ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        _controller.ModelState.AddModelError("Name", "Required");
        var result = await _controller.UpdateMenuItem(1, new MenuItemDTO()) as BadRequestObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task UpdateMenuItem_ShouldReturnOk_WhenSuccessful()
    {
        var dto = new CreateMenuItemDTO();
        var resultDto = new MenuItemDTO();
    }

    [Test]
    public async Task UpdateMenuItem_ShouldReturnNotFound_OnArgumentException()
    {
        var dto = new CreateMenuItemDTO();
    }

    [Test]
    public async Task UpdateMenuItemStatus_ShouldReturnOk_WhenSuccessful()
    {
        _mockMenuItemService.Setup(s => s.UpdateMenuItemStatusAsync(1, true)).Returns(Task.CompletedTask);

        var result = await _controller.UpdateMenuItemStatus(1, true) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task UpdateMenuItemStatus_ShouldReturnNotFound_OnArgumentException()
    {
        _mockMenuItemService.Setup(s => s.UpdateMenuItemStatusAsync(1, true)).ThrowsAsync(new ArgumentException());

        var result = await _controller.UpdateMenuItemStatus(1, true) as NotFoundObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(404));
    }


}
