using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using HotelManagement.BLL.DTOs;
using HotelManagement.BLL.Services;
using HotelManagement.DAL.Entities;
using HotelManagement.Repository.Interfaces;
using HotelManagement.Repository.Models;
using Moq;
using NUnit.Framework;

namespace HotelManagement.UnitTesting.Services;

[TestFixture]
public class RoomServiceTests
{
    private Mock<IRoomRepository> _mockRoomRepo;
    private Mock<IRoomTypeRepository> _mockRoomTypeRepo;
    private Mock<IBookingRepository> _mockBookingRepo;
    private Mock<IMapper> _mockMapper;
    private RoomService _roomService;

    [SetUp]
    public void Setup()
    {
        _mockRoomRepo = new Mock<IRoomRepository>();
        _mockRoomTypeRepo = new Mock<IRoomTypeRepository>();
        _mockBookingRepo = new Mock<IBookingRepository>();
        _mockMapper = new Mock<IMapper>();

        _roomService = new RoomService(
            _mockRoomRepo.Object,
            _mockRoomTypeRepo.Object,
            _mockBookingRepo.Object,
            _mockMapper.Object
        );
    }

    [Test]
    public async Task GetRoomsAsync_ShouldReturnMappedRooms()
    {
        var rooms = new List<Room> { new Room { Id = 1 } };
        var paged = new PaginatedResult<Room> { TotalCount = 1, PageNumber = 1, PageSize = 10, Data = rooms };
        _mockRoomRepo.Setup(r => r.GetPaginatedRoomsAsync(1, 10, false, null, null, null, false)).ReturnsAsync(paged);
        _mockMapper.Setup(m => m.Map<IEnumerable<RoomDTO>>(rooms)).Returns(new List<RoomDTO> { new RoomDTO { Id = 1 } });

        var result = await _roomService.GetRoomsAsync(1, 10, includeRetired: false);

        Assert.That(result.Data.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task GetRoomsAsync_ShouldFilterAndSort()
    {
        var rooms = new List<Room>
        {
            new Room { Id = 2, RoomTypeId = 2, RoomNumber = "102" },
            new Room { Id = 1, RoomTypeId = 2, RoomNumber = "101" }
        };
        var paged = new PaginatedResult<Room> { TotalCount = 2, PageNumber = 1, PageSize = 10, Data = rooms };
        _mockRoomRepo.Setup(r => r.GetPaginatedRoomsAsync(1, 10, false, 2, null, "RoomNumber", true)).ReturnsAsync(paged);

        _mockMapper.Setup(m => m.Map<IEnumerable<RoomDTO>>(rooms))
            .Returns(new List<RoomDTO> 
            { 
                new RoomDTO { Id = 2, RoomNumber = "102" },
                new RoomDTO { Id = 1, RoomNumber = "101" }
            });

        // Act with roomTypeId = 2, sortBy = "RoomNumber", sortDescending = true
        var result = await _roomService.GetRoomsAsync(1, 10, roomTypeId: 2, includeRetired: false, sortBy: "RoomNumber", sortDescending: true);

        Assert.That(result.Data.Count(), Is.EqualTo(2));
        var list = result.Data.ToList();
        Assert.That(list[0].RoomNumber, Is.EqualTo("102"));
        Assert.That(list[1].RoomNumber, Is.EqualTo("101"));
    }

    [Test]
    public void CreateRoomAsync_ShouldThrow_IfRoomTypeNotFoundOrInactive()
    {
        var dto = new CreateUpdateRoomDTO { RoomTypeId = 99 };
        _mockRoomTypeRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((RoomType?)null);

        var ex = Assert.ThrowsAsync<ArgumentException>(() => _roomService.CreateRoomAsync(dto));
        Assert.That(ex.Message, Does.Contain("Invalid or inactive"));

        var inactiveType = new RoomType { Id = 1, IsActive = false };
        _mockRoomTypeRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(inactiveType);
        dto.RoomTypeId = 1;

        var ex2 = Assert.ThrowsAsync<ArgumentException>(() => _roomService.CreateRoomAsync(dto));
        Assert.That(ex2.Message, Does.Contain("Invalid or inactive"));
    }

    [Test]
    public async Task CreateRoomAsync_ShouldAddAndReturn()
    {
        var dto = new CreateUpdateRoomDTO { RoomNumber = "101", RoomTypeId = 1 };
        var roomType = new RoomType { Id = 1, Name = "Deluxe", IsActive = true };

        _mockRoomTypeRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(roomType);
        _mockMapper.Setup(m => m.Map<RoomDTO>(It.IsAny<Room>())).Returns(new RoomDTO { Id = 1, RoomNumber = "101" });

        var result = await _roomService.CreateRoomAsync(dto);

        Assert.That(result.RoomNumber, Is.EqualTo("101"));
        Assert.That(result.RoomTypeName, Is.EqualTo("Deluxe"));

        _mockRoomRepo.Verify(r => r.AddAsync(It.Is<Room>(rm => rm.RoomNumber == "101" && rm.RoomTypeId == 1 && rm.IsActive)), Times.Once);
        _mockRoomRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void UpdateRoomAsync_ShouldThrow_IfRoomNotFound()
    {
        var dto = new CreateUpdateRoomDTO();

        _mockRoomRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Room?)null);
        var ex = Assert.ThrowsAsync<ArgumentException>(() => _roomService.UpdateRoomAsync(99, dto));
        Assert.That(ex.Message, Does.Contain("Room not found"));
    }

    [Test]
    public async Task UpdateRoomAsync_ShouldAllow_IfRoomInactive()
    {
        var dto = new CreateUpdateRoomDTO { RoomNumber = "101", RoomTypeId = 1 };
        var inactiveRoom = new Room { Id = 1, RoomNumber = "101", RoomTypeId = 1, IsActive = false };
        var activeType = new RoomType { Id = 1, Name = "Standard", IsActive = true };

        _mockRoomRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(inactiveRoom);
        _mockRoomTypeRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(activeType);
        _mockMapper.Setup(m => m.Map<RoomDTO>(inactiveRoom)).Returns(new RoomDTO { Id = 1, RoomNumber = "101" });

        await _roomService.UpdateRoomAsync(1, dto);

        _mockRoomRepo.Verify(r => r.Update(It.IsAny<Room>()), Times.Once);
        _mockRoomRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void UpdateRoomAsync_ShouldThrow_IfRoomTypeNotFoundOrInactive()
    {
        var dto = new CreateUpdateRoomDTO { RoomTypeId = 99 };
        var activeRoom = new Room { Id = 1, IsActive = true };

        _mockRoomRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(activeRoom);
        _mockRoomTypeRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((RoomType?)null);

        var ex = Assert.ThrowsAsync<ArgumentException>(() => _roomService.UpdateRoomAsync(1, dto));
        Assert.That(ex.Message, Does.Contain("Invalid or inactive"));
    }

    [Test]
    public async Task UpdateRoomAsync_ShouldUpdateAndReturn()
    {
        var dto = new CreateUpdateRoomDTO { RoomNumber = "102", RoomTypeId = 2 };
        var activeRoom = new Room { Id = 1, RoomNumber = "101", RoomTypeId = 1, IsActive = true };
        var activeType = new RoomType { Id = 2, Name = "Suite", IsActive = true };

        _mockRoomRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(activeRoom);
        _mockRoomTypeRepo.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(activeType);

        _mockMapper.Setup(m => m.Map<RoomDTO>(activeRoom)).Returns(new RoomDTO { Id = 1, RoomNumber = "102" });

        var result = await _roomService.UpdateRoomAsync(1, dto);

        Assert.That(activeRoom.RoomNumber, Is.EqualTo("102"));
        Assert.That(activeRoom.RoomTypeId, Is.EqualTo(2));
        Assert.That(result.RoomTypeName, Is.EqualTo("Suite"));

        _mockRoomRepo.Verify(r => r.Update(activeRoom), Times.Once);
        _mockRoomRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void DeleteRoomAsync_ShouldThrow_IfNotFound()
    {
        _mockRoomRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Room?)null);
        var ex = Assert.ThrowsAsync<ArgumentException>(() => _roomService.DeleteRoomAsync(99));
        Assert.That(ex.Message, Does.Contain("Room not found"));
    }

    [Test]
    public async Task DeleteRoomAsync_ShouldSoftDelete()
    {
        var room = new Room { Id = 1, IsActive = true };
        _mockRoomRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(room);

        await _roomService.DeleteRoomAsync(1);

        Assert.That(room.IsActive, Is.False);
        _mockRoomRepo.Verify(r => r.Update(room), Times.Once);
        _mockRoomRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void CreateRoomAsync_ShouldThrow_IfDuplicate()
    {
        var dto = new CreateUpdateRoomDTO { RoomNumber = "101" };
        var existingItems = new List<Room> { new Room { RoomNumber = "101" } };
        _mockRoomRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Room, bool>>>())).ReturnsAsync(existingItems);

        var ex = Assert.ThrowsAsync<InvalidOperationException>(() => _roomService.CreateRoomAsync(dto));
        Assert.That(ex.Message, Does.Contain("already exists in the system"));
    }

    [Test]
    public async Task UpdateRoomAsync_WithIsActiveValue_ShouldUpdateIsActive()
    {
        var dto = new CreateUpdateRoomDTO { RoomNumber = "102", RoomTypeId = 2, IsActive = false };
        var activeRoom = new Room { Id = 1, RoomNumber = "101", RoomTypeId = 1, IsActive = true };
        var activeType = new RoomType { Id = 2, Name = "Suite", IsActive = true };

        _mockRoomRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(activeRoom);
        _mockRoomTypeRepo.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(activeType);
        _mockMapper.Setup(m => m.Map<RoomDTO>(activeRoom)).Returns(new RoomDTO { Id = 1, RoomNumber = "102" });

        await _roomService.UpdateRoomAsync(1, dto);

        Assert.That(activeRoom.IsActive, Is.False);
    }
}
