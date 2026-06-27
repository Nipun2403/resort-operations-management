using HotelManagement.DAL.Entities;

namespace HotelManagement.Repository.Interfaces;

public interface IMenuItemRepository : IGenericRepository<MenuItem>
{
    Task<IEnumerable<MenuItem>> GetMenuItemsAsync(bool includeRetired = false);

    Task<HotelManagement.Repository.Models.PaginatedResult<MenuItem>> GetPaginatedMenuItemsAsync(
    int pageNumber,
    int pageSize,
    bool isAvailable,
    string? searchQuery,
    string? sortBy,
    bool sortDescending);
}
