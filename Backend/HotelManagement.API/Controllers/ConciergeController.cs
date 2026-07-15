using System.ClientModel;
using HotelManagement.BLL.DTOs;
using HotelManagement.BLL.Interfaces;
using HotelManagement.BLL.Services.Concierge;
using HotelManagement.API.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HotelManagement.BLL.Exceptions;

namespace HotelManagement.API.Controllers;

[ApiController]
[Route("api/v1/concierge")]
[Authorize(Roles = "RegisteredUser")]
public class ConciergeController : ControllerBase
{
    private readonly IConciergeService _concierge;

    public ConciergeController(IConciergeService concierge) => _concierge = concierge;

    [HttpPost("chat")]
    [Idempotent]
    public async Task<ActionResult<ConciergeChatResponseDTO>> Chat(
        [FromBody] ConciergeChatRequestDTO request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest(new ConciergeErrorResponseDTO
            {
                ErrorCode = "VALIDATION_ERROR",
                Message = "Message is required.",
                TraceId = HttpContext.TraceIdentifier
            });

        // Inline sanitization
        var sanitized = InputSanitizer.Sanitize(request.Message);

        try
        {
            var response = await _concierge.ProcessMessageAsync(sanitized, request.ConversationId, ct);
            return Ok(response);
        }
        catch (ConciergeValidationException ex)
        {
            return BadRequest(new ConciergeErrorResponseDTO
            {
                ErrorCode = "VALIDATION_ERROR",
                Message = ex.Message,
                Details = ex.Errors,
                TraceId = HttpContext.TraceIdentifier
            });
        }
        catch (Exception ex) when (ex is ClientResultException or HttpRequestException)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new ConciergeErrorResponseDTO
            {
                ErrorCode = "AI_SERVICE_UNAVAILABLE",
                Message = "The concierge AI is temporarily unavailable. Please try again in a moment.",
                TraceId = HttpContext.TraceIdentifier
            });
        }
    }

    [HttpPost("confirm")]
    [Idempotent]
    public async Task<ActionResult<ConciergeChatResponseDTO>> Confirm(
        [FromBody] ConciergeConfirmRequestDTO request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ConversationId))
            return BadRequest(new ConciergeErrorResponseDTO
            {
                ErrorCode = "VALIDATION_ERROR",
                Message = "ConversationId is required.",
                TraceId = HttpContext.TraceIdentifier
            });
        if (request.ProposalIds == null || request.ProposalIds.Count == 0)
            return BadRequest(new ConciergeErrorResponseDTO
            {
                ErrorCode = "VALIDATION_ERROR",
                Message = "At least one proposal ID is required.",
                TraceId = HttpContext.TraceIdentifier
            });

        try
        {
            var response = await _concierge.ConfirmProposalsAsync(request.ConversationId, request.ProposalIds, ct);
            return Ok(response);
        }
        catch (ConciergeValidationException ex)
        {
            return BadRequest(new ConciergeErrorResponseDTO
            {
                ErrorCode = "VALIDATION_ERROR",
                Message = ex.Message,
                Details = ex.Errors,
                TraceId = HttpContext.TraceIdentifier
            });
        }
        catch (ConciergeProposalExpiredException)
        {
            return BadRequest(new ConciergeErrorResponseDTO
            {
                ErrorCode = "PROPOSAL_EXPIRED",
                Message = "One or more proposals have expired. Please try again.",
                TraceId = HttpContext.TraceIdentifier
            });
        }
        catch (ConciergeProposalNotFoundException)
        {
            return NotFound(new ConciergeErrorResponseDTO
            {
                ErrorCode = "PROPOSAL_NOT_FOUND",
                Message = "One or more proposals not found.",
                TraceId = HttpContext.TraceIdentifier
            });
        }
        catch (Exception ex) when (ex is ClientResultException or HttpRequestException)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new ConciergeErrorResponseDTO
            {
                ErrorCode = "AI_SERVICE_UNAVAILABLE",
                Message = "The concierge AI is temporarily unavailable. Please try again in a moment.",
                TraceId = HttpContext.TraceIdentifier
            });
        }
    }

    [HttpGet("proposals")]
    public async Task<ActionResult<List<ConciergeProposalDTO>>> GetPendingProposals(
        [FromQuery] string conversationId,
        CancellationToken ct)
    {
        try
        {
            var proposals = await _concierge.GetPendingProposalsAsync(conversationId, ct);
            return Ok(proposals);
        }
        catch (ConciergeProposalNotFoundException)
        {
            return NotFound(new ConciergeErrorResponseDTO
            {
                ErrorCode = "PROPOSAL_NOT_FOUND",
                Message = "No pending proposals found.",
                TraceId = HttpContext.TraceIdentifier
            });
        }
    }

    [HttpGet("context")]
    public async Task<ActionResult<GuestContextDTO>> GetContext(CancellationToken ct)
    {
        var context = await _concierge.GetGuestContextAsync(ct);
        return Ok(context);
    }
}