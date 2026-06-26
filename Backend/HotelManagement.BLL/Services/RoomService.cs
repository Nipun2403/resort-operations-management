using AutoMapper;
using HotelManagement.BLL.DTOs;
using HotelManagement.BLL.Interfaces;
using HotelManagement.DAL.Entities;
using HotelManagement.Repository.Interfaces;
using HotelManagement.Repository.Models;
using HotelManagement.Repository.Utilities;

namespace HotelManagement.BLL.Services;

public class RoomService : IRoomService
{
    private readonly IRoomRepository _roomRepository;
    private readonly IRoomTypeRepository _roomTypeRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IMapper _mapper;

    public RoomService(IRoomRepository roomRepository, IRoomTypeRepository roomTypeRepository, IBookingRepository bookingRepository, IMapper mapper)
    {
        _roomRepository = roomRepository;
        _roomTypeRepository = roomTypeRepository;
        _bookingRepository = bookingRepository;
        _mapper = mapper;
    }


    public async Task<PaginatedResult<RoomDTO>> GetRoomsAsync(int pageNumber, int pageSize, int? roomTypeId = null, bool includeRetired = false, string? sortBy = null, bool sortDescending = false)
    {
        var rooms = await _roomRepository.GetRoomsWithTypesAsync(includeRetired);
        var query = rooms.AsQueryable();

        if (roomTypeId.HasValue)
        {
            query = query.Where(r => r.RoomTypeId == roomTypeId.Value);
        }

        if (!string.IsNullOrEmpty(sortBy))
        {
            query = query.OrderByDynamic(sortBy, sortDescending);
        }

        var totalCount = query.Count();
        var pagedRooms = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);

