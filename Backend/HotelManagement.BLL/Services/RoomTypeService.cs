using AutoMapper;
using HotelManagement.BLL.DTOs;
using HotelManagement.BLL.Interfaces;
using HotelManagement.DAL.Entities;
using HotelManagement.Repository.Interfaces;
using HotelManagement.Repository.Models;
using HotelManagement.Repository.Utilities;
using System.Text.Json;

namespace HotelManagement.BLL.Services;

public class RoomTypeService : IRoomTypeService
{
    private readonly IRoomTypeRepository _roomTypeRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    private readonly IRoomRepository _roomRepository;

    public RoomTypeService(IRoomTypeRepository roomTypeRepository, IBookingRepository bookingRepository, IMapper mapper, ICurrentUserService currentUserService, IRoomRepository roomRepository)
    {
        _roomTypeRepository = roomTypeRepository;
        _bookingRepository = bookingRepository;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _roomRepository = roomRepository;
    }

    public async Task<PaginatedResult<RoomTypeDTO>> GetRoomTypesAsync(int pageNumber, int pageSize, bool includeRetired = false, string? searchQuery = null, string? sortBy = null, bool sortDescending = false)
    {

        if (includeRetired && !_currentUserService.IsInRole("Admin"))
        {
            includeRetired = false;
        }

        var types = await _roomTypeRepository.GetRoomTypesAsync(includeRetired);
        var query = types.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            var lowerQuery = searchQuery.ToLower();
            query = query.Where(rt =>
                rt.Name.ToLower().Contains(lowerQuery) ||
                (rt.Description != null && rt.Description.ToLower().Contains(lowerQuery))
            );
        }

        // Apply Dynamic Sorting
        if (!string.IsNullOrEmpty(sortBy))
        {
            query = query.OrderByDynamic(sortBy, sortDescending);
        }

        var totalCount = query.Count();
        var pagedTypes = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);

        var dtos = _mapper.Map<IEnumerable<RoomTypeDTO>>(pagedTypes);
        return new PaginatedResult<RoomTypeDTO>
        {
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            Data = dtos
        };
    }

    public async Task<PaginatedResult<RoomTypeAvailabilityDTO>> GetAvailableRoomTypesAsync(DateTime checkIn, DateTime checkOut, int pageNumber, int pageSize, string? sortBy, bool descending)
    {
        var result = await _roomRepository.GetAvailableRoomTypesAsync(checkIn, checkOut, pageNumber, pageSize, sortBy, descending);

        var mappedData = result.Data.Select(rta => new RoomTypeAvailabilityDTO
        {
            RoomTypeId = rta.RoomType.Id,
            Name = rta.RoomType.Name,
            BasePrice = rta.RoomType.BasePrice,
            MaxOccupancy = rta.RoomType.MaxOccupancy,
            Description = rta.RoomType.Description,
            ImageUrls = rta.RoomType.ImageUrls,
            SquareFootage = rta.RoomType.SquareFootage,
            BedConfiguration = string.IsNullOrWhiteSpace(rta.RoomType.BedConfigurationJson)
                ? null
                : JsonSerializer.Deserialize<Dictionary<string, int>>(rta.RoomType.BedConfigurationJson, (JsonSerializerOptions?)null),
            AvailableCount = rta.AvailableCount
        });

        return new PaginatedResult<RoomTypeAvailabilityDTO>
        {
            TotalCount = result.TotalCount,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            Data = mappedData
        };
    }

    public async Task<RoomTypeDTO> CreateRoomTypeAsync(CreateRoomTypeDTO dto)
    {
        if (dto.BasePrice < 0) throw new ArgumentException("BasePrice cannot be negative.");
        if (dto.MaxOccupancy < 1) throw new ArgumentException("MaxOccupancy must be at least 1.");

        var existing = await _roomTypeRepository.FindAsync(r => r.Name.ToLower() == dto.Name.ToLower());
        if (existing.Any()) throw new InvalidOperationException($"A room type with the name '{dto.Name}' already exists in the system.");

        var roomType = new RoomType
        {
            Name = dto.Name,
            BasePrice = dto.BasePrice,
            MaxOccupancy = dto.MaxOccupancy,
            Description = dto.Description,
            ImageUrls = dto.ImageUrls,
            SquareFootage = dto.SquareFootage,
            BedConfigurationJson = dto.BedConfiguration == null
                ? null
                : JsonSerializer.Serialize(dto.BedConfiguration, (JsonSerializerOptions?)null),
            IsActive = true
        };

        await _roomTypeRepository.AddAsync(roomType);
        await _roomTypeRepository.SaveChangesAsync();

        return _mapper.Map<RoomTypeDTO>(roomType);
    }

    public async Task<RoomTypeDTO> UpdateRoomTypeAsync(int id, UpdateRoomTypeDTO dto)
    {
        if (dto.BasePrice.HasValue && dto.BasePrice.Value < 0) throw new ArgumentException("BasePrice cannot be negative.");
        if (dto.MaxOccupancy.HasValue && dto.MaxOccupancy.Value < 1) throw new ArgumentException("MaxOccupancy must be at least 1.");

        var existingType = await _roomTypeRepository.GetByIdAsync(id);
        // if (existingType == null || !existingType.IsActive) throw new ArgumentException("RoomType not found or inactive.");
        if (existingType == null) throw new ArgumentException("RoomType not found or inactive.");

        if (!string.IsNullOrEmpty(dto.Name)) existingType.Name = dto.Name;
        if (dto.BasePrice.HasValue) existingType.BasePrice = dto.BasePrice.Value;
        if (dto.MaxOccupancy.HasValue) existingType.MaxOccupancy = dto.MaxOccupancy.Value;
        if (dto.Description != null) existingType.Description = dto.Description;
        if (dto.ImageUrls != null) existingType.ImageUrls = dto.ImageUrls;
        if (dto.SquareFootage.HasValue) existingType.SquareFootage = dto.SquareFootage.Value;
        if (dto.BedConfiguration != null)
            existingType.BedConfigurationJson = JsonSerializer.Serialize(dto.BedConfiguration, (JsonSerializerOptions?)null);
        if (dto.IsActive.HasValue) existingType.IsActive = dto.IsActive.Value;


        _roomTypeRepository.Update(existingType);
        await _roomTypeRepository.SaveChangesAsync();

        return _mapper.Map<RoomTypeDTO>(existingType);
    }

    public async Task DeleteRoomTypeAsync(int id)
    {
        var existingType = await _roomTypeRepository.GetByIdAsync(id);
        if (existingType == null) throw new ArgumentException("RoomType not found.");

        // Critical BLL Validation: Ensure no active bookings rely on this room type!
        var activeBookings = await _bookingRepository.FindAsync(b =>
            b.BookingRooms.Any(br => br.RoomTypeId == id) && b.BookingStatus != DAL.Enums.BookingStatus.CheckedOut && b.BookingStatus != DAL.Enums.BookingStatus.Cancelled);

        if (activeBookings.Any())
        {
            throw new InvalidOperationException("Cannot delete RoomType because there are active bookings associated with it.");
        }

        existingType.IsActive = false;
        _roomTypeRepository.Update(existingType);
        await _roomTypeRepository.SaveChangesAsync();
    }
}
