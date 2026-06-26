using HotelManagement.DAL.Context;
using HotelManagement.DAL.Entities;
using HotelManagement.DAL.Enums;
using HotelManagement.Repository.Interfaces;
using HotelManagement.Repository.Models;
using Microsoft.EntityFrameworkCore;
namespace HotelManagement.Repository.Implementations;

public class RoomRepository : GenericRepository<Room>, IRoomRepository
{
    public RoomRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Room>> GetRoomsWithTypesAsync(bool includeRetired = false)
    {
        var query = _context.Rooms.Include(r => r.RoomType).AsQueryable();
        if (!includeRetired) query = query.Where(r => r.IsActive);
        
        return await query.ToListAsync();
    }

    public async Task<PaginatedResult<RoomTypeAvailability>> GetAvailableRoomTypesAsync(DateTime checkIn, DateTime checkOut, int pageNumber, int pageSize, string? sortBy, bool descending)
    {
        // 1. Get overlapping booking room counts per RoomTypeId
        var overlappingCounts = await _context.Set<BookingRoom>()
            .Where(br => (br.Booking.BookingStatus == BookingStatus.Booked || br.Booking.BookingStatus == BookingStatus.CheckedIn) &&
                         br.Booking.CheckInDate < checkOut && 
                         br.Booking.CheckOutDate > checkIn)
            .GroupBy(br => br.RoomTypeId)
            .Select(g => new { RoomTypeId = g.Key, ReservedCount = g.Count() })
            .ToDictionaryAsync(x => x.RoomTypeId, x => x.ReservedCount);

        // 2. Query Room Types and calculate availability
        var roomTypes = await _context.RoomTypes
            .Include(rt => rt.Rooms)
            .Where(rt => rt.IsActive)
            .ToListAsync();

        var availableRoomTypes = roomTypes.Select(rt => 
        {
            var totalActiveRooms = rt.Rooms.Count(r => r.IsActive);
            var reservedCount = overlappingCounts.GetValueOrDefault(rt.Id, 0);
            return new RoomTypeAvailability
            {
                RoomType = rt,
                AvailableCount = totalActiveRooms - reservedCount
            };
        })
        .Where(rta => rta.AvailableCount > 0)
        .AsQueryable();

        // Sorting
        if (!string.IsNullOrEmpty(sortBy))
        {
            availableRoomTypes = sortBy.ToLower() switch
            {
                "baseprice" => descending ? availableRoomTypes.OrderByDescending(rta => rta.RoomType.BasePrice) : availableRoomTypes.OrderBy(rta => rta.RoomType.BasePrice),
                "name" => descending ? availableRoomTypes.OrderByDescending(rta => rta.RoomType.Name) : availableRoomTypes.OrderBy(rta => rta.RoomType.Name),
                "availablecount" => descending ? availableRoomTypes.OrderByDescending(rta => rta.AvailableCount) : availableRoomTypes.OrderBy(rta => rta.AvailableCount),
                _ => availableRoomTypes
            };
        }

        // Pagination
        var totalCount = availableRoomTypes.Count();
        var data = availableRoomTypes.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        return new PaginatedResult<RoomTypeAvailability>
        {
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            Data = data
        };
    }

    public async Task<Room?> GetRoomWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(r => r.RoomType)
            .FirstOrDefaultAsync(r => r.Id == id);
    }
}
