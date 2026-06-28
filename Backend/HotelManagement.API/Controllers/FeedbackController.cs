using HotelManagement.BLL.DTOs;
using HotelManagement.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers;

[ApiController]
[Route("api/v1/feedback")]
public class FeedbackController : ControllerBase
{
    private readonly IFeedbackService _feedbackService;

    public FeedbackController(IFeedbackService feedbackService)
    {
        _feedbackService = feedbackService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,FrontDesk")]
    public async Task<IActionResult> GetAllFeedback(
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 10, 
        [FromQuery] bool includeHidden = false,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false)
    {
        pageSize = Math.Min(pageSize, 100);
        var feedback = await _feedbackService.GetFeedbackAsync(pageNumber, pageSize, includeHidden, sortBy, sortDescending);
        return Ok(feedback);
    }

    [HttpGet("booking/{bookingId}")]
    [Authorize(Roles = "Admin,FrontDesk,RegisteredUser")]
    public async Task<IActionResult> GetFeedbackForBooking(int bookingId)
    {
        var feedback = await _feedbackService.GetFeedbackForBookingAsync(bookingId);
        return Ok(feedback);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,FrontDesk,RegisteredUser")]
    public async Task<IActionResult> SubmitFeedback([FromBody] CreateFeedbackDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var result = await _feedbackService.SubmitFeedbackAsync(dto);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPatch("{id}/moderate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ModerateFeedback(int id, [FromBody] ModerateFeedbackRequestDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var result = await _feedbackService.ModerateFeedbackAsync(id, dto.IsHidden);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }
}
