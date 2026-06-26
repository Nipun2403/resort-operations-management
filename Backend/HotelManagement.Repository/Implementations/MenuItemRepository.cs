using HotelManagement.DAL.Context;
using HotelManagement.DAL.Entities;
using HotelManagement.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Repository.Implementations;

public class MenuItemRepository : GenericRepository<MenuItem>, IMenuItemRepository
{
    public MenuItemRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<MenuItem>> GetMenuItemsAsync(bool includeRetired = false)
    {
        var query = _context.MenuItems.AsQueryable();
        // MenuItem uses IsAvailable instead of IsActive for soft deletes.
        if (!includeRetired) query = query.Where(m => m.IsAvailable);
        
        return await query.ToListAsync();
    }
}
