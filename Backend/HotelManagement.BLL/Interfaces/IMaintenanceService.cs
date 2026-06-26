using HotelManagement.BLL.DTOs;
using System.Threading.Tasks;
using HotelManagement.Repository.Models;

namespace HotelManagement.BLL.Interfaces;

public interface IMaintenanceService
{
    Task<PaginatedResult<MaintenanceTaskDTO>> GetAllTasksAsync(int pageNumber, int pageSize, string? status = null, string? sortBy = null, bool sortDescending = false);
    Task<PaginatedResult<MaintenanceTaskDTO>> GetActiveTasksAsync(int pageNumber, int pageSize, string? sortBy = null, bool sortDescending = false);
    Task<MaintenanceTaskDTO> CreateTicketAsync(int roomId, CreateMaintenanceTaskDTO dto, string? originTypeOverride = null);
    Task<MaintenanceTaskDTO> CreateInternalTicketAsync(CreateInternalMaintenanceTaskDTO dto);
    Task<MaintenanceTaskDTO> UpdateStatusAsync(int id, UpdateMaintenanceStatusDTO dto);
}
