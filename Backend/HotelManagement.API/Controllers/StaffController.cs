using HotelManagement.BLL.DTOs;
using HotelManagement.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers;

[ApiController]
[Route("api/v1/staff")]
[Authorize(Roles = "Admin")]
public class StaffController : ControllerBase
{
    private readonly IStaffService _staffService;

    public StaffController(IStaffService staffService)
    {
        _staffService = staffService;
    }

    [HttpGet]
    public async Task<IActionResult> GetStaff(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool includeFired = false,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        [FromQuery] string? searchQuery = null)
    {
        pageSize = Math.Min(pageSize, 100);
        var staff = await _staffService.GetStaffAsync(pageNumber, pageSize, includeFired, sortBy, sortDescending, searchQuery);
        return Ok(staff);
    }

    [HttpPost]
    public async Task<IActionResult> CreateStaff([FromBody] StaffRegisterRequestDTO request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var staff = await _staffService.CreateStaffAsync(request);
            return Ok(new { Message = $"{request.Role} account created successfully." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateStaff(int id, [FromBody] UpdateStaffDTO request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            await _staffService.UpdateStaffAsync(id, request);
            return Ok(new { Message = "Staff details updated successfully." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> FireStaff(int id)
    {
        try
        {
            await _staffService.DeleteStaffAsync(id);
            return Ok(new { Message = "Staff member successfully deactivated (soft deleted)." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
