using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using HotelManagement.BLL.DTOs;
using HotelManagement.BLL.Services;
using HotelManagement.DAL.Entities;
using HotelManagement.DAL.Enums;
using HotelManagement.Repository.Interfaces;
using Moq;
using NUnit.Framework;
using HotelManagement.BLL.Interfaces;
using HotelManagement.Repository.Models;

namespace HotelManagement.UnitTesting.Services;

[TestFixture]
public class HousekeepingServiceTests
{
    private Mock<IHousekeepingRepository> _mockHousekeepingRepo;
    private Mock<IMapper> _mockMapper;
    private Mock<ICurrentUserService> _mockCurrentUserService;
    private Mock<IBookingRepository> _mockBookingRepo;
    private Mock<IUserRepository> _mockUserRepo;
    private Mock<INotificationService> _mockNotificationService;
    private Mock<IRoomRepository> _mockRoomRepo;

    private HousekeepingService _housekeepingService;

    [SetUp]
    public void Setup()
    {
        _mockHousekeepingRepo = new Mock<IHousekeepingRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mockBookingRepo = new Mock<IBookingRepository>();
        _mockUserRepo = new Mock<IUserRepository>();
        _mockNotificationService = new Mock<INotificationService>();
        _mockRoomRepo = new Mock<IRoomRepository>();

        _mockRoomRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(new Room { Id = 10, RoomNumber = "101" });

        _housekeepingService = new HousekeepingService(
            _mockHousekeepingRepo.Object,
            _mockMapper.Object,
            _mockCurrentUserService.Object,
            _mockBookingRepo.Object,
            _mockUserRepo.Object,
            _mockNotificationService.Object,
            _mockRoomRepo.Object
        );
    }

