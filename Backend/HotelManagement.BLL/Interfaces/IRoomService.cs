using HotelManagement.BLL.DTOs;
using HotelManagement.Repository.Models;
namespace HotelManagement.BLL.Interfaces;

public interface IRoomService
{

    // Core CRUD
    Task<PaginatedResult<RoomDTO>> GetRoomsAsync(int pageNumber, int pageSize, int? roomTypeId = null, bool includeRetired = false, string? sortBy = null, bool sortDescending = false, string? searchQuery = null);
    Task<RoomDTO> CreateRoomAsync(CreateUpdateRoomDTO dto);
    Task<RoomDTO> UpdateRoomAsync(int id, CreateUpdateRoomDTO dto);
    Task DeleteRoomAsync(int id);

    // Status & Availability
    Task<PaginatedResult<RoomStatusDashboardDTO>> GetRoomStatusDashboardAsync(int pageNumber, int pageSize, int? roomTypeId = null, string? sortBy = null, bool sortDescending = false);
    Task<IEnumerable<RoomDTO>> GetAvailableRoomsForCheckInAsync(int bookingId);
}
