using HotelManagement.BLL.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

using HotelManagement.Repository.Models;

namespace HotelManagement.BLL.Interfaces;

public interface IAuditLogService
{
    Task<PaginatedResult<AuditLogDTO>> GetAuditLogsAsync(int pageNumber, int pageSize, string? guestQuery = null, string? sortBy = null, bool sortDescending = false);
    Task<AuditLogDTO?> GetAuditLogByIdAsync(int id);
}
