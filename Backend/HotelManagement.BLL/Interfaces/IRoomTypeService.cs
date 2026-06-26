using HotelManagement.BLL.DTOs;
using HotelManagement.Repository.Models;

namespace HotelManagement.BLL.Interfaces;

public interface IRoomTypeService
{
    Task<PaginatedResult<RoomTypeDTO>> GetRoomTypesAsync(int pageNumber, int pageSize, bool includeRetired = false, string? sortBy = null, bool sortDescending = false);
    Task<PaginatedResult<RoomTypeAvailabilityDTO>> GetAvailableRoomTypesAsync(DateTime checkIn, DateTime checkOut, int pageNumber, int pageSize, string? sortBy, bool descending);

    Task<RoomTypeDTO> CreateRoomTypeAsync(CreateRoomTypeDTO dto);
    Task<RoomTypeDTO> UpdateRoomTypeAsync(int id, UpdateRoomTypeDTO dto);
    Task DeleteRoomTypeAsync(int id);
}
