using HotelManagement.DAL.Entities;

namespace HotelManagement.Repository.Interfaces;

public interface IMenuItemRepository : IGenericRepository<MenuItem>
{
    Task<IEnumerable<MenuItem>> GetMenuItemsAsync(bool includeRetired = false);
}
