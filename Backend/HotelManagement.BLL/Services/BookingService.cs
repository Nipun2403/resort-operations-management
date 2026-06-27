using AutoMapper;
using HotelManagement.BLL.DTOs;
using HotelManagement.BLL.Interfaces;
using HotelManagement.DAL.Entities;
using HotelManagement.DAL.Enums;
using HotelManagement.Repository.Interfaces;
using HotelManagement.Repository.Models;
using System.Linq.Expressions;
using HotelManagement.Repository.Utilities;
namespace HotelManagement.BLL.Services;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IHousekeepingService _housekeepingService;
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;
    private readonly IRoomRepository _roomRepository;
    private readonly IRoomTypeRepository _roomTypeRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAmenityRepository _amenityRepository;
    private readonly IBillingService _billingService;

    public BookingService(IBookingRepository bookingRepository, IHousekeepingService housekeepingService, IMapper mapper, INotificationService notificationService, IRoomRepository roomRepository, IRoomTypeRepository roomTypeRepository, IUserRepository userRepository, ICurrentUserService currentUserService, IAmenityRepository amenityRepository, IBillingService billingService)
    {
        _bookingRepository = bookingRepository;
        _housekeepingService = housekeepingService;
        _mapper = mapper;
        _notificationService = notificationService;
        _roomRepository = roomRepository;
        _roomTypeRepository = roomTypeRepository;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _amenityRepository = amenityRepository;
        _billingService = billingService;
    }
    public async Task<BookingDTO> CreateBookingAsync(CreateBookingRequestDTO dto)
    {
        if (dto.RoomTypeIds == null || !dto.RoomTypeIds.Any())
            throw new ArgumentException("At least one RoomTypeId is required.");

        if (dto.CheckInDate.Date < DateTime.UtcNow.Date)
            throw new ArgumentException("Check-in date cannot be in the past.");

        if (dto.CheckOutDate.Date <= dto.CheckInDate.Date)
            throw new ArgumentException("Check-out date must be after check-in date.");

        var emailToCheck = _currentUserService.IsInRole("RegisteredUser") ? _currentUserService.GetUserEmail() : dto.GuestEmail;
        if (!string.IsNullOrEmpty(emailToCheck))
        {
            var cutoffTime = DateTime.UtcNow.AddMinutes(-1);
            var recentBookings = await _bookingRepository.FindAsync(b =>
                b.BookedAt >= cutoffTime &&
                b.GuestEmail == emailToCheck
            );

            if (recentBookings.Any())
            {
                throw new InvalidOperationException("A booking was just created by this email. Please wait a minute before booking again to prevent accidental duplicates.");
            }
        }

        // 1. Pre-flight Validation: Prevent Overbooking
        var availableTypes = await _roomRepository.GetAvailableRoomTypesAsync(dto.CheckInDate, dto.CheckOutDate, 1, 100, null, false);

        int totalCapacity = 0;
        var requestedCounts = dto.RoomTypeIds.GroupBy(id => id).ToDictionary(g => g.Key, g => g.Count());

        var bookingRoomsToCreate = new List<BookingRoom>();

        foreach (var req in requestedCounts)
        {
            var targetType = availableTypes.Data.FirstOrDefault(rt => rt.RoomType.Id == req.Key);
            if (targetType == null || targetType.AvailableCount < req.Value)
            {
                throw new InvalidOperationException($"Room type ID {req.Key} does not have {req.Value} rooms available for the selected dates.");
            }

            for (int i = 0; i < req.Value; i++)
            {
                totalCapacity += targetType.RoomType.MaxOccupancy;
                bookingRoomsToCreate.Add(new BookingRoom
                {
                    RoomTypeId = req.Key,
                    LockedInPrice = targetType.RoomType.BasePrice
                });
            }
        }

        if (dto.GuestCount > totalCapacity)
        {
            throw new ArgumentException($"The number of guests ({dto.GuestCount}) exceeds the maximum capacity ({totalCapacity}) for the selected rooms.");
        }

        var origin = BookingOrigin.Guest;
        var guestEmail = dto.GuestEmail ?? string.Empty;
        var guestName = dto.GuestName ?? string.Empty;
        int? userId = null;

        var isRegisteredUser = _currentUserService.IsInRole("RegisteredUser");
        if (isRegisteredUser)
        {
            origin = BookingOrigin.RegisteredUser;
            guestEmail = _currentUserService.GetUserEmail() ?? string.Empty;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(guestEmail))
                throw new ArgumentException("A valid email address is strictly required for guest bookings.");

            if (string.IsNullOrWhiteSpace(guestName) || guestName.Length < 2)
                throw new ArgumentException("Guest name is required and must be at least 2 characters long.");
        }

        if (origin == BookingOrigin.RegisteredUser && !string.IsNullOrEmpty(guestEmail))
        {
            var user = await _userRepository.GetByEmailAsync(guestEmail);
            if (user != null)
            {
                userId = user.Id;
                if (string.IsNullOrWhiteSpace(guestName))
                {
                    guestName = $"{user.FirstName} {user.LastName}";
                }
            }
        }

        var booking = new Booking
        {
            GuestCount = dto.GuestCount,
            GuestName = guestName,
            GuestEmail = guestEmail,
            CheckInDate = dto.CheckInDate,
            CheckOutDate = dto.CheckOutDate,
            BookingStatus = BookingStatus.Booked,
            UserId = userId,
            Origin = origin,
            BookedAt = DateTime.UtcNow,
            PaymentStatus = PaymentStatus.Pending
        };

        foreach (var br in bookingRoomsToCreate)
        {
            booking.BookingRooms.Add(br);
        }

        // Process Amenities
        if (dto.AmenityIds != null && dto.AmenityIds.Any())
        {
            var uniqueAmenityIds = dto.AmenityIds.Distinct().ToList();
            foreach (var amenityId in uniqueAmenityIds)
            {
                var amenity = await _amenityRepository.GetByIdAsync(amenityId);
                if (amenity != null && amenity.IsAvailable)
                {
                    booking.BookingAmenities.Add(new BookingAmenity
                    {
                        AmenityId = amenityId,
                        PriceAtPurchase = amenity.Price
                    });
                }
            }
        }

        await _bookingRepository.AddAsync(booking);
        await _bookingRepository.SaveChangesAsync();

        return _mapper.Map<BookingDTO>(booking);
    }
    public async Task UpdateBookingStatusAsync(int bookingId, BookingStatus status)
    {
        var booking = await _bookingRepository.GetBookingWithDetailsAsync(bookingId);
        if (booking != null)
        {
            booking.BookingStatus = status;
            _bookingRepository.Update(booking);
            await _bookingRepository.SaveChangesAsync();

            if (status == BookingStatus.CheckedOut && booking.BookingRooms != null)
            {
                foreach (var br in booking.BookingRooms)
                {
                    if (br.RoomId.HasValue)
                    {
                        await _housekeepingService.CreateCheckoutTriggerAsync(br.RoomId.Value);
                        if (br.Room != null)
                        {
                            await _notificationService.SendHousekeepingAlertAsync($"Room {br.Room.RoomNumber} has checked out. Please prepare for cleaning.");
                        }
                    }
                }
            }
        }
    }

    public async Task<PaginatedResult<BookingDTO>> GetBookingsAsync(string? status, string? guestQuery, int pageNumber, int pageSize, string? sortBy = null, bool sortDescending = false)
    {
        var isStaff = _currentUserService.IsInRole("Admin") || _currentUserService.IsInRole("FrontDesk");
        var predicates = new List<Expression<Func<Booking, bool>>>();

        if (!isStaff)
        {
            var userEmail = _currentUserService.GetUserEmail();
            if (string.IsNullOrEmpty(userEmail))
                throw new UnauthorizedAccessException("You must be logged in to view your bookings.");

            var user = await _userRepository.GetByEmailAsync(userEmail);
            if (user == null) throw new ArgumentException("User not found.");

            predicates.Add(b => b.UserId == user.Id);
        }
        else if (status != null)
        {
            if (status.Equals("active", StringComparison.OrdinalIgnoreCase))
            {
                predicates.Add(b => b.BookingStatus == BookingStatus.CheckedIn || b.BookingStatus == BookingStatus.Booked);
            }
            else if (Enum.TryParse<BookingStatus>(status, true, out var parsedStatus))
            {
                predicates.Add(b => b.BookingStatus == parsedStatus);
            }
        }

        if (!string.IsNullOrWhiteSpace(guestQuery))
        {
            var lowerQuery = guestQuery.ToLower();
            predicates.Add(b => b.GuestName.ToLower().Contains(lowerQuery) || b.GuestEmail.ToLower().Contains(lowerQuery) || b.Id.ToString().Contains(lowerQuery));
        }

        Func<IQueryable<Booking>, IOrderedQueryable<Booking>>? orderBy = null;
        if (!string.IsNullOrEmpty(sortBy))
        {
            orderBy = q => q.OrderByDynamic(sortBy, sortDescending);
        }

        // var pagedBookings = await _bookingRepository.GetPaginatedResultAsync(pageNumber, pageSize, predicate, orderBy);
        var pagedBookings = await _bookingRepository.GetPaginatedBookingsWithDetailsAsync(pageNumber, pageSize, predicates, orderBy);

        var dtos = _mapper.Map<IEnumerable<BookingDTO>>(pagedBookings.Data);

        return new PaginatedResult<BookingDTO>
        {
            TotalCount = pagedBookings.TotalCount,
            PageNumber = pagedBookings.PageNumber,
            PageSize = pagedBookings.PageSize,
            Data = dtos
        };
    }

    public async Task<BookingDTO?> GetBookingByIdAsync(int id)
    {
        // var booking = await _bookingRepository.GetByIdAsync(id);
        var booking = await _bookingRepository.GetBookingWithDetailsAsync(id);
        if (booking == null) return null;
        return _mapper.Map<BookingDTO>(booking);
    }

    public async Task CancelBookingAsync(int id)
    {
        var booking = await _bookingRepository.GetByIdAsync(id);
        if (booking == null) throw new ArgumentException("Booking not found.");

        if (booking.BookingStatus == BookingStatus.Cancelled)
            throw new InvalidOperationException("Booking is already cancelled.");

        if (booking.BookingStatus != BookingStatus.Booked)
            throw new InvalidOperationException("Only upcoming bookings can be cancelled.");

        var isStaff = _currentUserService.IsInRole("Admin") || _currentUserService.IsInRole("FrontDesk");

        if (!isStaff)
        {
            var userEmail = _currentUserService.GetUserEmail();
            if (string.IsNullOrEmpty(userEmail)) throw new UnauthorizedAccessException("Must be logged in to cancel.");
            var user = await _userRepository.GetByEmailAsync(userEmail);
            if (user == null || booking.UserId != user.Id) throw new UnauthorizedAccessException("You do not have permission to cancel this booking.");
        }

        if (booking.PaymentStatus == PaymentStatus.Paid)
        {
            booking.PaymentStatus = PaymentStatus.Refunded;
        }

        booking.BookingStatus = BookingStatus.Cancelled;
        _bookingRepository.Update(booking);
        await _bookingRepository.SaveChangesAsync();
    }

    public async Task<BookingDTO> CheckInGuestAsync(int bookingId)
    {
        var booking = await _bookingRepository.GetBookingWithDetailsAsync(bookingId);
        if (booking == null) throw new ArgumentException("Booking not found.");
        if (booking.BookingStatus != BookingStatus.Booked)
            throw new InvalidOperationException("Only 'Booked' bookings can be checked in.");

        var today = DateTime.UtcNow.Date;
        if (booking.CheckInDate.Date != today)
        {
            throw new InvalidOperationException($"Check-in denied. Guests can only check in on their exact arrival date. This booking is scheduled for {booking.CheckInDate:dd-MM-yyyy}.");
        }

        var conflictingBookings = await _bookingRepository.FindAsync(b =>
            b.Id != bookingId &&
            b.BookingStatus != BookingStatus.CheckedOut &&
            b.BookingStatus != BookingStatus.Cancelled &&
            b.CheckInDate < booking.CheckOutDate &&
            b.CheckOutDate > booking.CheckInDate);

        var occupiedRoomIds = conflictingBookings
            .SelectMany(b => b.BookingRooms)
            .Where(br => br.RoomId.HasValue)
            .Select(br => br.RoomId!.Value)
            .ToHashSet();

        foreach (var br in booking.BookingRooms)
        {
            if (!br.RoomId.HasValue)
            {
                var allRooms = await _roomRepository.FindAsync(r => r.RoomTypeId == br.RoomTypeId && r.IsActive);
                var availableRoom = allRooms
                    .Where(r => !occupiedRoomIds.Contains(r.Id))
                    .OrderBy(r => r.RoomNumber)
                    .FirstOrDefault();

                if (availableRoom == null)
                    throw new InvalidOperationException($"No active rooms of type ID {br.RoomTypeId} are currently available for auto-assignment.");

                br.RoomId = availableRoom.Id;
                occupiedRoomIds.Add(availableRoom.Id); // Mark as occupied for next loop iteration
            }
            else
            {
                var specificRoom = await _roomRepository.GetByIdAsync(br.RoomId.Value);
                if (specificRoom == null || !specificRoom.IsActive)
                    throw new ArgumentException("Specific room not found or is retired.");
                if (specificRoom.RoomTypeId != br.RoomTypeId)
                    throw new ArgumentException($"Room {specificRoom.RoomNumber} does not match the booked Room Type ID {br.RoomTypeId}.");
                if (occupiedRoomIds.Contains(specificRoom.Id))
                    throw new InvalidOperationException($"Room {specificRoom.RoomNumber} is already occupied or reserved.");

                occupiedRoomIds.Add(specificRoom.Id);
            }
        }

        booking.BookingStatus = BookingStatus.CheckedIn;

        _bookingRepository.Update(booking);
        await _bookingRepository.SaveChangesAsync();

        return _mapper.Map<BookingDTO>(booking);
    }

    public async Task<BillingFolioDTO> UnifiedCheckoutAsync(int bookingId)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId);
        if (booking == null) throw new ArgumentException("Booking not found.");

        if (booking.BookingStatus == BookingStatus.CheckedOut)
            throw new InvalidOperationException("Booking is already checked out.");

        if (booking.PaymentStatus != HotelManagement.DAL.Enums.PaymentStatus.Paid)
            throw new InvalidOperationException("Cannot check out guest. The bill has not been settled.");

        // 1. Mark as CheckedOut (triggers SignalR Housekeeping)
        await UpdateBookingStatusAsync(bookingId, BookingStatus.CheckedOut);

        // 2. Generate and return the final Folio instantly
        return await _billingService.GenerateFolioAsync(bookingId);
    }

    public async Task ExtendStayAsync(int bookingId, DateTime newCheckOutDate)
    {
        var booking = await _bookingRepository.GetBookingWithDetailsAsync(bookingId);
        if (booking == null) throw new ArgumentException("Booking not found.");

        if (booking.BookingStatus != BookingStatus.CheckedIn)
            throw new InvalidOperationException("Only guests who have already checked in can extend their stay.");

        // Rule: Extension must be in the future relative to current checkout
        if (newCheckOutDate <= booking.CheckOutDate)
            throw new ArgumentException("New checkout date must be after current checkout date.");

        // Rule: Logical sanity check
        if (newCheckOutDate <= booking.CheckInDate)
            throw new ArgumentException("Checkout date cannot be before check-in date.");

        var myRoomIds = booking.BookingRooms.Where(br => br.RoomId.HasValue).Select(br => br.RoomId!.Value).ToList();

        var conflicting = await _bookingRepository.FindAsync(b =>
            b.Id != bookingId &&
            b.BookingStatus != BookingStatus.CheckedOut &&
            b.BookingRooms.Any(br => br.RoomId.HasValue && myRoomIds.Contains(br.RoomId.Value)) &&
            b.CheckInDate < newCheckOutDate &&
            b.CheckOutDate > booking.CheckOutDate);

        if (conflicting.Any())
            throw new ArgumentException("Cannot extend stay. Room is reserved by another guest.");

        booking.CheckOutDate = newCheckOutDate;
        _bookingRepository.Update(booking);
        await _bookingRepository.SaveChangesAsync();
    }
}
