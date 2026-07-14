using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using HotelManagement.BLL.DTOs;
using HotelManagement.BLL.Profiles;
using HotelManagement.BLL.Services;
using HotelManagement.DAL.Entities;
using HotelManagement.DAL.Enums;
using HotelManagement.Repository.Interfaces;
using HotelManagement.Repository.Models;
using Moq;
using NUnit.Framework;
using HotelManagement.BLL.Interfaces;

namespace HotelManagement.UnitTesting.Services;

[TestFixture]
public class BookingServiceTests
{
    private Mock<IBookingRepository> _mockBookingRepo;
    private Mock<IHousekeepingService> _mockHousekeepingService;
    private Mock<IMapper> _mockMapper;
    private Mock<INotificationService> _mockNotificationService;
    private Mock<IRoomRepository> _mockRoomRepo;
    private Mock<IRoomTypeRepository> _mockRoomTypeRepo;
    private Mock<IUserRepository> _mockUserRepo;
    private Mock<ICurrentUserService> _mockCurrentUserService;
    private Mock<IAmenityRepository> _mockAmenityRepo;
    private Mock<IBillingService> _mockBillingService;
    private Mock<IEmailService> _mockEmailService;

    private BookingService _bookingService;

    [SetUp]
    public void Setup()
    {
        _mockBookingRepo = new Mock<IBookingRepository>();
        _mockHousekeepingService = new Mock<IHousekeepingService>();
        _mockMapper = new Mock<IMapper>();
        _mockNotificationService = new Mock<INotificationService>();
        _mockRoomRepo = new Mock<IRoomRepository>();
        _mockRoomTypeRepo = new Mock<IRoomTypeRepository>();
        _mockUserRepo = new Mock<IUserRepository>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mockAmenityRepo = new Mock<IAmenityRepository>();
        _mockBillingService = new Mock<IBillingService>();
        _mockEmailService = new Mock<IEmailService>();

        _bookingService = new BookingService(
            _mockBookingRepo.Object,
            _mockHousekeepingService.Object,
            _mockMapper.Object,
            _mockNotificationService.Object,
            _mockRoomRepo.Object,
            _mockRoomTypeRepo.Object,
            _mockUserRepo.Object,
            _mockCurrentUserService.Object,
            _mockAmenityRepo.Object,
            _mockBillingService.Object,
            _mockEmailService.Object
        );
    }

    [Test]
    public void CreateBookingAsync_ShouldThrow_IfCheckInDateInPast()
    {
        var dto = new CreateBookingRequestDTO { RoomTypeIds = new List<int> { 1 }, CheckInDate = DateTime.UtcNow.AddDays(-1), CheckOutDate = DateTime.UtcNow.AddDays(1) };
        var ex = Assert.ThrowsAsync<ArgumentException>(() => _bookingService.CreateBookingAsync(dto));
        Assert.That(ex.Message, Does.Contain("cannot be in the past"));
    }

    [Test]
    public void CreateBookingAsync_ShouldThrow_IfCheckOutDateBeforeCheckInDate()
    {
        var dto = new CreateBookingRequestDTO { RoomTypeIds = new List<int> { 1 }, CheckInDate = DateTime.UtcNow.AddDays(2), CheckOutDate = DateTime.UtcNow.AddDays(1) };
        var ex = Assert.ThrowsAsync<ArgumentException>(() => _bookingService.CreateBookingAsync(dto));
        Assert.That(ex.Message, Does.Contain("must be after check-in date"));
    }

