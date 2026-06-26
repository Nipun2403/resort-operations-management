using HotelManagement.DAL.Context;
using HotelManagement.DAL.Entities;
using HotelManagement.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Repository.Implementations;

public class RoomTypeRepository : GenericRepository<RoomType>, IRoomTypeRepository
{
    public RoomTypeRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<RoomType>> GetRoomTypesAsync(bool includeRetired = false)
    {
        var query = _context.RoomTypes.AsQueryable();
        if (!includeRetired) query = query.Where(r => r.IsActive);
        
        return await query.ToListAsync();
    }
}
