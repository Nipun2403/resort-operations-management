using HotelManagement.BLL.DTOs;
using HotelManagement.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers;

[ApiController]
[Route("api/v1/amenities")]
public class AmenitiesController : ControllerBase
{
    private readonly IAmenityService _amenityService;

    public AmenitiesController(IAmenityService amenityService)
    {
        _amenityService = amenityService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAmenities(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchQuery = null,      // NEW
        [FromQuery] string? sortBy = null,           // NEW
        [FromQuery] bool sortDescending = false)     // NEW
    {
        pageSize = Math.Min(pageSize, 100);
        var amenities = await _amenityService.GetAllAmenitiesAsync(
            pageNumber,
            pageSize,
            searchQuery,
            sortBy,
            sortDescending);
        return Ok(amenities);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,FrontDesk,RegisteredUser")]
    public async Task<IActionResult> GetAmenity(int id)
    {
        var amenity = await _amenityService.GetAmenityByIdAsync(id);
        if (amenity == null) return NotFound("Amenity not found.");
        return Ok(amenity);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateAmenity([FromBody] CreateUpdateAmenityDTO request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var amenity = await _amenityService.CreateAmenityAsync(request);
        return CreatedAtAction(nameof(GetAmenity), new { id = amenity.Id }, amenity);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateAmenity(int id, [FromBody] CreateUpdateAmenityDTO request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            await _amenityService.UpdateAmenityAsync(id, request, request.IsAvailable);
            return Ok(new { Message = "Amenity updated successfully." });
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // [HttpPatch("{id}/status")]
    // [Authorize(Roles = "Admin")]
    // public async Task<IActionResult> UpdateAmenityStatus(int id, [FromQuery] bool isAvailable)
    // {
    //     try
    //     {
    //         await _amenityService.UpdateAmenityStatusAsync(id, isAvailable);
    //         return Ok(new { Message = $"Amenity availability updated to {isAvailable}." });
    //     }
    //     catch (ArgumentException ex)
    //     {
    //         return NotFound(ex.Message);
    //     }
    // }

    // [HttpDelete("{id}")]
    // [Authorize(Roles = "Admin")]
    // public async Task<IActionResult> DeleteAmenity(int id)
    // {
    //     try
    //     {
    //         await _amenityService.DeleteAmenityAsync(id);
    //         return Ok(new { Message = "Amenity deleted (deactivated) successfully." });
    //     }
    //     catch (ArgumentException ex)
    //     {
    //         return NotFound(ex.Message);
    //     }
    // }
}
