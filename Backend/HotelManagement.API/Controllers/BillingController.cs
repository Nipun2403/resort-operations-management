using HotelManagement.BLL.DTOs;
using HotelManagement.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers;

[ApiController]
[Route("api/v1/billing")]
public class BillingController : ControllerBase
{
    private readonly IBillingService _billingService;
    private readonly IPdfService _pdfService;

    public BillingController(IBillingService billingService, IPdfService pdfService)
    {
        _billingService = billingService;
        _pdfService = pdfService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,FrontDesk")]
    public async Task<IActionResult> GetGlobalBilling(
        [FromQuery] string? paymentStatus,
        [FromQuery] string? startDate,
        [FromQuery] string? endDate,
        [FromQuery] string? search,
        [FromQuery] bool detailed = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false)
    {
        DateTime? parsedStartDate = null;
        DateTime? parsedEndDate = null;

        if (!string.IsNullOrEmpty(startDate) && DateTime.TryParseExact(startDate, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var sDate))
        {
            parsedStartDate = DateTime.SpecifyKind(sDate, DateTimeKind.Utc);
        }

        if (!string.IsNullOrEmpty(endDate) && DateTime.TryParseExact(endDate, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var eDate))
        {
            parsedEndDate = DateTime.SpecifyKind(eDate, DateTimeKind.Utc);
        }

        pageSize = Math.Min(pageSize, 100);
        var result = await _billingService.GetGlobalBillingAsync(paymentStatus, parsedStartDate, parsedEndDate, search, detailed, pageNumber, pageSize, sortBy, sortDescending);
        return Ok(result);
    }

    [HttpGet("receipts")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetReceipts(
        [FromQuery] string? startDate,
        [FromQuery] string? endDate,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false)
    {
        DateTime? parsedStartDate = null;
        DateTime? parsedEndDate = null;

        if (!string.IsNullOrEmpty(startDate) && DateTime.TryParseExact(startDate, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var sDate))
        {
            parsedStartDate = DateTime.SpecifyKind(sDate, DateTimeKind.Utc);
        }

        if (!string.IsNullOrEmpty(endDate) && DateTime.TryParseExact(endDate, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var eDate))
        {
            parsedEndDate = DateTime.SpecifyKind(eDate, DateTimeKind.Utc);
        }

        pageSize = Math.Min(pageSize, 100);
        var receipts = await _billingService.GetReceiptsAsync(parsedStartDate, parsedEndDate, pageNumber, pageSize, sortBy, sortDescending);
        return Ok(receipts);
    }

    [HttpGet("receipts/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetReceiptById(int id)
    {
        try
        {
            var receipt = await _billingService.GetReceiptByIdAsync(id);
            return Ok(receipt);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("{bookingId}")]
    [Authorize(Roles = "FrontDesk,Admin,RegisteredUser")]
    public async Task<IActionResult> GetBillingFolio(int bookingId)
    {
        try
        {
            var folio = await _billingService.GenerateFolioAsync(bookingId);
            return Ok(folio);
        }
        catch (Exception ex)
        {
            if (ex is KeyNotFoundException || ex is UnauthorizedAccessException) throw;
            return NotFound(ex.Message);
        }
    }

    /// <summary>Download the billing folio for a booking as a PDF.</summary>
    [HttpGet("{bookingId}/folio/pdf")]
    [Authorize(Roles = "FrontDesk,Admin,RegisteredUser")]
    public async Task<IActionResult> DownloadFolioPdf(int bookingId)
    {
        try
        {
            var folio = await _billingService.GenerateFolioAsync(bookingId);
            var pdfBytes = _pdfService.GenerateFolioPdf(folio);
            var fileName = $"Aetheris_Folio_BK-{bookingId}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("{bookingId}/pay")]
    [Authorize(Roles = "FrontDesk,Admin")]
    public async Task<IActionResult> ProcessPayment(int bookingId, [FromBody] PaymentRequestDTO request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            if (string.Equals(request.PaymentType, "Service", StringComparison.OrdinalIgnoreCase))
                await _billingService.ProcessServicePaymentAsync(bookingId, request);
            else
                await _billingService.ProcessPaymentAsync(bookingId, request);

            return Ok(new { Message = "Payment processed successfully.", BookingId = bookingId });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            if (ex is KeyNotFoundException || ex is UnauthorizedAccessException) throw;
            return BadRequest(new { message = ex.Message });
        }
    }
}
