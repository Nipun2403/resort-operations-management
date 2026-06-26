using HotelManagement.DAL.Context;
using HotelManagement.DAL.Entities;
using HotelManagement.DAL.Enums;
using HotelManagement.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Repository.Implementations;

public class FoodOrderRepository : GenericRepository<FoodOrder>, IFoodOrderRepository
{
    public FoodOrderRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<FoodOrder>> GetActiveOrdersWithDetailsAsync()
    {
        return await _context.FoodOrders
            .Include(fo => fo.OrderItems)
            .ThenInclude(oi => oi.MenuItem)
            .Where(fo => fo.OrderStatus == FoodOrderStatus.Pending || fo.OrderStatus == FoodOrderStatus.Preparing)
            .OrderBy(fo => fo.GeneratedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<FoodOrder>> GetAllOrdersWithDetailsAsync()
    {
        return await _context.FoodOrders
            .Include(fo => fo.OrderItems)
            .ThenInclude(oi => oi.MenuItem)
            .OrderByDescending(fo => fo.GeneratedAt)
            .ToListAsync();
    }
}