    [Test]
    public async Task CreateCheckoutTriggerAsync_ShouldCreateTask()
    {
        await _housekeepingService.CreateCheckoutTriggerAsync(10);

        _mockHousekeepingRepo.Verify(h => h.AddAsync(It.Is<Housekeeping>(task =>
            task.RoomId == 10 &&
            task.OriginType == HousekeepingOriginType.CheckoutAutomated &&
            task.Status == HousekeepingStatus.Pending)), Times.Once);
        _mockHousekeepingRepo.Verify(h => h.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void CreateGuestTriggerAsync_ShouldThrow_IfUserEmailIsNull()
    {
        _mockCurrentUserService.Setup(c => c.IsInRole("RegisteredUser")).Returns(true);
        _mockCurrentUserService.Setup(c => c.IsInRole("Admin")).Returns(false);
        _mockCurrentUserService.Setup(c => c.IsInRole("FrontDesk")).Returns(false);
        _mockCurrentUserService.Setup(c => c.GetUserEmail()).Returns((string?)null);

        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(() => _housekeepingService.CreateGuestTriggerAsync(10, new CreateHousekeepingTaskDTO { Description = "test" }));
        Assert.That(ex.Message, Does.Contain("Must be logged in"));
    }

    [Test]
    public void CreateGuestTriggerAsync_ShouldThrow_IfUserNotFound()
    {
        _mockCurrentUserService.Setup(c => c.IsInRole("RegisteredUser")).Returns(true);
        _mockCurrentUserService.Setup(c => c.GetUserEmail()).Returns("test@example.com");
        _mockUserRepo.Setup(u => u.GetByEmailAsync("test@example.com")).ReturnsAsync((User?)null);

        var ex = Assert.ThrowsAsync<ArgumentException>(() => _housekeepingService.CreateGuestTriggerAsync(10, new CreateHousekeepingTaskDTO { Description = "test" }));
        Assert.That(ex.Message, Does.Contain("User not found"));
    }

    [Test]
    public void CreateGuestTriggerAsync_ShouldThrow_IfNotActiveInRoom()
    {
        _mockCurrentUserService.Setup(c => c.IsInRole("RegisteredUser")).Returns(true);
        _mockCurrentUserService.Setup(c => c.GetUserEmail()).Returns("test@example.com");

        var user = new User { Id = 1 };
        _mockUserRepo.Setup(u => u.GetByEmailAsync("test@example.com")).ReturnsAsync(user);

        var userBookings = new List<Booking>
        {
            new Booking { BookingRooms = new List<BookingRoom> { new BookingRoom { RoomId = 99 } }, BookingStatus = BookingStatus.CheckedIn } // Different room
        };
        var pagedBookings = new PaginatedResult<Booking> { Data = userBookings };
        _mockBookingRepo.Setup(b => b.GetPaginatedBookingsWithDetailsAsync(1, 100, It.IsAny<IEnumerable<Expression<Func<Booking, bool>>>>(), null)).ReturnsAsync(pagedBookings);

        var ex = Assert.ThrowsAsync<ArgumentException>(() => _housekeepingService.CreateGuestTriggerAsync(10, new CreateHousekeepingTaskDTO { Description = "test" }));
        Assert.That(ex.Message, Does.Contain("You do not have an active booking for this room"));
    }

    [Test]
    public async Task CreateGuestTriggerAsync_ShouldCreate_IfActiveInRoom()
    {
        _mockCurrentUserService.Setup(c => c.IsInRole("RegisteredUser")).Returns(true);
        _mockCurrentUserService.Setup(c => c.GetUserEmail()).Returns("test@example.com");

        var user = new User { Id = 1 };
        _mockUserRepo.Setup(u => u.GetByEmailAsync("test@example.com")).ReturnsAsync(user);

        var userBookings = new List<Booking>
        {
            new Booking { BookingRooms = new List<BookingRoom> { new BookingRoom { RoomId = 10 } }, BookingStatus = BookingStatus.CheckedIn } // Correct room
        };
        var pagedBookings = new PaginatedResult<Booking> { Data = userBookings };
        _mockBookingRepo.Setup(b => b.GetPaginatedBookingsWithDetailsAsync(1, 100, It.IsAny<IEnumerable<Expression<Func<Booking, bool>>>>(), null)).ReturnsAsync(pagedBookings);

        await _housekeepingService.CreateGuestTriggerAsync(10, new CreateHousekeepingTaskDTO { Description = "test" });

        _mockHousekeepingRepo.Verify(h => h.AddAsync(It.Is<Housekeeping>(task =>
            task.RoomId == 10 &&
            task.OriginType == HousekeepingOriginType.GuestRequested &&
            task.Status == HousekeepingStatus.Pending)), Times.Once);
        _mockNotificationService.Verify(n => n.SendHousekeepingAlertAsync(It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task CreateGuestTriggerAsync_ShouldCreate_IfAdmin()
    {
        _mockCurrentUserService.Setup(c => c.IsInRole("RegisteredUser")).Returns(true);
        _mockCurrentUserService.Setup(c => c.IsInRole("Admin")).Returns(true);

        await _housekeepingService.CreateGuestTriggerAsync(10, new CreateHousekeepingTaskDTO { Description = "test" });

        _mockHousekeepingRepo.Verify(h => h.AddAsync(It.Is<Housekeeping>(task =>
            task.RoomId == 10 && task.OriginType == HousekeepingOriginType.GuestRequested)), Times.Once);
        _mockNotificationService.Verify(n => n.SendHousekeepingAlertAsync(It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task CreateGuestTriggerAsync_ShouldCreate_IfFrontDesk()
    {
        _mockCurrentUserService.Setup(c => c.IsInRole("RegisteredUser")).Returns(true);
        _mockCurrentUserService.Setup(c => c.IsInRole("Admin")).Returns(false);
        _mockCurrentUserService.Setup(c => c.IsInRole("FrontDesk")).Returns(true);

        await _housekeepingService.CreateGuestTriggerAsync(10, new CreateHousekeepingTaskDTO { Description = "test" });

        _mockHousekeepingRepo.Verify(h => h.AddAsync(It.IsAny<Housekeeping>()), Times.Once);
    }

    [Test]
    public void CreateGuestTriggerAsync_ShouldThrow_IfActiveInRoom_ButStatusNotCheckedIn()
    {
        _mockCurrentUserService.Setup(c => c.IsInRole("RegisteredUser")).Returns(true);
        _mockCurrentUserService.Setup(c => c.GetUserEmail()).Returns("test@example.com");

        var user = new User { Id = 1 };
        _mockUserRepo.Setup(u => u.GetByEmailAsync("test@example.com")).ReturnsAsync(user);

        var userBookings = new List<Booking>
        {
            new Booking { BookingRooms = new List<BookingRoom> { new BookingRoom { RoomId = 10 } }, BookingStatus = BookingStatus.CheckedOut }
        };
        var pagedBookings = new PaginatedResult<Booking> { Data = userBookings };
        _mockBookingRepo.Setup(b => b.GetPaginatedBookingsWithDetailsAsync(1, 100, It.IsAny<IEnumerable<Expression<Func<Booking, bool>>>>(), null)).ReturnsAsync(pagedBookings);

        var ex = Assert.ThrowsAsync<ArgumentException>(() => _housekeepingService.CreateGuestTriggerAsync(10, new CreateHousekeepingTaskDTO { Description = "test" }));
        Assert.That(ex.Message, Does.Contain("You do not have an active booking for this room"));
    }

    [Test]
    public void UpdateStatusAsync_ShouldThrow_IfNotFound()
    {
        _mockHousekeepingRepo.Setup(h => h.GetByIdAsync(1)).ReturnsAsync((Housekeeping?)null);
        Assert.ThrowsAsync<KeyNotFoundException>(() => _housekeepingService.UpdateStatusAsync(1, HousekeepingStatus.InProgress));
    }

    [Test]
    public async Task UpdateStatusAsync_ShouldSetStartedAt_IfInProgress()
    {
        var task = new Housekeeping { Id = 1 };
        _mockHousekeepingRepo.Setup(h => h.GetByIdAsync(1)).ReturnsAsync(task);
        _mockCurrentUserService.Setup(c => c.GetUserEmail()).Returns("test@example.com");
        _mockUserRepo.Setup(u => u.GetByEmailAsync("test@example.com")).ReturnsAsync(new User { Id = 1 });
        _mockHousekeepingRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Housekeeping, bool>>>()))
            .ReturnsAsync(new List<Housekeeping>());

        await _housekeepingService.UpdateStatusAsync(1, HousekeepingStatus.InProgress);

        Assert.That(task.Status, Is.EqualTo(HousekeepingStatus.InProgress));
        Assert.That(task.StartedAt, Is.Not.Null);
        Assert.That(task.FinishedAt, Is.Null);
        _mockHousekeepingRepo.Verify(h => h.Update(task), Times.Once);
    }

    [Test]
    public async Task UpdateStatusAsync_ShouldSetFinishedAt_IfCompleted()
    {
        var task = new Housekeeping { Id = 1, StartedAt = DateTime.UtcNow.AddMinutes(-30), AssignedToUserId = 1 };
        _mockHousekeepingRepo.Setup(h => h.GetByIdAsync(1)).ReturnsAsync(task);
        _mockCurrentUserService.Setup(c => c.GetUserEmail()).Returns("test@example.com");
        _mockUserRepo.Setup(u => u.GetByEmailAsync("test@example.com")).ReturnsAsync(new User { Id = 1 });

        await _housekeepingService.UpdateStatusAsync(1, HousekeepingStatus.Completed);

        Assert.That(task.Status, Is.EqualTo(HousekeepingStatus.Completed));
        Assert.That(task.StartedAt, Is.Not.Null);
        Assert.That(task.FinishedAt, Is.Not.Null);
        _mockHousekeepingRepo.Verify(h => h.Update(task), Times.Once);
    }

    [Test]
    public async Task GetAllAsync_WithSortAndFilter_ShouldReturnFilteredAndSorted()
    {
        var records = new List<Housekeeping> { new Housekeeping { Id = 1, Status = HousekeepingStatus.Pending } };
        _mockHousekeepingRepo.Setup(h => h.GetPaginatedResultAsync(1, 10, It.IsAny<Expression<Func<Housekeeping, bool>>>(), It.IsAny<Func<System.Linq.IQueryable<Housekeeping>, System.Linq.IOrderedQueryable<Housekeeping>>>()))
            .ReturnsAsync((int p1, int p2, Expression<Func<Housekeeping, bool>> filter, Func<System.Linq.IQueryable<Housekeeping>, System.Linq.IOrderedQueryable<Housekeeping>> orderBy) =>
            {
                var q = records.AsQueryable();
                if (filter != null) q = q.Where(filter);
                if (orderBy != null) q = orderBy(q);
                return new PaginatedResult<Housekeeping> { TotalCount = q.Count(), PageNumber = p1, PageSize = p2, Data = q.ToList() };
            });
        _mockCurrentUserService.Setup(s => s.IsInRole("Admin")).Returns(true);
        _mockMapper.Setup(m => m.Map<IEnumerable<HousekeepingDTO>>(It.IsAny<IEnumerable<Housekeeping>>())).Returns(new List<HousekeepingDTO> { new HousekeepingDTO { Id = 1 } });

        var result1 = await _housekeepingService.GetAllAsync(1, 10, null, "Status", true);
        var result2 = await _housekeepingService.GetAllAsync(1, 10, null, "Status", false);

        Assert.That(result1.Data.Count(), Is.EqualTo(1));
        Assert.That(result2.Data.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task GetActiveAsync_WithSort_ShouldReturnSortedActive()
    {
        var records = new List<Housekeeping> { new Housekeeping { Id = 1, Status = HousekeepingStatus.Pending } };
        _mockHousekeepingRepo.Setup(h => h.GetPaginatedResultAsync(1, 10, It.IsAny<Expression<Func<Housekeeping, bool>>>(), It.IsAny<Func<System.Linq.IQueryable<Housekeeping>, System.Linq.IOrderedQueryable<Housekeeping>>>()))
            .ReturnsAsync((int p1, int p2, Expression<Func<Housekeeping, bool>> filter, Func<System.Linq.IQueryable<Housekeeping>, System.Linq.IOrderedQueryable<Housekeeping>> orderBy) =>
            {
                var q = records.AsQueryable();
                if (filter != null) q = q.Where(filter);
                if (orderBy != null) q = orderBy(q);
                return new PaginatedResult<Housekeeping> { TotalCount = q.Count(), PageNumber = p1, PageSize = p2, Data = q.ToList() };
            });
        _mockCurrentUserService.Setup(s => s.IsInRole("Admin")).Returns(true);
        _mockMapper.Setup(m => m.Map<IEnumerable<HousekeepingDTO>>(It.IsAny<IEnumerable<Housekeeping>>())).Returns(new List<HousekeepingDTO> { new HousekeepingDTO { Id = 1 } });

        var result = await _housekeepingService.GetActiveAsync(1, 10, "Status", true);

        Assert.That(result.Data.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task CreateInternalTriggerAsync_ShouldSaveTaskAndSendAlert()
    {
        var dto = new CreateInternalHousekeepingTaskDTO { Location = "Lobby", Description = "Spill" };

        await _housekeepingService.CreateInternalTriggerAsync(dto);

        _mockHousekeepingRepo.Verify(h => h.AddAsync(It.Is<Housekeeping>(t =>
            t.RoomId == null && t.Location == "Lobby" && t.Description == "Spill" && t.OriginType == HousekeepingOriginType.StaffRequested)), Times.Once);
        _mockHousekeepingRepo.Verify(h => h.SaveChangesAsync(), Times.Once);
        _mockNotificationService.Verify(n => n.SendHousekeepingAlertAsync(It.IsAny<string>()), Times.Once);
    }

    [Test]
    public void CreateCheckoutTriggerAsync_ShouldThrow_IfRoomIdInvalid()
    {
        var ex = Assert.ThrowsAsync<ArgumentException>(() => _housekeepingService.CreateCheckoutTriggerAsync(0));
        Assert.That(ex.Message, Does.Contain("valid Room ID"));
    }

    [Test]
    public void CreateCheckoutTriggerAsync_ShouldThrow_IfRoomNotFound()
    {
        _mockRoomRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Room?)null);
        var ex = Assert.ThrowsAsync<ArgumentException>(() => _housekeepingService.CreateCheckoutTriggerAsync(99));
        Assert.That(ex.Message, Does.Contain("does not exist"));
    }

    [Test]
    public void CreateGuestTriggerAsync_ShouldThrow_IfRoomIdInvalid()
    {
        var ex = Assert.ThrowsAsync<ArgumentException>(() => _housekeepingService.CreateGuestTriggerAsync(0, new CreateHousekeepingTaskDTO()));
        Assert.That(ex.Message, Does.Contain("valid Room ID"));
    }

    [Test]
    public void CreateGuestTriggerAsync_ShouldThrow_IfRoomNotFound()
    {
        _mockRoomRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Room?)null);
        var ex = Assert.ThrowsAsync<ArgumentException>(() => _housekeepingService.CreateGuestTriggerAsync(99, new CreateHousekeepingTaskDTO()));
        Assert.That(ex.Message, Does.Contain("does not exist"));
    }

    [Test]
    public async Task GetAllAsync_WithNoSortAndNoFilter_ShouldReturnAll()
    {
        var records = new List<Housekeeping> { new Housekeeping { Id = 1 } };
        var paged = new PaginatedResult<Housekeeping> { TotalCount = 1, PageNumber = 1, PageSize = 10, Data = records };
        _mockHousekeepingRepo.Setup(h => h.GetPaginatedResultAsync(1, 10, null, It.IsAny<Func<IQueryable<Housekeeping>, IOrderedQueryable<Housekeeping>>>()))
            .ReturnsAsync(paged);
        _mockCurrentUserService.Setup(s => s.IsInRole("Admin")).Returns(true);
        _mockMapper.Setup(m => m.Map<IEnumerable<HousekeepingDTO>>(records)).Returns(new List<HousekeepingDTO> { new HousekeepingDTO { Id = 1 } });

        var result = await _housekeepingService.GetAllAsync(1, 10, null, null, false);

        Assert.That(result.Data.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task GetActiveAsync_WithNoSort_ShouldReturnActive()
    {
        var records = new List<Housekeeping> { new Housekeeping { Id = 1 } };
        var paged = new PaginatedResult<Housekeeping> { TotalCount = 1, PageNumber = 1, PageSize = 10, Data = records };
        _mockHousekeepingRepo.Setup(h => h.GetPaginatedResultAsync(1, 10, It.IsAny<Expression<Func<Housekeeping, bool>>>(), It.IsAny<Func<IQueryable<Housekeeping>, IOrderedQueryable<Housekeeping>>>()))
            .ReturnsAsync(paged);
        _mockCurrentUserService.Setup(s => s.IsInRole("Admin")).Returns(true);
        _mockMapper.Setup(m => m.Map<IEnumerable<HousekeepingDTO>>(records)).Returns(new List<HousekeepingDTO> { new HousekeepingDTO { Id = 1 } });

        var result = await _housekeepingService.GetActiveAsync(1, 10, null, false);

        Assert.That(result.Data.Count(), Is.EqualTo(1));
    }

    [Test]
    public void CreateInternalTriggerAsync_ShouldThrow_IfIdenticalTaskPending()
    {
        // Arrange
        var dto = new CreateInternalHousekeepingTaskDTO { Location = "Lobby", Description = "Spill" };

        // Simulate an existing task in the DB with the exact same details
        var existingTasks = new List<Housekeeping> { new Housekeeping { Location = "Lobby", Description = "Spill", Status = HousekeepingStatus.Pending } };
        _mockHousekeepingRepo.Setup(h => h.FindAsync(It.IsAny<Expression<Func<Housekeeping, bool>>>())).ReturnsAsync(existingTasks);

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(() => _housekeepingService.CreateInternalTriggerAsync(dto));
        Assert.That(ex.Message, Does.Contain("identical internal housekeeping task is already pending"));
    }
}
