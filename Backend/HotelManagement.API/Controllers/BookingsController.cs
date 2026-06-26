using HotelManagement.BLL.DTOs;
using HotelManagement.BLL.Interfaces;
using HotelManagement.DAL.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers;

[ApiController]
[Route("api/v1/bookings")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly IBillingService _billingService;
    private readonly IAmenityService _amenityService;

    public BookingsController(
        IBookingService bookingService,
        IBillingService billingService,
        IAmenityService amenityService)
    {
        _bookingService = bookingService;
        _billingService = billingService;
        _amenityService = amenityService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,FrontDesk,RegisteredUser")]
    public async Task<IActionResult> GetBookings(
        [FromQuery] string? status,
        [FromQuery] string? guestQuery,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false)
    {
        pageSize = Math.Min(pageSize, 100);
        try
        {
            var bookings = await _bookingService.GetBookingsAsync(status, guestQuery, pageNumber, pageSize, sortBy, sortDescending);
            return Ok(bookings);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,FrontDesk")]
    public async Task<IActionResult> GetBooking(int id)
    {
        var booking = await _bookingService.GetBookingByIdAsync(id);
        if (booking == null) return NotFound("Booking not found.");

        return Ok(booking);
    }

    [HttpPost]
    [AllowAnonymous]
    [ServiceFilter(typeof(HotelManagement.API.Filters.IdempotentAttribute))]
    public async Task<IActionResult> BookRoom([FromBody] CreateBookingRequestDTO request)
    {
        try
        {
            var booking = await _bookingService.CreateBookingAsync(request);
            return CreatedAtAction(nameof(GetBooking), new { id = booking.Id }, booking);
        }
        catch (Exception ex)
        {
            if (ex is KeyNotFoundException || ex is UnauthorizedAccessException) throw;
            return BadRequest(ex.Message);
        }
    }

    [HttpPatch("{id}/extend-stay")]
    [Authorize(Roles = "FrontDesk,Admin")]
    public async Task<IActionResult> UpdateBooking(int id, [FromBody] UpdateBookingDTO dto)
    {
        try
        {
            // Only trigger extension if the date was actually sent
            if (dto.CheckOutDate.HasValue)
            {
                await _bookingService.ExtendStayAsync(id, dto.CheckOutDate.Value);
            }

            // Only trigger status change if the status was actually sent
            if (dto.BookingStatus.HasValue)
            {
                if (dto.BookingStatus.Value == BookingStatus.CheckedIn ||
                    dto.BookingStatus.Value == BookingStatus.CheckedOut)
                    return BadRequest("Use the explicit /checkin and /checkout endpoints.");

                await _bookingService.UpdateBookingStatusAsync(id, dto.BookingStatus.Value);
            }

            return Ok(new { Message = "Booking updated successfully." });
        }
        catch (ArgumentException ex)
        {
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            if (ex is KeyNotFoundException || ex is UnauthorizedAccessException) throw;
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id}/checkin")]
    [Authorize(Roles = "FrontDesk,Admin")]
    public async Task<IActionResult> CheckIn(int id)
    {
        try
        {
            var booking = await _bookingService.CheckInGuestAsync(id);
            return Ok(booking);
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

    [HttpPost("{id}/checkout")]
    [Authorize(Roles = "FrontDesk,Admin")]
    public async Task<IActionResult> CheckOut(int id)
    {
        try
        {
            var finalFolio = await _bookingService.UnifiedCheckoutAsync(id);
            return Ok(new { Message = "Checkout successful.", Folio = finalFolio });
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

    [HttpDelete("{id}/cancel")]
    [Authorize(Roles = "RegisteredUser,FrontDesk,Admin")]
    public async Task<IActionResult> CancelBooking(int id)
    {
        try
        {
            await _bookingService.CancelBookingAsync(id);
            return Ok(new { Message = "Booking successfully cancelled." });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            if (ex is KeyNotFoundException || ex is UnauthorizedAccessException) throw;
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id}/amenities")]
    [Authorize(Roles = "FrontDesk,Admin,RegisteredUser")]
    public async Task<IActionResult> SubscribeAmenity(int id, [FromBody] SubscribeAmenityDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            await _amenityService.SubscribeAsync(id, dto.AmenityId);
            return Ok(new { Message = "Amenity subscribed successfully." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            if (ex is KeyNotFoundException || ex is UnauthorizedAccessException) throw;
            return BadRequest(ex.Message);
        }
    }

}
