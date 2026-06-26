using HotelManagement.BLL.DTOs;
using HotelManagement.DAL.Enums;
using HotelManagement.Repository.Models;
namespace HotelManagement.BLL.Interfaces;
public interface IBookingService
{
    Task<BookingDTO> CreateBookingAsync(CreateBookingRequestDTO requestDto);
    Task UpdateBookingStatusAsync(int bookingId, BookingStatus status);
    Task<PaginatedResult<BookingDTO>> GetBookingsAsync(string? status, string? guestQuery, int pageNumber, int pageSize, string? sortBy = null, bool sortDescending = false);
    Task<BookingDTO?> GetBookingByIdAsync(int id);
    Task CancelBookingAsync(int id);
    
    // FrontDesk actions migrated to BookingService
    Task<BookingDTO> CheckInGuestAsync(int bookingId);
    Task<BillingFolioDTO> UnifiedCheckoutAsync(int bookingId);
    Task ExtendStayAsync(int bookingId, DateTime newCheckOutDate);
}
