using HotelManagement.BLL.DTOs;
using HotelManagement.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers;

[ApiController]
[Route("api/v1/menu-items")]
public class MenuItemsController : ControllerBase
{
    private readonly IMenuItemService _menuItemService;

    public MenuItemsController(IMenuItemService menuItemService)
    {
        _menuItemService = menuItemService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetMenuItems(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool? isAvailable = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false)
    {
        pageSize = Math.Min(pageSize, 100);
        var items = await _menuItemService.GetMenuItemsAsync(pageNumber, pageSize, isAvailable, sortBy, sortDescending);
        return Ok(items);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateMenuItem([FromBody] CreateMenuItemDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _menuItemService.CreateMenuItemAsync(dto);
        return CreatedAtAction(nameof(GetMenuItems), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateMenuItem(int id, [FromBody] MenuItemDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var result = await _menuItemService.UpdateMenuItemAsync(id, dto);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin, Kitchen")]
    public async Task<IActionResult> UpdateMenuItemStatus(int id, [FromQuery] bool isAvailable)
    {
        try
        {
            await _menuItemService.UpdateMenuItemStatusAsync(id, isAvailable);
            return Ok(new { Message = $"Menu item availability updated to {isAvailable}." });
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // [HttpDelete("{id}")]
    // [Authorize(Roles = "Admin")]
    // public async Task<IActionResult> DeleteMenuItem(int id)
    // {
    //     try
    //     {
    //         await _menuItemService.DeleteMenuItemAsync(id);
    //         return Ok(new { Message = "Menu item marked as unavailable (soft delete)." });
    //     }
    //     catch (ArgumentException ex)
    //     {
    //         return NotFound(ex.Message);
    //     }
    // }
}