        var dtos = _mapper.Map<IEnumerable<RoomDTO>>(pagedRooms);
        return new PaginatedResult<RoomDTO>
        {
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            Data = dtos
        };
    }

    public async Task<RoomDTO> CreateRoomAsync(CreateUpdateRoomDTO dto)
    {
        var existing = await _roomRepository.FindAsync(r => r.RoomNumber.ToLower() == dto.RoomNumber.ToLower());
        if (existing.Any()) throw new InvalidOperationException($"A room with the number '{dto.RoomNumber}' already exists in the system.");

        var roomType = await _roomTypeRepository.GetByIdAsync(dto.RoomTypeId);
        if (roomType == null || !roomType.IsActive) throw new ArgumentException("Invalid or inactive RoomTypeId.");

        var room = new Room
        {
            RoomNumber = dto.RoomNumber,
            RoomTypeId = dto.RoomTypeId,
            IsActive = true
        };

        await _roomRepository.AddAsync(room);
        await _roomRepository.SaveChangesAsync();

        // Map back to DTO, injecting the RoomTypeName manually since EF might not load the nav property immediately on insert
        var response = _mapper.Map<RoomDTO>(room);
        response.RoomTypeName = roomType.Name;
        return response;
    }

    public async Task<RoomDTO> UpdateRoomAsync(int id, CreateUpdateRoomDTO dto)
    {
        var existingRoom = await _roomRepository.GetByIdAsync(id);
        if (existingRoom == null || !existingRoom.IsActive) throw new ArgumentException("Room not found or inactive.");

        var roomType = await _roomTypeRepository.GetByIdAsync(dto.RoomTypeId);
        if (roomType == null || !roomType.IsActive) throw new ArgumentException("Invalid or inactive RoomTypeId.");

        existingRoom.RoomNumber = dto.RoomNumber;
        existingRoom.RoomTypeId = dto.RoomTypeId;

        if (dto.IsActive.HasValue)
        {
            existingRoom.IsActive = dto.IsActive.Value;
        }

        _roomRepository.Update(existingRoom);
        await _roomRepository.SaveChangesAsync();

        var response = _mapper.Map<RoomDTO>(existingRoom);
        response.RoomTypeName = roomType.Name;
        return response;
    }

    public async Task DeleteRoomAsync(int id)
    {
        var existingRoom = await _roomRepository.GetByIdAsync(id);
        if (existingRoom == null) throw new ArgumentException("Room not found.");

        existingRoom.IsActive = false;
        _roomRepository.Update(existingRoom);
        await _roomRepository.SaveChangesAsync();
    }

    public async Task<PaginatedResult<RoomStatusDashboardDTO>> GetRoomStatusDashboardAsync(int pageNumber, int pageSize, int? roomTypeId = null, string? sortBy = null, bool sortDescending = false)
    {
        var allRoomsQuery = await _roomRepository.GetRoomsWithTypesAsync();
        var allRooms = allRoomsQuery.AsQueryable();

        if (roomTypeId.HasValue)
        {
            allRooms = allRooms.Where(r => r.RoomTypeId == roomTypeId.Value);
        }

        var activeBookings = await _bookingRepository.GetActiveBookingsAsync();

        var dashboard = new List<RoomStatusDashboardDTO>();

        foreach (var room in allRooms)
        {
            var dto = new RoomStatusDashboardDTO
            {
                RoomId = room.Id,
                RoomNumber = room.RoomNumber,
                RoomTypeName = room.RoomType.Name
            };

            var currentBooking = activeBookings.FirstOrDefault(b => b.BookingRooms.Any(br => br.RoomId == room.Id) && b.BookingStatus == HotelManagement.DAL.Enums.BookingStatus.CheckedIn);
            if (currentBooking != null)
            {
                dto.Status = "Occupied";
                dto.CurrentGuestName = currentBooking.GuestName;
            }
            else
            {
                var nextBooking = activeBookings
                    .Where(b => b.BookingRooms.Any(br => br.RoomId == room.Id) && b.BookingStatus == HotelManagement.DAL.Enums.BookingStatus.Booked)
                    .OrderBy(b => b.CheckInDate)
                    .FirstOrDefault();

                if (nextBooking != null)
                {
                    dto.Status = "Reserved";
                    dto.NextCheckInDate = nextBooking.CheckInDate;
                }
                else
                {
                    dto.Status = "Available";
                }
            }

            dashboard.Add(dto);
        }

        var query = dashboard.AsQueryable();
        if (!string.IsNullOrEmpty(sortBy))
        {
            query = query.OrderByDynamic(sortBy, sortDescending);
        }

        var totalCount = query.Count();
        var pagedDashboard = query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        return new PaginatedResult<RoomStatusDashboardDTO>
        {
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            Data = pagedDashboard
        };
    }

    public async Task<IEnumerable<RoomDTO>> GetAvailableRoomsForCheckInAsync(int bookingId)
    {
        var booking = await _bookingRepository.GetBookingWithDetailsAsync(bookingId);
        if (booking == null) throw new ArgumentException("Booking not found.");

        var roomTypeIds = booking.BookingRooms.Select(br => br.RoomTypeId).Distinct().ToList();
        var allRooms = await _roomRepository.FindAsync(r => roomTypeIds.Contains(r.RoomTypeId) && r.IsActive);

        var conflictingBookings = await _bookingRepository.FindAsync(b =>
            b.Id != bookingId &&
            b.BookingStatus != HotelManagement.DAL.Enums.BookingStatus.CheckedOut &&
            b.BookingStatus != HotelManagement.DAL.Enums.BookingStatus.Cancelled &&
            b.CheckInDate < booking.CheckOutDate &&
            b.CheckOutDate > booking.CheckInDate);

        var occupiedRoomIds = conflictingBookings.SelectMany(b => b.BookingRooms).Where(br => br.RoomId.HasValue).Select(br => br.RoomId!.Value).ToHashSet();

        var availableRooms = allRooms
            .Where(r => !occupiedRoomIds.Contains(r.Id))
            .OrderBy(r => r.RoomNumber);

        return _mapper.Map<IEnumerable<RoomDTO>>(availableRooms);
    }
}
