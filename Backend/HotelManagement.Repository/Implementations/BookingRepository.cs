using HotelManagement.DAL.Context;
using HotelManagement.DAL.Entities;
using HotelManagement.DAL.Enums;
using HotelManagement.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
namespace HotelManagement.Repository.Implementations;

public class BookingRepository : GenericRepository<Booking>, IBookingRepository
{
    public BookingRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Booking?> GetBookingWithDetailsAsync(int bookingId)
    {
        // return await _context.Bookings
        return await _dbSet
            .Include(b => b.BookingRooms)
                .ThenInclude(br => br.Room)
            .Include(b => b.BookingRooms)
                .ThenInclude(br => br.RoomType)
            .Include(b => b.FoodOrders)
                .ThenInclude(f => f.OrderItems)
                    .ThenInclude(i => i.MenuItem)
            .Include(b => b.BookingAmenities)
                .ThenInclude(ba => ba.Amenity)
            .FirstOrDefaultAsync(b => b.Id == bookingId);
    }

    public async Task<IEnumerable<Booking>> GetActiveBookingsAsync()
    {
        return await _dbSet
            .Include(b => b.BookingRooms)
            .Where(b => b.BookingStatus == BookingStatus.CheckedIn || b.BookingStatus == BookingStatus.Booked)
            .ToListAsync();
    }

    public async Task<IEnumerable<Booking>> GetBookingsByStatusAsync(BookingStatus status)
    {
        return await _dbSet
            .Where(b => b.BookingStatus == status)
            .ToListAsync();
    }

    public async Task<HotelManagement.Repository.Models.PaginatedResult<Booking>> GetPaginatedBookingsWithDetailsAsync(
    int pageNumber,
    int pageSize,
    IEnumerable<Expression<Func<Booking, bool>>>? predicates = null,
    Func<IQueryable<Booking>, IOrderedQueryable<Booking>>? orderBy = null)
    {
        // Start with the includes to get the rooms and types
        var query = _dbSet
            .Include(b => b.BookingRooms)
                .ThenInclude(br => br.Room)
            .Include(b => b.BookingRooms)
                .ThenInclude(br => br.RoomType)
            .AsQueryable();

        if (predicates != null)
        {
            foreach (var predicate in predicates)
            {
                query = query.Where(predicate);
            }
        }

        var totalCount = await query.CountAsync();

        if (orderBy != null) query = orderBy(query);

        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        return new HotelManagement.Repository.Models.PaginatedResult<Booking>
        {
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            Data = items
        };
    }
}
