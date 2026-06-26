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
public class RoomTypeServiceTests
{
    private Mock<IRoomTypeRepository> _mockRoomTypeRepo;
    private Mock<IBookingRepository> _mockBookingRepo;
    private Mock<IRoomRepository> _mockRoomRepo;
    private Mock<IMapper> _mockMapper;
    private Mock<ICurrentUserService> _mockCurrentUserService;
    private RoomTypeService _roomTypeService;

    [SetUp]
    public void Setup()
    {
        _mockRoomTypeRepo = new Mock<IRoomTypeRepository>();
        _mockBookingRepo = new Mock<IBookingRepository>();
        _mockRoomRepo = new Mock<IRoomRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();

        _roomTypeService = new RoomTypeService(
            _mockRoomTypeRepo.Object,
            _mockBookingRepo.Object,
            _mockMapper.Object,
            _mockCurrentUserService.Object,
            _mockRoomRepo.Object
        );
    }

    [Test]
    public async Task GetAvailableRoomTypesAsync_ShouldMapPaginatedResult()
    {
        var data = new List<RoomTypeAvailability>
        {
            new RoomTypeAvailability
            {
                RoomType = new RoomType { Id = 1, Name = "Deluxe", BasePrice = 100, MaxOccupancy = 2 },
                AvailableCount = 5
            }
        };

        var paginated = new PaginatedResult<RoomTypeAvailability>
        {
            Data = data,
            PageNumber = 1,
            PageSize = 10,
            TotalCount = 1
        };

        _mockRoomRepo.Setup(r => r.GetAvailableRoomTypesAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), 1, 10, null, false))
            .ReturnsAsync(paginated);

        var result = await _roomTypeService.GetAvailableRoomTypesAsync(DateTime.UtcNow, DateTime.UtcNow.AddDays(1), 1, 10, null, false);

        Assert.That(result.TotalCount, Is.EqualTo(1));
        var list = result.Data.ToList();
        Assert.That(list.Count, Is.EqualTo(1));
        Assert.That(list[0].RoomTypeId, Is.EqualTo(1));
        Assert.That(list[0].AvailableCount, Is.EqualTo(5));
    }

    [Test]
    public async Task GetRoomTypesAsync_ShouldOverrideIncludeRetired_IfNotAdmin()
    {
        _mockCurrentUserService.Setup(c => c.IsInRole("Admin")).Returns(false);
        var types = new List<RoomType> { new RoomType { Id = 1 } };
        _mockRoomTypeRepo.Setup(r => r.GetRoomTypesAsync(false)).ReturnsAsync(types); // expects false
        _mockMapper.Setup(m => m.Map<IEnumerable<RoomTypeDTO>>(types)).Returns(new List<RoomTypeDTO> { new RoomTypeDTO { Id = 1 } });

        var result = await _roomTypeService.GetRoomTypesAsync(1, 10, true);
        var data = result.Data.ToList();

        Assert.That(data.Count, Is.EqualTo(1));
        _mockRoomTypeRepo.Verify(r => r.GetRoomTypesAsync(false), Times.Once);
    }

    [Test]
    public async Task GetRoomTypesAsync_ShouldDoNothing_IfIncludeRetiredIsFalse()
    {
        var types = new List<RoomType> { new RoomType { Id = 1 } };
        _mockRoomTypeRepo.Setup(r => r.GetRoomTypesAsync(false)).ReturnsAsync(types); // expects false
        _mockMapper.Setup(m => m.Map<IEnumerable<RoomTypeDTO>>(types)).Returns(new List<RoomTypeDTO> { new RoomTypeDTO { Id = 1 } });

        var result = await _roomTypeService.GetRoomTypesAsync(1, 10, false);
        var data = result.Data.ToList();

        Assert.That(data.Count, Is.EqualTo(1));
        _mockRoomTypeRepo.Verify(r => r.GetRoomTypesAsync(false), Times.Once);
    }

    [Test]
    public async Task GetRoomTypesAsync_ShouldAllowIncludeRetired_IfAdmin()
    {
        _mockCurrentUserService.Setup(c => c.IsInRole("Admin")).Returns(true);
        var types = new List<RoomType> { new RoomType { Id = 1 } };
        _mockRoomTypeRepo.Setup(r => r.GetRoomTypesAsync(true)).ReturnsAsync(types); // expects true
        _mockMapper.Setup(m => m.Map<IEnumerable<RoomTypeDTO>>(types)).Returns(new List<RoomTypeDTO> { new RoomTypeDTO { Id = 1 } });

        var result = await _roomTypeService.GetRoomTypesAsync(1, 10, true);
        var data = result.Data.ToList();

        Assert.That(data.Count, Is.EqualTo(1));
        _mockRoomTypeRepo.Verify(r => r.GetRoomTypesAsync(true), Times.Once);
    }

    [Test]
    public async Task CreateRoomTypeAsync_ShouldAddAndReturn()
    {
        // Changed to CreateRoomTypeDTO and MaxOccupancy
        var dto = new CreateRoomTypeDTO { Name = "Deluxe", BasePrice = 100m, MaxOccupancy = 2 };
        _mockMapper.Setup(m => m.Map<RoomTypeDTO>(It.IsAny<RoomType>())).Returns(new RoomTypeDTO { Id = 1, Name = "Deluxe" });

        var result = await _roomTypeService.CreateRoomTypeAsync(dto);

        Assert.That(result.Id, Is.EqualTo(1));
        _mockRoomTypeRepo.Verify(r => r.AddAsync(It.Is<RoomType>(rt => rt.Name == "Deluxe" && rt.IsActive)), Times.Once);
        _mockRoomTypeRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void UpdateRoomTypeAsync_ShouldThrow_IfNotFoundOrInactive()
    {
        // Simulate repository returning null to trigger the "not found" exception
        _mockRoomTypeRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((RoomType?)null);
        var dto = new UpdateRoomTypeDTO { Name = "Test" };

        // Use Assert.ThrowsAsync correctly
        var ex = Assert.ThrowsAsync<ArgumentException>(() => _roomTypeService.UpdateRoomTypeAsync(99, dto));
        Assert.That(ex.Message, Does.Contain("not found"));
    }

    [Test]
    public async Task UpdateRoomTypeAsync_ShouldUpdateAndReturn()
    {
        var existingType = new RoomType { Id = 1, Name = "Old", IsActive = true };
        var dto = new UpdateRoomTypeDTO { Name = "New", BasePrice = 150m, MaxOccupancy = 4, Description = "NewDesc", ImageUrl = "newurl", SquareFootage = 300, BedConfiguration = "Queen" };

        _mockRoomTypeRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingType);
        _mockMapper.Setup(m => m.Map<RoomTypeDTO>(existingType)).Returns(new RoomTypeDTO { Id = 1, Name = "New" });

        var result = await _roomTypeService.UpdateRoomTypeAsync(1, dto);

        Assert.That(existingType.Name, Is.EqualTo("New"));
        Assert.That(existingType.BasePrice, Is.EqualTo(150m));
        Assert.That(existingType.MaxOccupancy, Is.EqualTo(4));
        Assert.That(existingType.Description, Is.EqualTo("NewDesc"));
        Assert.That(existingType.ImageUrl, Is.EqualTo("newurl"));
        Assert.That(existingType.SquareFootage, Is.EqualTo(300));
        Assert.That(existingType.BedConfiguration, Is.EqualTo("Queen"));
        _mockRoomTypeRepo.Verify(r => r.Update(existingType), Times.Once);
        _mockRoomTypeRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task UpdateRoomTypeAsync_ShouldUpdateNothing_IfDtoPropertiesAreNull()
    {
        var existingType = new RoomType { Id = 1, Name = "Old", IsActive = true, BasePrice = 100m, MaxOccupancy = 2, Description = "Desc", ImageUrl = "url", SquareFootage = 200, BedConfiguration = "King" };
        var dto = new UpdateRoomTypeDTO { Name = null, BasePrice = null, MaxOccupancy = null, Description = null, ImageUrl = null, SquareFootage = null, BedConfiguration = null };

        _mockRoomTypeRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingType);
        _mockMapper.Setup(m => m.Map<RoomTypeDTO>(existingType)).Returns(new RoomTypeDTO { Id = 1, Name = "Old" });

        var result = await _roomTypeService.UpdateRoomTypeAsync(1, dto);

        Assert.That(existingType.Name, Is.EqualTo("Old"));
        Assert.That(existingType.BasePrice, Is.EqualTo(100m));
        Assert.That(existingType.MaxOccupancy, Is.EqualTo(2));
        Assert.That(existingType.Description, Is.EqualTo("Desc"));
        Assert.That(existingType.ImageUrl, Is.EqualTo("url"));
        Assert.That(existingType.SquareFootage, Is.EqualTo(200));
        Assert.That(existingType.BedConfiguration, Is.EqualTo("King"));
        _mockRoomTypeRepo.Verify(r => r.Update(existingType), Times.Once);
        _mockRoomTypeRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void DeleteRoomTypeAsync_ShouldThrow_IfNotFound()
    {
        _mockRoomTypeRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((RoomType?)null);
        var ex = Assert.ThrowsAsync<ArgumentException>(() => _roomTypeService.DeleteRoomTypeAsync(99));
        Assert.That(ex.Message, Does.Contain("RoomType not found"));
    }

    [Test]
    public void DeleteRoomTypeAsync_ShouldThrow_IfActiveBookingsExist()
    {
        var existingType = new RoomType { Id = 1, IsActive = true };
        _mockRoomTypeRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingType);

        var activeBookings = new List<Booking> { new Booking { Id = 10, BookingStatus = BookingStatus.Booked } };
        _mockBookingRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Booking, bool>>>())).ReturnsAsync(activeBookings);

        var ex = Assert.ThrowsAsync<InvalidOperationException>(() => _roomTypeService.DeleteRoomTypeAsync(1));
        Assert.That(ex.Message, Does.Contain("Cannot delete RoomType"));
    }

    [Test]
    public async Task DeleteRoomTypeAsync_ShouldSoftDelete_IfNoActiveBookings()
    {
        var existingType = new RoomType { Id = 1, IsActive = true };
        _mockRoomTypeRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingType);
        _mockBookingRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Booking, bool>>>())).ReturnsAsync(new List<Booking>());

        await _roomTypeService.DeleteRoomTypeAsync(1);

        Assert.That(existingType.IsActive, Is.False);
        _mockRoomTypeRepo.Verify(r => r.Update(existingType), Times.Once);
        _mockRoomTypeRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void CreateRoomTypeAsync_ShouldThrow_IfDuplicate()
    {
        // Changed to CreateRoomTypeDTO and MaxOccupancy
        var dto = new CreateRoomTypeDTO { Name = "Deluxe", MaxOccupancy = 2, BasePrice = 100 };
        var existingItems = new List<RoomType> { new RoomType { Name = "Deluxe" } };
        _mockRoomTypeRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<RoomType, bool>>>())).ReturnsAsync(existingItems);

        var ex = Assert.ThrowsAsync<InvalidOperationException>(() => _roomTypeService.CreateRoomTypeAsync(dto));
        Assert.That(ex.Message, Does.Contain("already exists in the system"));
    }
    [Test]
    public void CreateRoomTypeAsync_ShouldThrow_IfBasePriceNegative()
    {
        var dto = new CreateRoomTypeDTO { Name = "Test", BasePrice = -10, MaxOccupancy = 2 };
        var ex = Assert.ThrowsAsync<ArgumentException>(() => _roomTypeService.CreateRoomTypeAsync(dto));
        Assert.That(ex.Message, Does.Contain("cannot be negative"));
    }

    [Test]
    public void CreateRoomTypeAsync_ShouldThrow_IfMaxOccupancyLessThanOne()
    {
        var dto = new CreateRoomTypeDTO { Name = "Test", BasePrice = 100, MaxOccupancy = 0 };
        var ex = Assert.ThrowsAsync<ArgumentException>(() => _roomTypeService.CreateRoomTypeAsync(dto));
        Assert.That(ex.Message, Does.Contain("must be at least 1"));
    }

    [Test]
    public void UpdateRoomTypeAsync_ShouldThrow_IfBasePriceNegative()
    {
        var dto = new UpdateRoomTypeDTO { BasePrice = -10 };
        var ex = Assert.ThrowsAsync<ArgumentException>(() => _roomTypeService.UpdateRoomTypeAsync(1, dto));
        Assert.That(ex.Message, Does.Contain("cannot be negative"));
    }

    [Test]
    public void UpdateRoomTypeAsync_ShouldThrow_IfMaxOccupancyLessThanOne()
    {
        var dto = new UpdateRoomTypeDTO { MaxOccupancy = 0 };
        var ex = Assert.ThrowsAsync<ArgumentException>(() => _roomTypeService.UpdateRoomTypeAsync(1, dto));
        Assert.That(ex.Message, Does.Contain("must be at least 1"));
    }
}
