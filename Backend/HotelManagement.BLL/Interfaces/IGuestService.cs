using HotelManagement.BLL.DTOs;
using HotelManagement.Repository.Models;

namespace HotelManagement.BLL.Interfaces;

public interface IGuestService
{
    Task<PaginatedResult<GuestSearchDTO>> SearchGuestsAsync(string? query, int pageNumber, int pageSize);
    Task<PaginatedResult<BookingDTO>> GetIncomingGuestsAsync(int pageNumber, int pageSize);
}
