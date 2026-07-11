using HotelManagement.BLL.DTOs;
using HotelManagement.BLL.Interfaces;
using HotelManagement.DAL.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class MaintenanceController : ControllerBase
{
    private readonly IMaintenanceService _maintenanceService;

    public MaintenanceController(IMaintenanceService maintenanceService)
    {
        _maintenanceService = maintenanceService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,FrontDesk,Housekeeping,Maintenance,RegisteredUser")]
    public async Task<IActionResult> GetAllTasks(
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        [FromQuery] bool assignedToMe = false)
    {
        pageSize = Math.Min(pageSize, 100);
        var tasks = await _maintenanceService.GetAllTasksAsync(pageNumber, pageSize, status, sortBy, sortDescending, assignedToMe);
        return Ok(tasks);
    }

    [HttpGet("active")]
    [Authorize(Roles = "Admin,Maintenance")]
    public async Task<IActionResult> GetPendingTasks(
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false)
    {
        pageSize = Math.Min(pageSize, 100);
        var tasks = await _maintenanceService.GetActiveTasksAsync(pageNumber, pageSize, sortBy, sortDescending);
        return Ok(tasks);
    }

    [HttpPost("trigger/{roomId}")]
    [Authorize(Roles = "Admin,FrontDesk,Housekeeping,RegisteredUser")]
    public async Task<IActionResult> CreateTicket(int roomId, [FromBody] CreateMaintenanceTaskDTO dto)
    {

        try
        {
            var task = await _maintenanceService.CreateTicketAsync(roomId, dto);
            return Ok(task);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    [HttpPost("internal")]
    [Authorize(Roles = "Admin,FrontDesk,Maintenance")]
    public async Task<IActionResult> CreateInternalTicket([FromBody] CreateInternalMaintenanceTaskDTO dto)
    {
        try
        {
            var task = await _maintenanceService.CreateInternalTicketAsync(dto);
            return Ok(task);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin,Maintenance")]
    public async Task<IActionResult> UpdateTaskStatus(int id, [FromBody] UpdateMaintenanceStatusDTO dto)
    {
        try
        {
            var task = await _maintenanceService.UpdateStatusAsync(id, dto);
            return Ok(task);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
