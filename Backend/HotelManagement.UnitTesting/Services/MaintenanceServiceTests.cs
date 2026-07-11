using AutoMapper;
using HotelManagement.BLL.DTOs;
using HotelManagement.BLL.Interfaces;
using HotelManagement.BLL.Profiles;
using HotelManagement.BLL.Services;
using HotelManagement.DAL.Entities;
using HotelManagement.DAL.Enums;
using HotelManagement.Repository.Interfaces;
using Moq;
using NUnit.Framework;
using System.Linq.Expressions;
using HotelManagement.Repository.Models;

namespace HotelManagement.UnitTesting.Services;

[TestFixture]
public class MaintenanceServiceTests
{
    private Mock<IMaintenanceRepository> _mockMaintenanceRepo;
    private Mock<IMapper> _mockMapper;
    private Mock<ICurrentUserService> _mockCurrentUserService;
    private Mock<IBookingRepository> _mockBookingRepo;
    private Mock<IUserRepository> _mockUserRepo;
    private Mock<INotificationService> _mockNotificationService;
    private Mock<IRoomRepository> _mockRoomRepo;

    private MaintenanceService _maintenanceService;

    [SetUp]
    public void Setup()
    {
        _mockMaintenanceRepo = new Mock<IMaintenanceRepository>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mockBookingRepo = new Mock<IBookingRepository>();
        _mockUserRepo = new Mock<IUserRepository>();
        _mockNotificationService = new Mock<INotificationService>();
        _mockMapper = new Mock<IMapper>();
        _mockRoomRepo = new Mock<IRoomRepository>();

        _mockRoomRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(new Room { Id = 10, RoomNumber = "101" });

        _maintenanceService = new MaintenanceService(
            _mockMaintenanceRepo.Object,
            _mockMapper.Object,
            _mockCurrentUserService.Object,
            _mockBookingRepo.Object,
            _mockUserRepo.Object,
            _mockNotificationService.Object,
            _mockRoomRepo.Object
        );
    }

