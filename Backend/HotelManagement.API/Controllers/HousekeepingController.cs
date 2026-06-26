using HotelManagement.BLL.DTOs;
using HotelManagement.BLL.Interfaces;
using HotelManagement.DAL.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers;

[ApiController]
[Route("api/v1/housekeeping")]
public class HousekeepingController : ControllerBase
{
    private readonly IHousekeepingService _housekeepingService;
    private readonly IBookingService _bookingService;

    public HousekeepingController(IHousekeepingService housekeepingService, IBookingService bookingService)
    {
        _housekeepingService = housekeepingService;
        _bookingService = bookingService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,FrontDesk,Housekeeping,RegisteredUser")]
    public async Task<IActionResult> GetAllTasks(
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false)
    {
        pageSize = Math.Min(pageSize, 100);
        var tasks = await _housekeepingService.GetAllAsync(pageNumber, pageSize, status, sortBy, sortDescending);
        return Ok(tasks);
    }

    [HttpGet("active")]
    [Authorize(Roles = "Admin,FrontDesk,Housekeeping")]
    public async Task<IActionResult> GetPendingTasks(
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false)
    {
        pageSize = Math.Min(pageSize, 100);
        var tasks = await _housekeepingService.GetActiveAsync(pageNumber, pageSize, sortBy, sortDescending);
        return Ok(tasks);
    }

    [HttpPost("trigger/{roomId}")]
    [Authorize(Roles = "Admin,FrontDesk,RegisteredUser")]
    public async Task<IActionResult> CreateGuestTrigger(int roomId, [FromBody] CreateHousekeepingTaskDTO dto)
    {
        try
        {
            await _housekeepingService.CreateGuestTriggerAsync(roomId, dto);
            return Ok(new { Message = "Housekeeping requested successfully." });
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
    [Authorize(Roles = "Admin,FrontDesk")]
    public async Task<IActionResult> CreateInternalTrigger([FromBody] CreateInternalHousekeepingTaskDTO dto)
    {
        try
        {
            await _housekeepingService.CreateInternalTriggerAsync(dto);
            return Ok(new { Message = "Internal housekeeping task triggered successfully." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin,Housekeeping")]
    public async Task<IActionResult> UpdateTaskStatus(int id, [FromBody] UpdateHousekeepingStatusDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            await _housekeepingService.UpdateStatusAsync(id, dto.Status);
            return Ok(new { Message = $"Housekeeping task #{id} status updated to {dto.Status}." });
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }
}
