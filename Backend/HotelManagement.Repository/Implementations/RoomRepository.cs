using HotelManagement.DAL.Context;
using HotelManagement.DAL.Entities;
using HotelManagement.DAL.Enums;
using HotelManagement.Repository.Interfaces;
using HotelManagement.Repository.Models;
using Microsoft.EntityFrameworkCore;
using HotelManagement.Repository.Utilities;
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
            .Where(br => (br.Booking.BookingStatus == BookingStatus.Booked || br.Booking.BookingStatus == BookingStatus.CheckedIn) && br.Booking.CheckInDate < checkOut && br.Booking.CheckOutDate > checkIn)
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
    public async Task<PaginatedResult<Room>> GetPaginatedRoomsAsync(
        int pageNumber,
        int pageSize,
        bool includeRetired,
        int? roomTypeId,
        string? searchQuery,
        string? sortBy,
        bool sortDescending)
    {
        var query = _dbSet
            .Include(r => r.RoomType)
            .AsQueryable();

        // 1. Retired filter
        if (!includeRetired)
            query = query.Where(r => r.IsActive);

        // 2. RoomType filter
        if (roomTypeId.HasValue)
            query = query.Where(r => r.RoomTypeId == roomTypeId.Value);

        // 3. Search filter (case-insensitive)
        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            var lowerQuery = searchQuery.ToLower();
            query = query.Where(r =>
                r.RoomNumber.ToLower().Contains(lowerQuery) ||
                (r.RoomType != null && r.RoomType.Name.ToLower().Contains(lowerQuery)) ||
                (r.RoomType != null && r.RoomType.Description != null && r.RoomType.Description.ToLower().Contains(lowerQuery))
            );
        }

        // 4. Dynamic sorting (using the same OrderByDynamic extension from Utilities)
        if (!string.IsNullOrEmpty(sortBy))
        {
            if (sortBy.Equals("basePrice", StringComparison.OrdinalIgnoreCase))
            {
                query = sortDescending
                    ? query.OrderByDescending(r => r.RoomType.BasePrice)
                    : query.OrderBy(r => r.RoomType.BasePrice);
            }
            else if (sortBy.Equals("maxOccupancy", StringComparison.OrdinalIgnoreCase))
            {
                query = sortDescending
                    ? query.OrderByDescending(r => r.RoomType.MaxOccupancy)
                    : query.OrderBy(r => r.RoomType.MaxOccupancy);
            }
            else if (sortBy.Equals("roomTypeName", StringComparison.OrdinalIgnoreCase))
            {
                query = sortDescending
                    ? query.OrderByDescending(r => r.RoomType.Name)
                    : query.OrderBy(r => r.RoomType.Name);
            }
            else if (sortBy.Equals("isAvailable", StringComparison.OrdinalIgnoreCase))
            {
                query = sortDescending
                    ? query.OrderByDescending(r => r.IsActive)
                    : query.OrderBy(r => r.IsActive);
            }
            else
            {
                query = query.OrderByDynamic(sortBy, sortDescending);
            }
        }

        // 5. Pagination
        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResult<Room>
        {
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            Data = items
        };
    }

}