    [Test]
    public async Task GetActiveTasksAsync_ReturnsOnlyNonCompletedTasks()
    {
        // Arrange
        var tasks = new List<MaintenanceTask>
        {
            new MaintenanceTask { Id = 1, Status = MaintenanceStatus.Pending },
            new MaintenanceTask { Id = 2, Status = MaintenanceStatus.InProgress }
        };
        var paged = new PaginatedResult<MaintenanceTask> { TotalCount = 2, PageNumber = 1, PageSize = 10, Data = tasks };
        _mockMaintenanceRepo.Setup(r => r.GetPaginatedResultAsync(1, 10, It.IsAny<Expression<Func<MaintenanceTask, bool>>>()))
            .ReturnsAsync(paged);

        var dtos = new List<MaintenanceTaskDTO>
        {
            new MaintenanceTaskDTO { Id = 1 },
            new MaintenanceTaskDTO { Id = 2 }
        };
        _mockMapper.Setup(m => m.Map<IEnumerable<MaintenanceTaskDTO>>(tasks)).Returns(dtos);

        // Act
        var result = await _maintenanceService.GetActiveTasksAsync(1, 10);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Data.Count(), Is.EqualTo(2));
    }

    [Test]
    public void CreateTicketAsync_InvalidDescription_ThrowsArgumentException()
    {
        // Arrange
        var dto = new CreateMaintenanceTaskDTO { Description = "123456" };

        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(() => _maintenanceService.CreateTicketAsync(1, dto));
        Assert.That(ex.Message, Does.Contain("alphabet"));
    }

    [Test]
    public async Task CreateTicketAsync_OriginOverride_UsesOverride()
    {
        // Arrange
        var dto = new CreateMaintenanceTaskDTO { Description = "Fix TV" };
        MaintenanceTask? savedTask = null;

        _mockMaintenanceRepo.Setup(r => r.AddAsync(It.IsAny<MaintenanceTask>()))
            .Callback<MaintenanceTask>(t => savedTask = t);

        // Act
        var result = await _maintenanceService.CreateTicketAsync(1, dto, "StaffRequested");

        // Assert
        Assert.That(savedTask, Is.Not.Null);
        Assert.That(savedTask!.OriginType, Is.EqualTo(MaintenanceOriginType.StaffRequested));
        _mockNotificationService.Verify(n => n.SendMaintenanceAlertAsync(It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task CreateTicketAsync_OriginOverride_Invalid_FallsBackToRoles()
    {
        // Arrange
        var dto = new CreateMaintenanceTaskDTO { Description = "Fix TV" };
        _mockCurrentUserService.Setup(s => s.IsInRole("Admin")).Returns(true);
        MaintenanceTask? savedTask = null;

        _mockMaintenanceRepo.Setup(r => r.AddAsync(It.IsAny<MaintenanceTask>()))
            .Callback<MaintenanceTask>(t => savedTask = t);

        // Act
        var result = await _maintenanceService.CreateTicketAsync(1, dto, "InvalidOverride123");

        // Assert
        Assert.That(savedTask, Is.Not.Null);
        Assert.That(savedTask!.OriginType, Is.EqualTo(MaintenanceOriginType.StaffRequested));
    }

    [Test]
    public async Task CreateTicketAsync_StaffRole_SetsStaffRequested()
    {
        // Arrange
        var dto = new CreateMaintenanceTaskDTO { Description = "Fix TV" };
        _mockCurrentUserService.Setup(s => s.IsInRole("Admin")).Returns(true);

        MaintenanceTask? savedTask = null;
        _mockMaintenanceRepo.Setup(r => r.AddAsync(It.IsAny<MaintenanceTask>()))
            .Callback<MaintenanceTask>(t => savedTask = t);

        // Act
        var result = await _maintenanceService.CreateTicketAsync(1, dto);

        // Assert
        Assert.That(savedTask, Is.Not.Null);
        Assert.That(savedTask!.OriginType, Is.EqualTo(MaintenanceOriginType.StaffRequested));
    }

    [Test]
    public async Task CreateTicketAsync_StaffFlow_Housekeeping_ShouldSucceed()
    {
        var dto = new CreateMaintenanceTaskDTO { Description = "Fix TV" };
        _mockCurrentUserService.Setup(s => s.IsInRole("Admin")).Returns(false);
        _mockCurrentUserService.Setup(s => s.IsInRole("FrontDesk")).Returns(false);
        _mockCurrentUserService.Setup(s => s.IsInRole("Housekeeping")).Returns(true);

        MaintenanceTask? savedTask = null;
        _mockMaintenanceRepo.Setup(r => r.AddAsync(It.IsAny<MaintenanceTask>()))
            .Callback<MaintenanceTask>(t => savedTask = t);

        await _maintenanceService.CreateTicketAsync(1, dto);

        Assert.That(savedTask!.OriginType, Is.EqualTo(MaintenanceOriginType.StaffRequested));
    }

    [Test]
    public void CreateTicketAsync_GuestFlow_EmailNull_ThrowsUnauthorized()
    {
        // Arrange
        var dto = new CreateMaintenanceTaskDTO { Description = "Fix TV" };
        _mockCurrentUserService.Setup(s => s.IsInRole("Admin")).Returns(false);
        _mockCurrentUserService.Setup(s => s.IsInRole("FrontDesk")).Returns(false);
        _mockCurrentUserService.Setup(s => s.IsInRole("Housekeeping")).Returns(false);
        _mockCurrentUserService.Setup(s => s.IsInRole("RegisteredUser")).Returns(true);
        _mockCurrentUserService.Setup(s => s.GetUserEmail()).Returns((string?)null);

        // Act & Assert
        Assert.ThrowsAsync<UnauthorizedAccessException>(() => _maintenanceService.CreateTicketAsync(1, dto));
    }


    [Test]
    public void CreateTicketAsync_GuestFlow_NoActiveBookingForRoom_ThrowsArgumentException()
    {
        // Arrange
        var dto = new CreateMaintenanceTaskDTO { Description = "Fix TV" };
        _mockCurrentUserService.Setup(s => s.IsInRole("Admin")).Returns(false);
        _mockCurrentUserService.Setup(s => s.IsInRole("FrontDesk")).Returns(false);
        _mockCurrentUserService.Setup(s => s.IsInRole("Housekeeping")).Returns(false);
        _mockCurrentUserService.Setup(s => s.IsInRole("RegisteredUser")).Returns(true);
        _mockCurrentUserService.Setup(s => s.GetUserEmail()).Returns("guest@example.com");

        var user = new User { Id = 100, Email = "guest@example.com" };
        _mockUserRepo.Setup(r => r.GetByEmailAsync("guest@example.com")).ReturnsAsync(user);

        var bookings = new List<Booking>
        {
            new Booking { BookingRooms = new List<BookingRoom> { new BookingRoom { RoomId = 1 } }, BookingStatus = BookingStatus.CheckedOut }, // Wrong status
            new Booking { BookingRooms = new List<BookingRoom> { new BookingRoom { RoomId = 2 } }, BookingStatus = BookingStatus.CheckedIn }  // Wrong room
        };
        var pagedBookings = new PaginatedResult<Booking> { Data = bookings };
        _mockBookingRepo.Setup(r => r.GetPaginatedBookingsWithDetailsAsync(1, 100, It.IsAny<Expression<Func<Booking, bool>>>(), null)).ReturnsAsync(pagedBookings);

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() => _maintenanceService.CreateTicketAsync(1, dto));
    }

    [Test]
    public async Task CreateTicketAsync_GuestFlow_ActiveBookingExists_Success()
    {
        // Arrange
        var dto = new CreateMaintenanceTaskDTO { Description = "Fix TV" };
        _mockCurrentUserService.Setup(s => s.IsInRole("Admin")).Returns(false);
        _mockCurrentUserService.Setup(s => s.IsInRole("FrontDesk")).Returns(false);
        _mockCurrentUserService.Setup(s => s.IsInRole("Housekeeping")).Returns(false);
        _mockCurrentUserService.Setup(s => s.IsInRole("RegisteredUser")).Returns(true);
        _mockCurrentUserService.Setup(s => s.GetUserEmail()).Returns("guest@example.com");

        var user = new User { Id = 100, Email = "guest@example.com" };
        _mockUserRepo.Setup(r => r.GetByEmailAsync("guest@example.com")).ReturnsAsync(user);

        var bookings = new List<Booking>
        {
            new Booking { BookingRooms = new List<BookingRoom> { new BookingRoom { RoomId = 1 } }, BookingStatus = BookingStatus.CheckedIn } // Match!
        };
        var pagedBookings = new PaginatedResult<Booking> { Data = bookings };
        _mockBookingRepo.Setup(r => r.GetPaginatedBookingsWithDetailsAsync(1, 100, It.IsAny<Expression<Func<Booking, bool>>>(), null)).ReturnsAsync(pagedBookings);

        MaintenanceTask? savedTask = null;
        _mockMaintenanceRepo.Setup(r => r.AddAsync(It.IsAny<MaintenanceTask>()))
            .Callback<MaintenanceTask>(t => savedTask = t);

        // Act
        var result = await _maintenanceService.CreateTicketAsync(1, dto);

        // Assert
        Assert.That(savedTask, Is.Not.Null);
        Assert.That(savedTask!.OriginType, Is.EqualTo(MaintenanceOriginType.GuestRequested));
        _mockNotificationService.Verify(n => n.SendMaintenanceAlertAsync(It.IsAny<string>()), Times.Once);
    }

    [Test]
    public void UpdateStatusAsync_TaskNotFound_ThrowsArgumentException()
    {
        // Arrange
        _mockMaintenanceRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((MaintenanceTask?)null);

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() => _maintenanceService.UpdateStatusAsync(1, new UpdateMaintenanceStatusDTO { Status = MaintenanceStatus.InProgress }));
    }

    [Test]
    public async Task UpdateStatusAsync_SetInProgress_SetsStartedAt()
    {
        // Arrange
        var task = new MaintenanceTask { Id = 1, Status = MaintenanceStatus.Pending };
        _mockMaintenanceRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(task);
        _mockCurrentUserService.Setup(c => c.GetUserEmail()).Returns("test@example.com");
        _mockUserRepo.Setup(u => u.GetByEmailAsync("test@example.com")).ReturnsAsync(new User { Id = 1 });
        _mockMaintenanceRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<MaintenanceTask, bool>>>()))
            .ReturnsAsync(new List<MaintenanceTask>());

        // Act
        var result = await _maintenanceService.UpdateStatusAsync(1, new UpdateMaintenanceStatusDTO { Status = MaintenanceStatus.InProgress });

        // Assert
        Assert.That(task.Status, Is.EqualTo(MaintenanceStatus.InProgress));
        Assert.That(task.StartedAt, Is.Not.Null);
        _mockMaintenanceRepo.Verify(r => r.Update(task), Times.Once);
        _mockMaintenanceRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }



    [Test]
    public async Task GetAllTasksAsync_WithSort_ReturnsMappedTasks()
    {
        var tasks = new List<MaintenanceTask> { new MaintenanceTask { Id = 1, Status = MaintenanceStatus.Pending } };
        _mockMaintenanceRepo.Setup(r => r.GetPaginatedResultAsync(1, 10, It.IsAny<Expression<Func<MaintenanceTask, bool>>>(), It.IsAny<Func<System.Linq.IQueryable<MaintenanceTask>, System.Linq.IOrderedQueryable<MaintenanceTask>>>()))
            .ReturnsAsync((int p1, int p2, Expression<Func<MaintenanceTask, bool>> filter, Func<System.Linq.IQueryable<MaintenanceTask>, System.Linq.IOrderedQueryable<MaintenanceTask>> orderBy) =>
            {
                var q = tasks.AsQueryable();
                if (filter != null) q = q.Where(filter);
                if (orderBy != null) q = orderBy(q);
                return new PaginatedResult<MaintenanceTask> { TotalCount = q.Count(), PageNumber = p1, PageSize = p2, Data = q.ToList() };
            });
        _mockMapper.Setup(m => m.Map<IEnumerable<MaintenanceTaskDTO>>(It.IsAny<IEnumerable<MaintenanceTask>>())).Returns(new List<MaintenanceTaskDTO> { new MaintenanceTaskDTO { Id = 1 } });

        var result = await _maintenanceService.GetAllTasksAsync(1, 10, null, "Status", true);

        Assert.That(result.Data.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task GetActiveTasksAsync_WithSort_ReturnsMappedTasks()
    {
        var tasks = new List<MaintenanceTask> { new MaintenanceTask { Id = 1, Status = MaintenanceStatus.Pending } };
        _mockMaintenanceRepo.Setup(r => r.GetPaginatedResultAsync(1, 10, It.IsAny<Expression<Func<MaintenanceTask, bool>>>(), It.IsAny<Func<System.Linq.IQueryable<MaintenanceTask>, System.Linq.IOrderedQueryable<MaintenanceTask>>>()))
            .ReturnsAsync((int p1, int p2, Expression<Func<MaintenanceTask, bool>> filter, Func<System.Linq.IQueryable<MaintenanceTask>, System.Linq.IOrderedQueryable<MaintenanceTask>> orderBy) =>
            {
                var q = tasks.AsQueryable();
                if (filter != null) q = q.Where(filter);
                if (orderBy != null) q = orderBy(q);
                return new PaginatedResult<MaintenanceTask> { TotalCount = q.Count(), PageNumber = p1, PageSize = p2, Data = q.ToList() };
            });
        _mockMapper.Setup(m => m.Map<IEnumerable<MaintenanceTaskDTO>>(It.IsAny<IEnumerable<MaintenanceTask>>())).Returns(new List<MaintenanceTaskDTO> { new MaintenanceTaskDTO { Id = 1 } });

        var result = await _maintenanceService.GetActiveTasksAsync(1, 10, "Status", true);

        Assert.That(result.Data.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task CreateInternalTicketAsync_ShouldSaveTaskAndSendAlert()
    {
        var dto = new CreateInternalMaintenanceTaskDTO { Location = "Lobby", Description = "Spill" };

        await _maintenanceService.CreateInternalTicketAsync(dto);

        _mockMaintenanceRepo.Verify(h => h.AddAsync(It.Is<MaintenanceTask>(t =>
            t.RoomId == null && t.Location == "Lobby" && t.Description == "Spill" && t.OriginType == MaintenanceOriginType.StaffRequested)), Times.Once);
        _mockMaintenanceRepo.Verify(h => h.SaveChangesAsync(), Times.Once);
        _mockNotificationService.Verify(n => n.SendMaintenanceAlertAsync(It.IsAny<string>()), Times.Once);
    }



    [Test]
    public async Task CreateTicketAsync_FrontDeskRole_SetsStaffRequested()
    {
        var dto = new CreateMaintenanceTaskDTO { Description = "Fix TV" };
        _mockCurrentUserService.Setup(s => s.IsInRole("Admin")).Returns(false);
        _mockCurrentUserService.Setup(s => s.IsInRole("FrontDesk")).Returns(true);

        MaintenanceTask? savedTask = null;
        _mockMaintenanceRepo.Setup(r => r.AddAsync(It.IsAny<MaintenanceTask>()))
            .Callback<MaintenanceTask>(t => savedTask = t);

        await _maintenanceService.CreateTicketAsync(1, dto);

        Assert.That(savedTask!.OriginType, Is.EqualTo(MaintenanceOriginType.StaffRequested));
    }

    [Test]
    public async Task CreateTicketAsync_NoRoles_SetsSystemAutomated()
    {
        var dto = new CreateMaintenanceTaskDTO { Description = "Fix TV" };
        _mockCurrentUserService.Setup(s => s.IsInRole("Admin")).Returns(false);
        _mockCurrentUserService.Setup(s => s.IsInRole("FrontDesk")).Returns(false);
        _mockCurrentUserService.Setup(s => s.IsInRole("Housekeeping")).Returns(false);
        _mockCurrentUserService.Setup(s => s.IsInRole("RegisteredUser")).Returns(false);

        MaintenanceTask? savedTask = null;
        _mockMaintenanceRepo.Setup(r => r.AddAsync(It.IsAny<MaintenanceTask>()))
            .Callback<MaintenanceTask>(t => savedTask = t);

        await _maintenanceService.CreateTicketAsync(1, dto);

        Assert.That(savedTask!.OriginType, Is.EqualTo(MaintenanceOriginType.SystemAutomated));
    }

    [Test]
    public async Task GetAllTasksAsync_WithNoSort_ReturnsMappedTasks()
    {
        var tasks = new List<MaintenanceTask> { new MaintenanceTask { Id = 1 } };
        var paged = new PaginatedResult<MaintenanceTask> { TotalCount = 1, PageNumber = 1, PageSize = 10, Data = tasks };
        _mockMaintenanceRepo.Setup(r => r.GetPaginatedResultAsync(1, 10, null, null))
            .ReturnsAsync(paged);
        _mockMapper.Setup(m => m.Map<IEnumerable<MaintenanceTaskDTO>>(tasks)).Returns(new List<MaintenanceTaskDTO> { new MaintenanceTaskDTO { Id = 1 } });

        var result = await _maintenanceService.GetAllTasksAsync(1, 10, null, null, false);

        Assert.That(result.Data.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task GetActiveTasksAsync_WithNoSort_ReturnsMappedTasks()
    {
        var tasks = new List<MaintenanceTask> { new MaintenanceTask { Id = 1 } };
        var paged = new PaginatedResult<MaintenanceTask> { TotalCount = 1, PageNumber = 1, PageSize = 10, Data = tasks };
        _mockMaintenanceRepo.Setup(r => r.GetPaginatedResultAsync(1, 10, It.IsAny<Expression<Func<MaintenanceTask, bool>>>(), null))
            .ReturnsAsync(paged);
        _mockMapper.Setup(m => m.Map<IEnumerable<MaintenanceTaskDTO>>(tasks)).Returns(new List<MaintenanceTaskDTO> { new MaintenanceTaskDTO { Id = 1 } });

        var result = await _maintenanceService.GetActiveTasksAsync(1, 10, null, false);

        Assert.That(result.Data.Count(), Is.EqualTo(1));
    }

    [Test]
    public void CreateTicketAsync_GuestFlow_UserNotFound_ThrowsArgumentException()
    {
        var dto = new CreateMaintenanceTaskDTO { Description = "Fix TV" };
        _mockCurrentUserService.Setup(s => s.IsInRole("RegisteredUser")).Returns(true);
        _mockCurrentUserService.Setup(s => s.GetUserEmail()).Returns("guest@example.com");
        _mockUserRepo.Setup(r => r.GetByEmailAsync("guest@example.com")).ReturnsAsync((User?)null);

        Assert.ThrowsAsync<ArgumentException>(() => _maintenanceService.CreateTicketAsync(1, dto));
    }

    [Test]
    public void CreateTicketAsync_InvalidRoomId_ThrowsArgumentException()
    {
        var dto = new CreateMaintenanceTaskDTO { Description = "Fix TV" };
        var ex = Assert.ThrowsAsync<ArgumentException>(() => _maintenanceService.CreateTicketAsync(0, dto));
        Assert.That(ex.Message, Does.Contain("valid Room ID"));
    }

    [Test]
    public void CreateTicketAsync_RoomNotFound_ThrowsArgumentException()
    {
        _mockRoomRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Room?)null);
        var dto = new CreateMaintenanceTaskDTO { Description = "Fix TV" };
        var ex = Assert.ThrowsAsync<ArgumentException>(() => _maintenanceService.CreateTicketAsync(99, dto));
        Assert.That(ex.Message, Does.Contain("does not exist"));
    }

    [Test]
    public async Task UpdateStatusAsync_SetCompleted_SetsFinishedAt()
    {
        var task = new MaintenanceTask { Id = 1, Status = MaintenanceStatus.InProgress, AssignedToUserId = 1 };
        _mockMaintenanceRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(task);
        _mockCurrentUserService.Setup(c => c.GetUserEmail()).Returns("test@example.com");
        _mockUserRepo.Setup(u => u.GetByEmailAsync("test@example.com")).ReturnsAsync(new User { Id = 1 });

        await _maintenanceService.UpdateStatusAsync(1, new UpdateMaintenanceStatusDTO { Status = MaintenanceStatus.Completed });

        Assert.That(task.Status, Is.EqualTo(MaintenanceStatus.Completed));
        Assert.That(task.FinishedAt, Is.Not.Null);
        _mockMaintenanceRepo.Verify(r => r.Update(task), Times.Once);
        _mockMaintenanceRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}
