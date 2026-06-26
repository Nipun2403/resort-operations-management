using HotelManagement.DAL.Entities;
using HotelManagement.DAL.Enums;
using System.Linq.Expressions;
namespace HotelManagement.Repository.Interfaces;

public interface IBookingRepository : IGenericRepository<Booking>
{
    Task<Booking?> GetBookingWithDetailsAsync(int bookingId);
    Task<IEnumerable<Booking>> GetActiveBookingsAsync();
    Task<IEnumerable<Booking>> GetBookingsByStatusAsync(BookingStatus status);
    Task<HotelManagement.Repository.Models.PaginatedResult<Booking>> GetPaginatedBookingsWithDetailsAsync(
        int pageNumber,
        int pageSize,
        IEnumerable<Expression<Func<Booking, bool>>>? predicates = null,
        Func<IQueryable<Booking>, IOrderedQueryable<Booking>>? orderBy = null);
}
