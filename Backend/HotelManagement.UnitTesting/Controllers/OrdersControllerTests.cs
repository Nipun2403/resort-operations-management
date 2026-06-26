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
public class OrdersControllerTests
{
    private Mock<IOrderService> _mockOrderService;
    private OrdersController _controller;

    [SetUp]
    public void Setup()
    {
        _mockOrderService = new Mock<IOrderService>();
        _controller = new OrdersController(_mockOrderService.Object);
    }

    [Test]
    public async Task GetOrders_ShouldCallGetActiveOrders_WhenStatusIsActive()
    {
        var resultDto = new PaginatedResult<FoodOrderDTO>();
        _mockOrderService.Setup(s => s.GetActiveOrdersAsync(1, 10, null, null, false)).ReturnsAsync(resultDto);

        var result = await _controller.GetOrders("active", null, 1, 10) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
        _mockOrderService.Verify(s => s.GetActiveOrdersAsync(1, 10, null, null, false), Times.Once);
    }

    [Test]
    public async Task GetOrders_ShouldCallGetAllOrders_WhenStatusIsNotActive()
    {
        var resultDto = new PaginatedResult<FoodOrderDTO>();
        _mockOrderService.Setup(s => s.GetAllOrdersAsync(1, 10, null, "completed", null, false)).ReturnsAsync(resultDto);

        var result = await _controller.GetOrders("completed", null, 1, 10) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
        _mockOrderService.Verify(s => s.GetAllOrdersAsync(1, 10, null, "completed", null, false), Times.Once);
    }

    [Test]
    public async Task CreateOrder_ShouldReturnOk_WhenSuccessful()
    {
        var request = new CreateFoodOrderDTO();
        var dto = new FoodOrderDTO { Id = 1 };
        _mockOrderService.Setup(s => s.CreateOrderAsync(request)).ReturnsAsync(dto);

        var result = await _controller.CreateOrder(request) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task CreateOrder_ShouldReturnBadRequest_OnGeneralException()
    {
        var request = new CreateFoodOrderDTO();
        _mockOrderService.Setup(s => s.CreateOrderAsync(request)).ThrowsAsync(new Exception());

        var result = await _controller.CreateOrder(request) as BadRequestObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public void CreateOrder_ShouldRethrow_KeyNotFoundException()
    {
        var request = new CreateFoodOrderDTO();
        _mockOrderService.Setup(s => s.CreateOrderAsync(request)).ThrowsAsync(new KeyNotFoundException());

        Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.CreateOrder(request));
    }

    [Test]
    public async Task UpdateOrderStatus_ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        _controller.ModelState.AddModelError("Status", "Required");
        var result = await _controller.UpdateOrderStatus(1, new UpdateOrderStatusDTO()) as BadRequestObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task UpdateOrderStatus_ShouldReturnOk_WhenSuccessful()
    {
        var dto = new UpdateOrderStatusDTO { Status = FoodOrderStatus.Preparing };
        _mockOrderService.Setup(s => s.UpdateOrderStatusAsync(1, dto.Status)).Returns(Task.CompletedTask);

        var result = await _controller.UpdateOrderStatus(1, dto) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task UpdateOrderStatus_ShouldReturnNotFound_OnArgumentException()
    {
        var dto = new UpdateOrderStatusDTO { Status = FoodOrderStatus.Preparing };
        _mockOrderService.Setup(s => s.UpdateOrderStatusAsync(1, dto.Status)).ThrowsAsync(new ArgumentException());

        var result = await _controller.UpdateOrderStatus(1, dto) as NotFoundObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(404));
    }
}
