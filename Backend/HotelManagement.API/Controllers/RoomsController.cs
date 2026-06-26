using HotelManagement.BLL.DTOs;
using HotelManagement.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers;

[ApiController]
[Route("api/v1/rooms")]
public class RoomsController : ControllerBase
{
    private readonly IRoomService _roomService;
    public RoomsController(IRoomService roomService)
    {
        _roomService = roomService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,FrontDesk")]
    public async Task<IActionResult> GetRooms(
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 10, 
        [FromQuery] int? roomTypeId = null,
        [FromQuery] bool includeRetired = false,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false)
    {
        pageSize = Math.Min(pageSize, 100);
        var rooms = await _roomService.GetRoomsAsync(pageNumber, pageSize, roomTypeId, includeRetired, sortBy, sortDescending);
        return Ok(rooms);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateRoom([FromBody] CreateUpdateRoomDTO dto)
    {
        try
        {
            var room = await _roomService.CreateRoomAsync(dto);
            return Ok(new { Message = $"Room {room.RoomNumber} created successfully.", RoomId = room.Id });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPatch("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateRoom(int id, [FromBody] CreateUpdateRoomDTO dto)
    {
        try
        {
            await _roomService.UpdateRoomAsync(id, dto);
            return Ok(new { Message = $"Room {dto.RoomNumber} updated successfully." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteRoom(int id)
    {
        try
        {
            await _roomService.DeleteRoomAsync(id);
            return Ok(new { Message = "Room permanently retired (soft deleted)." });
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("status")]
    [Authorize(Roles = "Admin,FrontDesk")]
    public async Task<IActionResult> GetRoomStatusDashboard(
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 10, 
        [FromQuery] int? roomTypeId = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false)
    {
        pageSize = Math.Min(pageSize, 100);
        var result = await _roomService.GetRoomStatusDashboardAsync(pageNumber, pageSize, roomTypeId, sortBy, sortDescending);
        return Ok(result);
    }

    [HttpGet("available-for-booking/{bookingId}")]
    [Authorize(Roles = "Admin,FrontDesk")]
    public async Task<IActionResult> GetAvailableRoomsForBooking(int bookingId)
    {
        try
        {
            var rooms = await _roomService.GetAvailableRoomsForCheckInAsync(bookingId);
            return Ok(rooms);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
