using AutoMapper;
using HotelManagement.BLL.DTOs;
using HotelManagement.BLL.Interfaces;
using HotelManagement.DAL.Entities;
using HotelManagement.Repository.Interfaces;
using HotelManagement.Repository.Models;

namespace HotelManagement.BLL.Services;

public class AmenityService : IAmenityService
{
    private readonly IAmenityRepository _amenityRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public AmenityService(IAmenityRepository amenityRepository, IBookingRepository bookingRepository, IMapper mapper, ICurrentUserService currentUserService)
    {
        _amenityRepository = amenityRepository;
        _bookingRepository = bookingRepository;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<PaginatedResult<AmenityDTO>> GetAllAmenitiesAsync(
            int pageNumber,
            int pageSize,
            string? searchQuery = null,
            string? sortBy = null,
            bool sortDescending = false,
            bool isAvailable = true)
    {
        var pagedResult = await _amenityRepository.GetPaginatedAmenitiesAsync(
            pageNumber,
            pageSize,
            searchQuery,
            sortBy,
            sortDescending,
            isAvailable);

        var dtos = _mapper.Map<IEnumerable<AmenityDTO>>(pagedResult.Data);

        return new PaginatedResult<AmenityDTO>
        {
            TotalCount = pagedResult.TotalCount,
            PageNumber = pagedResult.PageNumber,
            PageSize = pagedResult.PageSize,
            Data = dtos
        };
    }


    public async Task<AmenityDTO?> GetAmenityByIdAsync(int id)
    {
        var amenity = await _amenityRepository.GetByIdAsync(id);
        if (amenity == null) return null;
        return _mapper.Map<AmenityDTO>(amenity);
    }

    public async Task<AmenityDTO> CreateAmenityAsync(CreateUpdateAmenityDTO dto)
    {
        var existing = await _amenityRepository.FindAsync(a => a.Name.ToLower() == dto.Name.ToLower());
        if (existing.Any()) throw new InvalidOperationException($"An amenity with the name '{dto.Name}' already exists in the system.");

        var amenity = new Amenity
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            IsAvailable = true
        };

        await _amenityRepository.AddAsync(amenity);
        await _amenityRepository.SaveChangesAsync();

        return _mapper.Map<AmenityDTO>(amenity);
    }

    public async Task UpdateAmenityAsync(int id, CreateUpdateAmenityDTO dto, bool isAvailable)
    {
        var amenity = await _amenityRepository.GetByIdAsync(id);
        if (amenity == null) throw new KeyNotFoundException("Amenity not found.");

        amenity.Name = dto.Name;
        amenity.Description = dto.Description;
        amenity.Price = dto.Price;
        amenity.IsAvailable = dto.IsAvailable;

        _amenityRepository.Update(amenity);
        await _amenityRepository.SaveChangesAsync();
    }

    // public async Task UpdateAmenityStatusAsync(int id, bool isAvailable)
    // {
    //     var amenity = await _amenityRepository.GetByIdAsync(id);
    //     if (amenity == null) throw new KeyNotFoundException("Amenity not found.");

    //     amenity.IsAvailable = isAvailable;

    //     _amenityRepository.Update(amenity);
    //     await _amenityRepository.SaveChangesAsync();
    // }


    public async Task SubscribeAsync(int bookingId, int amenityId)
    {
        var booking = await _bookingRepository.GetBookingWithDetailsAsync(bookingId);
        if (booking == null) throw new KeyNotFoundException("Booking not found.");

        if (_currentUserService.IsInRole("RegisteredUser") && !_currentUserService.IsInRole("Admin") && !_currentUserService.IsInRole("FrontDesk"))
        {
            if (booking.GuestEmail != _currentUserService.GetUserEmail())
                throw new UnauthorizedAccessException("You can only subscribe to amenities for your own bookings.");
        }

        if (booking.BookingStatus == DAL.Enums.BookingStatus.Cancelled ||
            booking.BookingStatus == DAL.Enums.BookingStatus.CheckedOut)
        {
            throw new InvalidOperationException("Cannot add amenities to a cancelled or checked-out booking.");
        }

        if (booking.PaymentStatus == DAL.Enums.PaymentStatus.Paid)
        {
            throw new InvalidOperationException("Cannot add amenities after payment has been settled.");
        }

        if (booking.BookingAmenities.Any(ba => ba.AmenityId == amenityId))
        {
            throw new InvalidOperationException("This amenity is already added to this booking.");
        }

        var amenity = await _amenityRepository.GetByIdAsync(amenityId);
        if (amenity == null || !amenity.IsAvailable)
        {
            throw new KeyNotFoundException("This amenity is currently unavailable or not found.");
        }

        var bookingAmenity = new BookingAmenity
        {
            BookingId = bookingId,
            AmenityId = amenityId,
            PriceAtPurchase = amenity.Price
        };

        booking.BookingAmenities.Add(bookingAmenity);
        _bookingRepository.Update(booking);
        await _bookingRepository.SaveChangesAsync();
    }
}
