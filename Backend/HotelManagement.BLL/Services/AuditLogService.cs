using AutoMapper;
using HotelManagement.BLL.DTOs;
using HotelManagement.BLL.Interfaces;
using HotelManagement.Repository.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelManagement.Repository.Models;
using HotelManagement.Repository.Utilities;
using HotelManagement.DAL.Entities;

namespace HotelManagement.BLL.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IMapper _mapper;

    public AuditLogService(IAuditLogRepository auditLogRepository, IMapper mapper)
    {
        _auditLogRepository = auditLogRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<AuditLogDTO>> GetAuditLogsAsync(int pageNumber, int pageSize, string? sortBy = null, bool sortDescending = false)
    {
        Func<IQueryable<AuditLog>, IOrderedQueryable<AuditLog>>? orderBy = null;
        if (!string.IsNullOrEmpty(sortBy))
        {
            orderBy = q => q.OrderByDynamic(sortBy, sortDescending);
        }

        var pagedLogs = await _auditLogRepository.GetPaginatedResultAsync(pageNumber, pageSize, null, orderBy);
        var dtos = _mapper.Map<IEnumerable<AuditLogDTO>>(pagedLogs.Data);
        
        return new PaginatedResult<AuditLogDTO>
        {
            TotalCount = pagedLogs.TotalCount,
            PageNumber = pagedLogs.PageNumber,
            PageSize = pagedLogs.PageSize,
            Data = dtos
        };
    }

    public async Task<AuditLogDTO?> GetAuditLogByIdAsync(int id)
    {
        var log = await _auditLogRepository.GetByIdAsync(id);
        if (log == null) return null;
        return _mapper.Map<AuditLogDTO>(log);
    }
}
