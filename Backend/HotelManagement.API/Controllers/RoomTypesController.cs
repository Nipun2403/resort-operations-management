using HotelManagement.BLL.DTOs;
using HotelManagement.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers;

[ApiController]
[Route("api/v1/room-types")]
public class RoomTypesController : ControllerBase
{
    private readonly IRoomTypeService _roomTypeService;

    public RoomTypesController(IRoomTypeService roomTypeService)
    {
        _roomTypeService = roomTypeService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetRoomTypes([FromQuery] bool includeRetired = false, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string? sortBy = null, [FromQuery] bool sortDescending = false)
    {
        pageSize = Math.Min(pageSize, 100);
        var result = await _roomTypeService.GetRoomTypesAsync(pageNumber, pageSize, includeRetired, sortBy, sortDescending);
        return Ok(result);
    }

    [HttpGet("availability")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAvailableRoomTypes(
        [FromQuery] string checkIn,
        [FromQuery] string checkOut,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = "BasePrice",
        [FromQuery] bool descending = false)
    {
        if (!DateTime.TryParseExact(checkIn, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var parsedCheckIn))
            return BadRequest("Invalid checkIn format. Use dd-MM-yyyy");

        if (!DateTime.TryParseExact(checkOut, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var parsedCheckOut))
            return BadRequest("Invalid checkOut format. Use dd-MM-yyyy");

        var checkInUtc = DateTime.SpecifyKind(parsedCheckIn, DateTimeKind.Utc);
        var checkOutUtc = DateTime.SpecifyKind(parsedCheckOut, DateTimeKind.Utc);

        if (checkInUtc >= checkOutUtc) return BadRequest("CheckOut must be strictly after CheckIn.");

        var paginatedResult = await _roomTypeService.GetAvailableRoomTypesAsync(checkInUtc, checkOutUtc, pageNumber, pageSize, sortBy, descending);

        return Ok(paginatedResult);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateRoomType([FromBody] CreateRoomTypeDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var result = await _roomTypeService.CreateRoomTypeAsync(dto);
            return CreatedAtAction(nameof(GetRoomTypes), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPatch("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateRoomType(int id, [FromBody] UpdateRoomTypeDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var result = await _roomTypeService.UpdateRoomTypeAsync(id, dto);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteRoomType(int id)
    {
        try
        {
            await _roomTypeService.DeleteRoomTypeAsync(id);
            return Ok(new { Message = "Room Type permanently retired (soft deleted)." });
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
