using HotelManagement.BLL.DTOs;
using HotelManagement.BLL.Interfaces;
using HotelManagement.BLL.Services.Concierge;
using HotelManagement.API.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
            return BadRequest("Message is required.");

        // Inline sanitization
        var sanitized = InputSanitizer.Sanitize(request.Message);

        var response = await _concierge.ProcessMessageAsync(sanitized, request.ConversationId, ct);
        return Ok(response);
    }

    [HttpPost("confirm")]
    [Idempotent]
    public async Task<ActionResult<ConciergeChatResponseDTO>> Confirm(
        [FromBody] ConciergeConfirmRequestDTO request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ConversationId))
            return BadRequest("ConversationId is required.");
        if (request.ProposalIds == null || request.ProposalIds.Count == 0)
            return BadRequest("At least one proposal ID is required.");

        var response = await _concierge.ConfirmProposalsAsync(request.ConversationId, request.ProposalIds, ct);
        return Ok(response);
    }

    [HttpGet("proposals")]
    public async Task<ActionResult<List<ConciergeProposalDTO>>> GetPendingProposals(
        [FromQuery] string conversationId,
        CancellationToken ct)
    {
        var proposals = await _concierge.GetPendingProposalsAsync(conversationId, ct);
        return Ok(proposals);
    }

    [HttpGet("context")]
    public async Task<ActionResult<GuestContextDTO>> GetContext(CancellationToken ct)
    {
        var context = await _concierge.GetGuestContextAsync(ct);
        return Ok(context);
    }
}