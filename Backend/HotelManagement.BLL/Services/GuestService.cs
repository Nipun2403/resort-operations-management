using AutoMapper;
using HotelManagement.BLL.DTOs;
using HotelManagement.BLL.Interfaces;
using HotelManagement.DAL.Enums;
using HotelManagement.Repository.Interfaces;
using HotelManagement.Repository.Models;

namespace HotelManagement.BLL.Services;

public class GuestService : IGuestService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IMapper _mapper;

    public GuestService(IBookingRepository bookingRepository, IMapper mapper)
    {
        _bookingRepository = bookingRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<GuestSearchDTO>> SearchGuestsAsync(string? query, int pageNumber, int pageSize)
    {
        var allBookings = await _bookingRepository.GetAllAsync();

        var filteredBookings = string.IsNullOrWhiteSpace(query)
            ? allBookings
            : allBookings.Where(b => b.GuestName.ToLower().Contains(query.ToLower()) || b.GuestEmail.ToLower().Contains(query.ToLower()));

        var grouped = filteredBookings.GroupBy(b => b.GuestEmail).ToList();
        var totalCount = grouped.Count;

        var guests = grouped
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(g => new GuestSearchDTO
            {
                GuestName = g.First().GuestName,
                GuestEmail = g.Key,
                TotalStays = g.Count(b => b.BookingStatus == BookingStatus.CheckedOut),
                LastCheckInDate = g.Max(b => (DateTime?)b.CheckInDate)
            })
            .ToList();

        return new PaginatedResult<GuestSearchDTO>
        {
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            Data = guests
        };
    }

    public async Task<PaginatedResult<BookingDTO>> GetIncomingGuestsAsync(int pageNumber, int pageSize)
    {
        var today = DateTime.UtcNow.Date;
        var pagedBookings = await _bookingRepository.GetPaginatedResultAsync(pageNumber, pageSize,
            b => b.CheckInDate.Date == today && b.BookingStatus == BookingStatus.Booked);

        var dtos = _mapper.Map<IEnumerable<BookingDTO>>(pagedBookings.Data);
        return new PaginatedResult<BookingDTO>
        {
            TotalCount = pagedBookings.TotalCount,
            PageNumber = pagedBookings.PageNumber,
            PageSize = pagedBookings.PageSize,
            Data = dtos
        };
    }
}
