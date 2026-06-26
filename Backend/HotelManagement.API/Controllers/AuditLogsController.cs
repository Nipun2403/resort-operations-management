using HotelManagement.BLL.Interfaces;
using HotelManagement.DAL.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = UserRoles.Admin)]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogsController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false)
    {
        pageSize = Math.Min(pageSize, 100);
        var logs = await _auditLogService.GetAuditLogsAsync(pageNumber, pageSize, sortBy, sortDescending);
        return Ok(logs);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAuditLog(int id)
    {
        var log = await _auditLogService.GetAuditLogByIdAsync(id);
        if (log == null) return NotFound("Audit log not found.");
        return Ok(log);
    }
}
