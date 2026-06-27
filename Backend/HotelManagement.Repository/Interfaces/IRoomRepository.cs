using HotelManagement.DAL.Entities;
using HotelManagement.Repository.Models;

namespace HotelManagement.Repository.Interfaces;

public interface IRoomRepository : IGenericRepository<Room>
{
    Task<IEnumerable<Room>> GetRoomsWithTypesAsync(bool includeRetired = false);
    Task<PaginatedResult<RoomTypeAvailability>> GetAvailableRoomTypesAsync(DateTime checkIn, DateTime checkOut, int pageNumber, int pageSize, string? sortBy, bool descending);
    Task<Room?> GetRoomWithDetailsAsync(int id);
    Task<PaginatedResult<Room>> GetPaginatedRoomsAsync(
        int pageNumber,
        int pageSize,
        bool includeRetired,
        int? roomTypeId,
        string? searchQuery,          // free-text search
        string? sortBy,
        bool sortDescending);
}