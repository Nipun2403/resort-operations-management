using HotelManagement.DAL.Context;
using HotelManagement.DAL.Entities;
using HotelManagement.Repository.Interfaces;
using HotelManagement.Repository.Models;
using HotelManagement.Repository.Utilities;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Repository.Implementations;

public class MenuItemRepository : GenericRepository<MenuItem>, IMenuItemRepository
{
    public MenuItemRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<MenuItem>> GetMenuItemsAsync(bool includeRetired = false)
    {
        var query = _context.MenuItems.AsQueryable();
        if (!includeRetired) query = query.Where(m => m.IsAvailable);
        return await query.ToListAsync();
    }

    public async Task<PaginatedResult<MenuItem>> GetPaginatedMenuItemsAsync(
        int pageNumber,
        int pageSize,
        bool? isAvailable,
        string? category,
        string? searchQuery,
        string? sortBy,
        bool sortDescending)
    {
        var query = _dbSet.AsQueryable();

        if (isAvailable.HasValue && isAvailable == true)
            query = query.Where(a => a.IsAvailable);

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(m => m.Category == category);

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            var lowerQuery = searchQuery.ToLower();
            query = query.Where(m =>
                m.Name.ToLower().Contains(lowerQuery) ||
                m.Category.ToLower().Contains(lowerQuery)
            );
        }

        if (!string.IsNullOrEmpty(sortBy))
        {
            query = query.OrderByDynamic(sortBy, sortDescending);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResult<MenuItem>
        {
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            Data = items
        };
    }
}