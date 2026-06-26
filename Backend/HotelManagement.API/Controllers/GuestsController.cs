using HotelManagement.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers;

[ApiController]
[Route("api/v1/guests")]
[Authorize(Roles = "Admin,FrontDesk")]
public class GuestsController : ControllerBase
{
    private readonly IGuestService _guestService;

    public GuestsController(IGuestService guestService)
    {
        _guestService = guestService;
    }

    [HttpGet]
    public async Task<IActionResult> GetGuests([FromQuery] string? search, [FromQuery] string? status, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        pageSize = Math.Min(pageSize, 100);
        if (status != null && status.Equals("incoming", StringComparison.OrdinalIgnoreCase))
        {
            var incoming = await _guestService.GetIncomingGuestsAsync(pageNumber, pageSize);
            return Ok(incoming);
        }
        
        var searchResults = await _guestService.SearchGuestsAsync(search, pageNumber, pageSize);
        return Ok(searchResults);
    }
}
