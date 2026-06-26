using HotelManagement.BLL.DTOs;
using HotelManagement.BLL.Interfaces;
using HotelManagement.DAL.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HotelManagement.Repository.Models;

namespace HotelManagement.API.Controllers;

[ApiController]
[Route("api/v1/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    [Authorize(Roles = "Kitchen,Admin,FrontDesk,RegisteredUser")]
    public async Task<IActionResult> GetOrders(
        [FromQuery] string? status, 
        [FromQuery] int? roomId, 
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false)
    {
        pageSize = Math.Min(pageSize, 100);
        PaginatedResult<FoodOrderDTO> orders;

        if (status != null && status.Equals("active", StringComparison.OrdinalIgnoreCase))
        {
            orders = await _orderService.GetActiveOrdersAsync(pageNumber, pageSize, roomId, sortBy, sortDescending);
        }
        else
        {
            orders = await _orderService.GetAllOrdersAsync(pageNumber, pageSize, roomId, status, sortBy, sortDescending);
        }

        return Ok(orders);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Kitchen,Admin")]
    public async Task<IActionResult> GetOrder(int id)
    {
        try
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            return Ok(order);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost]
    [Authorize(Roles = "FrontDesk,Admin,RegisteredUser")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateFoodOrderDTO request)
    {
        try
        {
            var order = await _orderService.CreateOrderAsync(request);
            return Ok(new { Message = "Order created successfully.", OrderId = order.Id });
        }
        catch (Exception ex)
        {
            if (ex is KeyNotFoundException || ex is UnauthorizedAccessException) throw;
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id}")]
    [Authorize(Roles = "Kitchen,Admin")]
    public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            await _orderService.UpdateOrderStatusAsync(id, dto.Status);
            return Ok(new { Message = $"Order #{id} status updated to {dto.Status}." });
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }
}
