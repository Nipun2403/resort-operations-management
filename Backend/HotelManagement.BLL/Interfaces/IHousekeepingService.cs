using HotelManagement.BLL.DTOs;
using HotelManagement.DAL.Enums;
using HotelManagement.Repository.Models;
namespace HotelManagement.BLL.Interfaces;
public interface IHousekeepingService
{
    Task CreateCheckoutTriggerAsync(int roomId);
    Task CreateGuestTriggerAsync(int roomId, CreateHousekeepingTaskDTO dto);
    Task CreateInternalTriggerAsync(CreateInternalHousekeepingTaskDTO dto);
    Task UpdateStatusAsync(int taskId, HousekeepingStatus status);
    Task<PaginatedResult<HousekeepingDTO>> GetAllAsync(int pageNumber, int pageSize, string? status = null, string? sortBy = null, bool sortDescending = false, bool assignedToMe = false);
    Task<PaginatedResult<HousekeepingDTO>> GetActiveAsync(int pageNumber, int pageSize, string? sortBy = null, bool sortDescending = false);
    Task<PaginatedResult<HousekeepingDTO>> GetActiveTasksAsync(int pageNumber, int pageSize, int roomId);
}
