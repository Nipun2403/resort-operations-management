using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using HotelManagement.BLL.Interfaces;
using HotelManagement.DAL.Entities;

namespace HotelManagement.API.Controllers;

[ApiController]
[Route("api/v1/images")]
[Authorize(Roles = "Admin")]
public class ImagesController : ControllerBase
{
    private readonly IImageUploadService _imageUploadService;
    private readonly ICurrentUserService _currentUserService;

    public ImagesController(IImageUploadService imageUploadService, ICurrentUserService currentUserService)
    {
        _imageUploadService = imageUploadService;
        _currentUserService = currentUserService;
    }

    [HttpPost("upload-sas")]
    [EnableRateLimiting("image-upload")]
    public async Task<IActionResult> RequestUpload([FromBody] UploadSasRequest request)
    {
        var userEmail = _currentUserService.GetUserEmail();
        if (string.IsNullOrEmpty(userEmail))
            return Unauthorized("User email not found.");

        if (!Enum.TryParse<UploadEntityType>(request.EntityType, ignoreCase: true, out var entityType))
            return BadRequest($"Invalid entity type '{request.EntityType}'. Valid values: Amenity, MenuItem, RoomType.");

        try
        {
            var result = await _imageUploadService.RequestUploadAsync(
                entityType, request.FileName, request.ContentType, request.SizeBytes, userEmail, request.EntityId);

            return Ok(new
            {
                sessionId = result.SessionId.ToString(),
                uploadUrl = result.UploadUrl,
                blobUrl = result.BlobUrl,
                expiresOn = result.ExpiresOn
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{sessionId}/confirm")]
    public async Task<IActionResult> ConfirmUpload(Guid sessionId)
    {
        var userEmail = _currentUserService.GetUserEmail();
        if (string.IsNullOrEmpty(userEmail))
            return Unauthorized("User email not found.");

        try
        {
            await _imageUploadService.ConfirmUploadAsync(sessionId, userEmail);
            return Accepted(new { message = "Upload confirmed, validation queued." });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "Upload session not found." });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{sessionId}/status")]
    public async Task<IActionResult> GetStatus(Guid sessionId)
    {
        var userEmail = _currentUserService.GetUserEmail();
        if (string.IsNullOrEmpty(userEmail))
            return Unauthorized("User email not found.");

        try
        {
            var result = await _imageUploadService.GetStatusAsync(sessionId, userEmail);
            return Ok(new
            {
                status = result.Status.ToString(),
                rejectionReason = result.RejectionReason
            });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "Upload session not found." });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
}

public class UploadSasRequest
{
    public string EntityType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public int? EntityId { get; set; }
}