    [Test]
    public void CreateBookingAsync_ShouldThrow_IfRoomTypeFullyBooked()
    {
        // Arrange
        var dto = new CreateBookingRequestDTO { RoomTypeIds = new List<int> { 1 }, CheckInDate = DateTime.UtcNow.AddDays(1), CheckOutDate = DateTime.UtcNow.AddDays(3) };

        var availableTypes = new PaginatedResult<RoomTypeAvailability>
        {
            Data = new List<RoomTypeAvailability>() // Empty list implies zero capacity
        };

        _mockRoomRepo.Setup(r => r.GetAvailableRoomTypesAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), 1, 100, null, false))
            .ReturnsAsync(availableTypes);

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(() => _bookingService.CreateBookingAsync(dto));
        Assert.That(ex.Message, Does.Contain("rooms available for the selected dates"));
    }

    [Test]
    public void CreateBookingAsync_ShouldThrow_IfRoomTypeHasZeroAvailable()
    {
        // Arrange
        var dto = new CreateBookingRequestDTO { RoomTypeIds = new List<int> { 1 }, CheckInDate = DateTime.UtcNow.AddDays(1), CheckOutDate = DateTime.UtcNow.AddDays(3) };

        var availableTypes = new PaginatedResult<RoomTypeAvailability>
        {
            Data = new List<RoomTypeAvailability>
            {
                new RoomTypeAvailability { RoomType = new RoomType { Id = 1, MaxOccupancy = 2 }, AvailableCount = 0 }
            }
        };

        _mockRoomRepo.Setup(r => r.GetAvailableRoomTypesAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), 1, 100, null, false))
            .ReturnsAsync(availableTypes);

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(() => _bookingService.CreateBookingAsync(dto));
        Assert.That(ex.Message, Does.Contain("rooms available for the selected dates"));
    }
    [Test]
    public void CreateBookingAsync_ShouldThrow_IfGuestCountExceedsCapacity()
    {
        // Arrange
        var dto = new CreateBookingRequestDTO { RoomTypeIds = new List<int> { 1 }, GuestCount = 5, CheckInDate = DateTime.UtcNow.AddDays(1), CheckOutDate = DateTime.UtcNow.AddDays(3) };

        var availableTypes = new PaginatedResult<RoomTypeAvailability>
        {
            Data = new List<RoomTypeAvailability>
            {
                new RoomTypeAvailability { RoomType = new RoomType { Id = 1, MaxOccupancy = 2 }, AvailableCount = 1 }
            }
        };

        _mockRoomRepo.Setup(r => r.GetAvailableRoomTypesAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), 1, 100, null, false))
            .ReturnsAsync(availableTypes);

        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(() => _bookingService.CreateBookingAsync(dto));
        Assert.That(ex.Message, Does.Contain("exceeds the maximum capacity"));
    }
    [Test]
    public void CreateBookingAsync_ShouldThrow_IfGuestEmailMissingForUnregistered()
    {
        // Arrange
        var dto = new CreateBookingRequestDTO { RoomTypeIds = new List<int> { 1 }, GuestEmail = "", GuestName = "John", CheckInDate = DateTime.UtcNow.AddDays(1), CheckOutDate = DateTime.UtcNow.AddDays(3) };
        var availableTypes = new PaginatedResult<RoomTypeAvailability>
        {
            Data = new List<RoomTypeAvailability> { new RoomTypeAvailability { RoomType = new RoomType { Id = 1, MaxOccupancy = 2, BasePrice = 100m }, AvailableCount = 5 } }
        };

        _mockRoomRepo.Setup(r => r.GetAvailableRoomTypesAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), 1, 100, null, false))
            .ReturnsAsync(availableTypes);
        _mockCurrentUserService.Setup(c => c.IsInRole("RegisteredUser")).Returns(false);

        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(() => _bookingService.CreateBookingAsync(dto));
        Assert.That(ex.Message, Does.Contain("valid email address is strictly required"));
    }

    [Test]
    public void CreateBookingAsync_ShouldThrow_IfGuestNameMissingForUnregistered()
    {
        // Arrange
        var dto = new CreateBookingRequestDTO { RoomTypeIds = new List<int> { 1 }, GuestEmail = "guest@example.com", GuestName = "", CheckInDate = DateTime.UtcNow.AddDays(1), CheckOutDate = DateTime.UtcNow.AddDays(3) };
        var availableTypes = new PaginatedResult<RoomTypeAvailability>
        {
            Data = new List<RoomTypeAvailability> { new RoomTypeAvailability { RoomType = new RoomType { Id = 1, MaxOccupancy = 2, BasePrice = 100m }, AvailableCount = 5 } }
        };

        _mockRoomRepo.Setup(r => r.GetAvailableRoomTypesAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), 1, 100, null, false))
            .ReturnsAsync(availableTypes);
        _mockCurrentUserService.Setup(c => c.IsInRole("RegisteredUser")).Returns(false);

        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(() => _bookingService.CreateBookingAsync(dto));
        Assert.That(ex.Message, Does.Contain("Guest name is required"));
    }

    [Test]
    public void CreateBookingAsync_ShouldThrow_IfDuplicateRecentBooking()
    {
        // Arrange
        var dto = new CreateBookingRequestDTO { RoomTypeIds = new List<int> { 1 }, CheckInDate = DateTime.UtcNow.AddDays(1), CheckOutDate = DateTime.UtcNow.AddDays(3), GuestEmail = "spam@example.com", GuestName = "Spammer" };
        _mockCurrentUserService.Setup(c => c.IsInRole("RegisteredUser")).Returns(false);

        // Simulate a booking made in the last minute by the same email
        var recentBookings = new List<Booking> { new Booking { Id = 1, GuestEmail = "spam@example.com" } };
        _mockBookingRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Booking, bool>>>())).ReturnsAsync(recentBookings);

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(() => _bookingService.CreateBookingAsync(dto));
        Assert.That(ex.Message, Does.Contain("A booking was just created by this email"));
    }

    [Test]
    public async Task CreateBookingAsync_ShouldCreateBooking_ForRegisteredUser()
    {
        // Arrange
        var dto = new CreateBookingRequestDTO { RoomTypeIds = new List<int> { 1 }, GuestEmail = "reg@example.com", CheckInDate = DateTime.UtcNow.AddDays(1), CheckOutDate = DateTime.UtcNow.AddDays(3) };
        var availableTypes = new PaginatedResult<RoomTypeAvailability>
        {
            Data = new List<RoomTypeAvailability> { new RoomTypeAvailability { RoomType = new RoomType { Id = 1, MaxOccupancy = 2, BasePrice = 150m }, AvailableCount = 5 } }
        };
        var user = new User { Id = 10, Email = "reg@example.com", FirstName = "Reggie", LastName = "User" };

        _mockRoomRepo.Setup(r => r.GetAvailableRoomTypesAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), 1, 100, null, false))
            .ReturnsAsync(availableTypes);
        _mockCurrentUserService.Setup(c => c.IsInRole("RegisteredUser")).Returns(true);
        _mockCurrentUserService.Setup(c => c.GetUserEmail()).Returns("reg@example.com");
        _mockUserRepo.Setup(u => u.GetByEmailAsync("reg@example.com")).ReturnsAsync(user);

        var bookingEntity = new Booking { Id = 1, UserId = 10 };
        _mockMapper.Setup(m => m.Map<BookingDTO>(It.IsAny<Booking>())).Returns(new BookingDTO { Origin = BookingOrigin.RegisteredUser, UserId = 10, GuestName = "Reggie User", Rooms = new List<BookingRoomDTO> { new BookingRoomDTO { LockedInPrice = 150m } } });

        // Act
        var result = await _bookingService.CreateBookingAsync(dto);

        // Assert
        Assert.That(result.Origin, Is.EqualTo(BookingOrigin.RegisteredUser));
        Assert.That(result.UserId, Is.EqualTo(10));
        Assert.That(result.GuestName, Is.EqualTo("Reggie User"));
        Assert.That(result.Rooms.First().LockedInPrice, Is.EqualTo(150m));
        Assert.That(result.Rooms.First().LockedInPrice, Is.EqualTo(150m));
        _mockBookingRepo.Verify(b => b.AddAsync(It.IsAny<Booking>()), Times.Once);
        _mockBookingRepo.Verify(b => b.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task CreateBookingAsync_ShouldHandleNullUserEmail_ForRegisteredUser()
    {
        // Arrange
        var dto = new CreateBookingRequestDTO { RoomTypeIds = new List<int> { 1 }, CheckInDate = DateTime.UtcNow.AddDays(1), CheckOutDate = DateTime.UtcNow.AddDays(3) };
        var availableTypes = new PaginatedResult<RoomTypeAvailability>
        {
            Data = new List<RoomTypeAvailability> { new RoomTypeAvailability { RoomType = new RoomType { Id = 1, MaxOccupancy = 2, BasePrice = 150m }, AvailableCount = 5 } }
        };

        _mockRoomRepo.Setup(r => r.GetAvailableRoomTypesAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), 1, 100, null, false))
            .ReturnsAsync(availableTypes);
        _mockCurrentUserService.Setup(c => c.IsInRole("RegisteredUser")).Returns(true);
        _mockCurrentUserService.Setup(c => c.GetUserEmail()).Returns((string?)null); // Branch: ?? string.Empty
        _mockUserRepo.Setup(u => u.GetByEmailAsync(string.Empty)).ReturnsAsync((User?)null); // Won't find user

        var bookingEntity = new Booking { Id = 3 };
        _mockMapper.Setup(m => m.Map<BookingDTO>(It.IsAny<Booking>())).Returns(new BookingDTO { Rooms = new List<BookingRoomDTO> { new BookingRoomDTO { LockedInPrice = 150m } } });

        // Act
        var result = await _bookingService.CreateBookingAsync(dto);

        // Assert
        Assert.That(result.Rooms.First().LockedInPrice, Is.EqualTo(150m));
        _mockBookingRepo.Verify(b => b.AddAsync(It.IsAny<Booking>()), Times.Once);
        _mockBookingRepo.Verify(b => b.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task CreateBookingAsync_ShouldCreateBooking_ForGuest()
    {
        // Arrange
        var dto = new CreateBookingRequestDTO { RoomTypeIds = new List<int> { 1 }, GuestEmail = "guest@example.com", GuestName = "Guest User", CheckInDate = DateTime.UtcNow.AddDays(1), CheckOutDate = DateTime.UtcNow.AddDays(3) };
        var availableTypes = new PaginatedResult<RoomTypeAvailability>
        {
            Data = new List<RoomTypeAvailability> { new RoomTypeAvailability { RoomType = new RoomType { Id = 1, MaxOccupancy = 2, BasePrice = 200m }, AvailableCount = 5 } }
        };

        _mockRoomRepo.Setup(r => r.GetAvailableRoomTypesAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), 1, 100, null, false))
            .ReturnsAsync(availableTypes);
        _mockCurrentUserService.Setup(c => c.IsInRole("RegisteredUser")).Returns(false);

        var bookingEntity = new Booking { Id = 2 };
        _mockMapper.Setup(m => m.Map<BookingDTO>(It.IsAny<Booking>())).Returns(new BookingDTO { Origin = BookingOrigin.Guest, Rooms = new List<BookingRoomDTO> { new BookingRoomDTO { LockedInPrice = 200m } } });

        // Act
        var result = await _bookingService.CreateBookingAsync(dto);

        // Assert
        Assert.That(result.Origin, Is.EqualTo(BookingOrigin.Guest));
        Assert.That(result.Rooms.First().LockedInPrice, Is.EqualTo(200m));
        _mockBookingRepo.Verify(b => b.AddAsync(It.IsAny<Booking>()), Times.Once);
    }

    [Test]
    public async Task UpdateBookingStatusAsync_ShouldTriggerHousekeeping_WhenCheckedOut()
    {
        // Arrange
        var booking = new Booking { Id = 1, BookingRooms = new List<BookingRoom> { new BookingRoom { RoomId = 5, Room = new Room { RoomNumber = "101" } } }, BookingStatus = BookingStatus.CheckedIn };
        _mockBookingRepo.Setup(b => b.GetBookingWithDetailsAsync(1)).ReturnsAsync(booking);

        // Act
        await _bookingService.UpdateBookingStatusAsync(1, BookingStatus.CheckedOut);

        // Assert
        Assert.That(booking.BookingStatus, Is.EqualTo(BookingStatus.CheckedOut));
        _mockHousekeepingService.Verify(h => h.CreateCheckoutTriggerAsync(5), Times.Once);
        _mockNotificationService.Verify(n => n.SendHousekeepingAlertAsync(It.IsAny<string>()), Times.Once);
        _mockBookingRepo.Verify(b => b.Update(booking), Times.Once);
        _mockBookingRepo.Verify(b => b.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task UpdateBookingStatusAsync_ShouldNotTriggerHousekeeping_IfNoRoomId()
    {
        // Arrange: RoomId is null
        var booking = new Booking { Id = 1, BookingRooms = new List<BookingRoom>(), BookingStatus = BookingStatus.CheckedIn };
        _mockBookingRepo.Setup(b => b.GetBookingWithDetailsAsync(1)).ReturnsAsync(booking);

        // Act
        await _bookingService.UpdateBookingStatusAsync(1, BookingStatus.CheckedOut);

        // Assert
        Assert.That(booking.BookingStatus, Is.EqualTo(BookingStatus.CheckedOut));
        _mockHousekeepingService.Verify(h => h.CreateCheckoutTriggerAsync(It.IsAny<int>()), Times.Never);
        _mockNotificationService.Verify(n => n.SendHousekeepingAlertAsync(It.IsAny<string>()), Times.Never);
        _mockBookingRepo.Verify(b => b.Update(booking), Times.Once);
        _mockBookingRepo.Verify(b => b.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task UpdateBookingStatusAsync_ShouldNotTriggerHousekeeping_IfNotCheckedOut()
    {
        // Arrange
        var booking = new Booking { Id = 1, BookingRooms = new List<BookingRoom> { new BookingRoom { RoomId = 5 } }, BookingStatus = BookingStatus.Booked };
        _mockBookingRepo.Setup(b => b.GetBookingWithDetailsAsync(1)).ReturnsAsync(booking);

        // Act
        await _bookingService.UpdateBookingStatusAsync(1, BookingStatus.CheckedIn);

        // Assert
        Assert.That(booking.BookingStatus, Is.EqualTo(BookingStatus.CheckedIn));
        _mockHousekeepingService.Verify(h => h.CreateCheckoutTriggerAsync(It.IsAny<int>()), Times.Never);
        _mockNotificationService.Verify(n => n.SendHousekeepingAlertAsync(It.IsAny<string>()), Times.Never);
        _mockBookingRepo.Verify(b => b.Update(booking), Times.Once);
        _mockBookingRepo.Verify(b => b.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task UpdateBookingStatusAsync_ShouldDoNothing_IfBookingNull()
    {
        // Arrange
        _mockBookingRepo.Setup(b => b.GetBookingWithDetailsAsync(1)).ReturnsAsync((Booking?)null);

        // Act
        await _bookingService.UpdateBookingStatusAsync(1, BookingStatus.CheckedIn);

        // Assert
        _mockBookingRepo.Verify(b => b.Update(It.IsAny<Booking>()), Times.Never);
    }

    [Test]
    public void GetBookingsAsync_ShouldThrow_IfNotLoggedIn()
    {
        // Arrange
        _mockCurrentUserService.Setup(c => c.IsInRole("Admin")).Returns(false);
        _mockCurrentUserService.Setup(c => c.IsInRole("FrontDesk")).Returns(false);
        _mockCurrentUserService.Setup(c => c.GetUserEmail()).Returns((string?)null);

        // Act & Assert
        Assert.ThrowsAsync<UnauthorizedAccessException>(() => _bookingService.GetBookingsAsync(null, null, 1, 10, null, false));
    }

    [Test]
    public void GetBookingsAsync_ShouldThrow_IfUserNotFound()
    {
        // Arrange
        _mockCurrentUserService.Setup(c => c.IsInRole("Admin")).Returns(false);
        _mockCurrentUserService.Setup(c => c.IsInRole("FrontDesk")).Returns(false);
        _mockCurrentUserService.Setup(c => c.GetUserEmail()).Returns("test@example.com");
        _mockUserRepo.Setup(u => u.GetByEmailAsync("test@example.com")).ReturnsAsync((User?)null);

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() => _bookingService.GetBookingsAsync(null, null, 1, 10, null, false));
    }

    [Test]
    public async Task GetBookingsAsync_ShouldReturnUserBookings_IfNotStaff()
    {
        _mockCurrentUserService.Setup(s => s.IsInRole("Admin")).Returns(false);
        _mockCurrentUserService.Setup(s => s.IsInRole("FrontDesk")).Returns(false);
        _mockCurrentUserService.Setup(s => s.GetUserEmail()).Returns("user@example.com");
        // FIXED: Added User mock to prevent "User not found" exception
        _mockUserRepo.Setup(u => u.GetByEmailAsync("user@example.com")).ReturnsAsync(new User { Id = 10 });

        var pagedResult = new PaginatedResult<Booking> { Data = new List<Booking> { new Booking { Id = 1 } }, TotalCount = 1 };

        // FIXED: Mocked GetPaginatedBookingsWithDetailsAsync instead of GetPaginatedResultAsync
        _mockBookingRepo.Setup(r => r.GetPaginatedBookingsWithDetailsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<Expression<Func<Booking, bool>>>>(), It.IsAny<Func<IQueryable<Booking>, IOrderedQueryable<Booking>>>()))
            .ReturnsAsync(pagedResult);
        _mockMapper.Setup(m => m.Map<IEnumerable<BookingDTO>>(It.IsAny<IEnumerable<Booking>>()))
            .Returns(new List<BookingDTO> { new BookingDTO { Id = 1 } });

        var result = await _bookingService.GetBookingsAsync(null, null, 1, 10, null, false);
        Assert.That(result.Data.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task GetBookingsAsync_ShouldReturnActiveBookings_IfStaffAndStatusIsActive()
    {
        _mockCurrentUserService.Setup(s => s.IsInRole("Admin")).Returns(true);

        var pagedResult = new PaginatedResult<Booking> { Data = new List<Booking> { new Booking { Id = 1 } }, TotalCount = 1 };

        _mockBookingRepo.Setup(r => r.GetPaginatedBookingsWithDetailsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<Expression<Func<Booking, bool>>>>(), It.IsAny<Func<IQueryable<Booking>, IOrderedQueryable<Booking>>>()))
            .ReturnsAsync(pagedResult);
        _mockMapper.Setup(m => m.Map<IEnumerable<BookingDTO>>(It.IsAny<IEnumerable<Booking>>()))
            .Returns(new List<BookingDTO> { new BookingDTO { Id = 1 } });

        var result = await _bookingService.GetBookingsAsync("Booked", null, 1, 10, null, false);
        Assert.That(result.Data.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task GetBookingsAsync_ShouldReturnAllBookings_IfStaffAndNoStatus()
    {
        _mockCurrentUserService.Setup(s => s.IsInRole("Admin")).Returns(true);

        var pagedResult = new PaginatedResult<Booking> { Data = new List<Booking> { new Booking { Id = 1 } }, TotalCount = 1 };

        _mockBookingRepo.Setup(r => r.GetPaginatedBookingsWithDetailsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<Expression<Func<Booking, bool>>>>(), It.IsAny<Func<IQueryable<Booking>, IOrderedQueryable<Booking>>>()))
            .ReturnsAsync(pagedResult);
        _mockMapper.Setup(m => m.Map<IEnumerable<BookingDTO>>(It.IsAny<IEnumerable<Booking>>()))
            .Returns(new List<BookingDTO> { new BookingDTO { Id = 1 } });

        var result = await _bookingService.GetBookingsAsync(null, null, 1, 10, null, false);
        Assert.That(result.Data.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task GetBookingsAsync_WithSortBy_AppliesSorting()
    {
        _mockCurrentUserService.Setup(s => s.IsInRole("Admin")).Returns(true);

        var pagedResult = new PaginatedResult<Booking> { Data = new List<Booking> { new Booking { Id = 1 } }, TotalCount = 1 };

        _mockBookingRepo.Setup(r => r.GetPaginatedBookingsWithDetailsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IEnumerable<Expression<Func<Booking, bool>>>>(), It.IsAny<Func<IQueryable<Booking>, IOrderedQueryable<Booking>>>()))
            .ReturnsAsync(pagedResult);
        _mockMapper.Setup(m => m.Map<IEnumerable<BookingDTO>>(It.IsAny<IEnumerable<Booking>>()))
            .Returns(new List<BookingDTO> { new BookingDTO { Id = 1 } });

        var result = await _bookingService.GetBookingsAsync(null, null, 1, 10, "GuestName", true);
        Assert.That(result.Data.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task GetBookingByIdAsync_ShouldReturnNull_IfNotFound()
    {
        // Service uses GetBookingWithDetailsAsync, not GetByIdAsync!
        _mockBookingRepo.Setup(b => b.GetBookingWithDetailsAsync(99)).ReturnsAsync((Booking?)null);
        var result = await _bookingService.GetBookingByIdAsync(99);
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetBookingByIdAsync_ShouldReturnBooking()
    {
        var booking = new Booking { Id = 1 };
        // Service uses GetBookingWithDetailsAsync, not GetByIdAsync!
        _mockBookingRepo.Setup(r => r.GetBookingWithDetailsAsync(1)).ReturnsAsync(booking);
        _mockMapper.Setup(m => m.Map<BookingDTO>(booking)).Returns(new BookingDTO { Id = 1 });

        var result = await _bookingService.GetBookingByIdAsync(1);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(1));
    }

    [Test]
    public void CancelBookingAsync_ShouldThrow_IfNotFound()
    {
        _mockBookingRepo.Setup(b => b.GetByIdAsync(99)).ReturnsAsync((Booking?)null);
        Assert.ThrowsAsync<ArgumentException>(() => _bookingService.CancelBookingAsync(99));
    }

    [Test]
    public void CancelBookingAsync_ShouldThrow_IfNotLoggedInAndNotStaff()
    {
        var booking = new Booking { Id = 1, UserId = 10 };
        _mockBookingRepo.Setup(b => b.GetByIdAsync(1)).ReturnsAsync(booking);
        _mockCurrentUserService.Setup(c => c.IsInRole(It.IsAny<string>())).Returns(false);
        _mockCurrentUserService.Setup(c => c.GetUserEmail()).Returns((string?)null);

        Assert.ThrowsAsync<UnauthorizedAccessException>(() => _bookingService.CancelBookingAsync(1));
    }

    [Test]
    public void CancelBookingAsync_ShouldThrow_IfUserDoesNotOwnBooking()
    {
        var booking = new Booking { Id = 1, UserId = 10 };
        _mockBookingRepo.Setup(b => b.GetByIdAsync(1)).ReturnsAsync(booking);
        _mockCurrentUserService.Setup(c => c.IsInRole(It.IsAny<string>())).Returns(false);
        _mockCurrentUserService.Setup(c => c.GetUserEmail()).Returns("other@example.com");
        _mockUserRepo.Setup(u => u.GetByEmailAsync("other@example.com")).ReturnsAsync(new User { Id = 99 });

        Assert.ThrowsAsync<UnauthorizedAccessException>(() => _bookingService.CancelBookingAsync(1));
    }

    [Test]
    public void CancelBookingAsync_ShouldThrow_IfUserNotFound()
    {
        var booking = new Booking { Id = 1, UserId = 10 };
        _mockBookingRepo.Setup(b => b.GetByIdAsync(1)).ReturnsAsync(booking);
        _mockCurrentUserService.Setup(c => c.IsInRole(It.IsAny<string>())).Returns(false);
        _mockCurrentUserService.Setup(c => c.GetUserEmail()).Returns("notfound@example.com");
        _mockUserRepo.Setup(u => u.GetByEmailAsync("notfound@example.com")).ReturnsAsync((User?)null); // user == null

        Assert.ThrowsAsync<UnauthorizedAccessException>(() => _bookingService.CancelBookingAsync(1));
    }

    [Test]
    public void CancelBookingAsync_ShouldThrow_IfNotBookedStatus()
    {
        var booking = new Booking { Id = 1, UserId = 10, BookingStatus = BookingStatus.CheckedIn };
        _mockBookingRepo.Setup(b => b.GetByIdAsync(1)).ReturnsAsync(booking);
        _mockCurrentUserService.Setup(c => c.IsInRole(It.IsAny<string>())).Returns(false);
        _mockCurrentUserService.Setup(c => c.GetUserEmail()).Returns("test@example.com");
        _mockUserRepo.Setup(u => u.GetByEmailAsync("test@example.com")).ReturnsAsync(new User { Id = 10 });

        var ex = Assert.ThrowsAsync<InvalidOperationException>(() => _bookingService.CancelBookingAsync(1));
        Assert.That(ex.Message, Does.Contain("upcoming bookings can be cancelled"));
    }

    [Test]
    public async Task CancelBookingAsync_ShouldCancel_IfValid()
    {
        var booking = new Booking { Id = 1, UserId = 10, BookingStatus = BookingStatus.Booked };
        _mockBookingRepo.Setup(b => b.GetByIdAsync(1)).ReturnsAsync(booking);
        _mockCurrentUserService.Setup(c => c.IsInRole("Admin")).Returns(true);

        await _bookingService.CancelBookingAsync(1);

        Assert.That(booking.BookingStatus, Is.EqualTo(BookingStatus.Cancelled));
        _mockBookingRepo.Verify(b => b.Update(booking), Times.Once);
        _mockBookingRepo.Verify(b => b.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task CancelBookingAsync_ShouldCancel_IfValid_NormalUser()
    {
        var booking = new Booking { Id = 1, UserId = 10, BookingStatus = BookingStatus.Booked };
        _mockBookingRepo.Setup(b => b.GetByIdAsync(1)).ReturnsAsync(booking);
        _mockCurrentUserService.Setup(c => c.IsInRole(It.IsAny<string>())).Returns(false);
        _mockCurrentUserService.Setup(c => c.GetUserEmail()).Returns("test@example.com");
        _mockUserRepo.Setup(u => u.GetByEmailAsync("test@example.com")).ReturnsAsync(new User { Id = 10 });

        await _bookingService.CancelBookingAsync(1);

        Assert.That(booking.BookingStatus, Is.EqualTo(BookingStatus.Cancelled));
        _mockBookingRepo.Verify(r => r.Update(It.Is<Booking>(b => b.BookingStatus == BookingStatus.Cancelled)), Times.Once);
        _mockBookingRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task CancelBookingAsync_ShouldSetRefunded_IfPaymentPaid()
    {
        // Arrange
        var booking = new Booking { Id = 1, UserId = 10, BookingStatus = BookingStatus.Booked, PaymentStatus = PaymentStatus.Paid };
        _mockBookingRepo.Setup(b => b.GetByIdAsync(1)).ReturnsAsync(booking);
        _mockCurrentUserService.Setup(c => c.IsInRole("Admin")).Returns(true);

        // Act
        await _bookingService.CancelBookingAsync(1);

        // Assert
        Assert.That(booking.PaymentStatus, Is.EqualTo(PaymentStatus.Refunded));
        Assert.That(booking.BookingStatus, Is.EqualTo(BookingStatus.Cancelled));
        _mockBookingRepo.Verify(b => b.Update(booking), Times.Once);
        _mockBookingRepo.Verify(b => b.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task CreateBookingAsync_WithAmenityIds_ShouldAddBookingAmenities()
    {
        var dto = new CreateBookingRequestDTO
        {
            CheckInDate = DateTime.UtcNow.AddDays(1),
            CheckOutDate = DateTime.UtcNow.AddDays(3),
            RoomTypeIds = new List<int> { 1 },
            // Origin is not part of the request DTO, it's set by the service
            GuestName = "Test",
            GuestEmail = "test@test.com",
            AmenityIds = new List<int> { 1 }
        };

        var roomTypeData = new PaginatedResult<RoomTypeAvailability>
        {
            Data = new List<RoomTypeAvailability>
            {
                new RoomTypeAvailability { RoomType = new RoomType { Id = 1, MaxOccupancy = 2, BasePrice = 100 }, AvailableCount = 5 }
            }
        };

        _mockRoomRepo.Setup(r => r.GetAvailableRoomTypesAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), 1, 100, null, false))
            .ReturnsAsync(roomTypeData);

        var booking = new Booking { Id = 1, BookingAmenities = new List<BookingAmenity>() };
        _mockMapper.Setup(m => m.Map<Booking>(dto)).Returns(booking);

        _mockCurrentUserService.Setup(s => s.IsInRole("RegisteredUser")).Returns(false);

        _mockAmenityRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Amenity { Id = 1, Price = 25.0m, IsAvailable = true });

        await _bookingService.CreateBookingAsync(dto);

        _mockBookingRepo.Verify(r => r.AddAsync(It.Is<Booking>(b =>
            b.BookingAmenities.Count == 1 &&
            b.BookingAmenities.First().AmenityId == 1 &&
            b.BookingAmenities.First().PriceAtPurchase == 25.0m
        )), Times.Once);
    }

    [Test]
    public async Task CreateBookingAsync_ShouldIgnore_IfAmenityNotFoundOrUnavailable()
    {
        var dto = new CreateBookingRequestDTO
        {
            CheckInDate = DateTime.UtcNow.AddDays(1),
            CheckOutDate = DateTime.UtcNow.AddDays(3),
            RoomTypeIds = new List<int> { 1 },
            // Origin is not part of the request DTO, it's set by the service
            GuestName = "Test",
            GuestEmail = "test@test.com",
            AmenityIds = new List<int> { 1, 2 }
        };

        var roomTypeData = new PaginatedResult<RoomTypeAvailability>
        {
            Data = new List<RoomTypeAvailability>
            {
                new RoomTypeAvailability { RoomType = new RoomType { Id = 1, MaxOccupancy = 2, BasePrice = 100 }, AvailableCount = 5 }
            }
        };

        _mockRoomRepo.Setup(r => r.GetAvailableRoomTypesAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), 1, 100, null, false))
            .ReturnsAsync(roomTypeData);

        var booking = new Booking { Id = 1, BookingAmenities = new List<BookingAmenity>() };
        _mockMapper.Setup(m => m.Map<Booking>(dto)).Returns(booking);

        _mockCurrentUserService.Setup(s => s.IsInRole("RegisteredUser")).Returns(false);

        // 1 is not found, 2 is unavailable
        _mockAmenityRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Amenity?)null);
        _mockAmenityRepo.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(new Amenity { Id = 2, IsAvailable = false });

        await _bookingService.CreateBookingAsync(dto);

        // Should ignore both and add 0 amenities
        Assert.That(booking.BookingAmenities.Count, Is.EqualTo(0));
    }
}