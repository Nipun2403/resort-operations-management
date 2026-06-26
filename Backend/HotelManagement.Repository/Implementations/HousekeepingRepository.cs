using HotelManagement.DAL.Context;
using HotelManagement.DAL.Entities;
using HotelManagement.DAL.Enums;
using HotelManagement.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
namespace HotelManagement.Repository.Implementations;

public class HousekeepingRepository : GenericRepository<Housekeeping>, IHousekeepingRepository
{
    public HousekeepingRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Housekeeping>> GetActiveTasksForRoomAsync(int roomId)
    {
        return await _context.HousekeepingTasks
            .Where(h => h.RoomId == roomId && 
                       (h.Status == HousekeepingStatus.Pending || h.Status == HousekeepingStatus.InProgress))
            .ToListAsync();
    }
}
