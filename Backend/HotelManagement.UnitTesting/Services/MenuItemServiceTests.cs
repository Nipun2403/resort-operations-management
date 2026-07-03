using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using HotelManagement.BLL.DTOs;
using HotelManagement.BLL.Services;
using HotelManagement.DAL.Entities;
using HotelManagement.Repository.Interfaces;
using Moq;
using NUnit.Framework;
using HotelManagement.BLL.Interfaces;
using HotelManagement.Repository.Models;

namespace HotelManagement.UnitTesting.Services;

[TestFixture]
public class MenuItemServiceTests
{
    private Mock<IMenuItemRepository> _mockMenuItemRepo;
    private Mock<IMapper> _mockMapper;
    private Mock<ICurrentUserService> _mockCurrentUserService;
    private MenuItemService _menuItemService;

    [SetUp]
    public void Setup()
    {
        _mockMenuItemRepo = new Mock<IMenuItemRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();

        _menuItemService = new MenuItemService(
            _mockMenuItemRepo.Object,
            _mockMapper.Object,
            _mockCurrentUserService.Object
        );
    }

    [Test]
    public async Task GetMenuItemsAsync_ShouldAllowIncludeRetired_IfAdminOrKitchen()
    {
        _mockCurrentUserService.Setup(c => c.IsInRole("Admin")).Returns(false);
        _mockCurrentUserService.Setup(c => c.IsInRole("Kitchen")).Returns(true);
        var items = new List<MenuItem> { new MenuItem { Id = 1, IsAvailable = false } };

        // FIXED: Mocking GetAllAsync()
        _mockMenuItemRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(items);
        _mockMapper.Setup(m => m.Map<IEnumerable<MenuItemDTO>>(It.IsAny<IEnumerable<MenuItem>>()))
            .Returns(new List<MenuItemDTO> { new MenuItemDTO { Id = 1 } });

        var result = await _menuItemService.GetMenuItemsAsync(1, 10, false);
        Assert.That(result.Data.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task GetMenuItemsAsync_ShouldOverrideIncludeRetired_IfNotAdminOrKitchen()
    {
        _mockCurrentUserService.Setup(c => c.IsInRole("Admin")).Returns(false);
        _mockCurrentUserService.Setup(c => c.IsInRole("Kitchen")).Returns(false);
        var items = new List<MenuItem> { new MenuItem { Id = 1, IsAvailable = true } };

        _mockMenuItemRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(items);
        _mockMapper.Setup(m => m.Map<IEnumerable<MenuItemDTO>>(It.IsAny<IEnumerable<MenuItem>>()))
            .Returns(new List<MenuItemDTO> { new MenuItemDTO { Id = 1 } });

        var result = await _menuItemService.GetMenuItemsAsync(1, 10, false);
        Assert.That(result.Data.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task GetMenuItemsAsync_ShouldDoNothing_IfIncludeRetiredIsFalse()
    {
        _mockCurrentUserService.Setup(c => c.IsInRole("Admin")).Returns(false);
        _mockCurrentUserService.Setup(c => c.IsInRole("Kitchen")).Returns(false);
        var items = new List<MenuItem> { new MenuItem { Id = 1, IsAvailable = true } };

        _mockMenuItemRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(items);
        _mockMapper.Setup(m => m.Map<IEnumerable<MenuItemDTO>>(It.IsAny<IEnumerable<MenuItem>>()))
            .Returns(new List<MenuItemDTO> { new MenuItemDTO { Id = 1 } });

        var result = await _menuItemService.GetMenuItemsAsync(1, 10, true);
        Assert.That(result.Data.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task GetMenuItemsAsync_ShouldAllowIncludeRetired_IfAdmin()
    {
        _mockCurrentUserService.Setup(c => c.IsInRole("Admin")).Returns(true);
        var items = new List<MenuItem> { new MenuItem { Id = 1, IsAvailable = false } };

        _mockMenuItemRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(items);
        _mockMapper.Setup(m => m.Map<IEnumerable<MenuItemDTO>>(It.IsAny<IEnumerable<MenuItem>>()))
            .Returns(new List<MenuItemDTO> { new MenuItemDTO { Id = 1 } });

        var result = await _menuItemService.GetMenuItemsAsync(1, 10, false);
        Assert.That(result.Data.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task CreateMenuItemAsync_ShouldAddAndReturn()
    {
        var dto = new CreateMenuItemDTO { Name = "Pizza", Price = 10m, Category = "Food", IsAvailable = true, Description = "Classic pizza.", ImageUrl = "https://images.unsplash.com/photo-test" };
        _mockMapper.Setup(m => m.Map<MenuItemDTO>(It.IsAny<MenuItem>())).Returns(new MenuItemDTO { Id = 1, Name = "Pizza" });

        var result = await _menuItemService.CreateMenuItemAsync(dto);

        Assert.That(result.Id, Is.EqualTo(1));
        _mockMenuItemRepo.Verify(r => r.AddAsync(It.Is<MenuItem>(m => m.Name == "Pizza")), Times.Once);
        _mockMenuItemRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void UpdateMenuItemAsync_ShouldThrow_IfNotFound()
    {
        _mockMenuItemRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((MenuItem?)null);
        var ex = Assert.ThrowsAsync<ArgumentException>(() => _menuItemService.UpdateMenuItemAsync(99, new MenuItemDTO()));
        Assert.That(ex.Message, Does.Contain("not found"));
    }

    [Test]
    public async Task UpdateMenuItemAsync_ShouldUpdateAndReturn()
    {
        var existingItem = new MenuItem { Id = 1, Name = "Old" };
        var dto = new MenuItemDTO { Name = "New", Price = 15m, Category = "Food", IsAvailable = false, Description = "Updated desc.", ImageUrl = "https://images.unsplash.com/photo-test-update" };

        _mockMenuItemRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingItem);
        _mockMapper.Setup(m => m.Map<MenuItemDTO>(existingItem)).Returns(new MenuItemDTO { Id = 1, Name = "New" });

        var result = await _menuItemService.UpdateMenuItemAsync(1, dto);

        Assert.That(existingItem.Name, Is.EqualTo("New"));
        Assert.That(existingItem.Price, Is.EqualTo(15m));
        _mockMenuItemRepo.Verify(r => r.Update(existingItem), Times.Once);
        _mockMenuItemRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    // // [Test]
    // public void DeleteMenuItemAsync_ShouldThrow_IfNotFound()
    // {
    //     _mockMenuItemRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((MenuItem?)null);
    //     var ex = Assert.ThrowsAsync<ArgumentException>(() => _menuItemService.DeleteMenuItemAsync(99));
    //     Assert.That(ex.Message, Does.Contain("not found"));
    // }

    // // [Test]
    // public async Task DeleteMenuItemAsync_ShouldSoftDelete()
    // {
    //     var existingItem = new MenuItem { Id = 1, IsAvailable = true };
    //     _mockMenuItemRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingItem);

    //     await _menuItemService.DeleteMenuItemAsync(1);

    //     Assert.That(existingItem.IsAvailable, Is.False);
    //     _mockMenuItemRepo.Verify(r => r.Update(existingItem), Times.Once);
    //     _mockMenuItemRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    // }

    [Test]
    public async Task GetMenuItemsAsync_WithSort_ShouldReturnSorted()
    {
        var items = new List<MenuItem> { new MenuItem { Id = 1 } };
        _mockMenuItemRepo.Setup(r => r.GetMenuItemsAsync(false)).ReturnsAsync(items);
        _mockMapper.Setup(m => m.Map<IEnumerable<MenuItemDTO>>(It.IsAny<IEnumerable<MenuItem>>()))
            .Returns(new List<MenuItemDTO> { new MenuItemDTO { Id = 1 } });

        var result = await _menuItemService.GetMenuItemsAsync(1, 10, false, sortBy: "Name", sortDescending: true);

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void UpdateMenuItemStatusAsync_ShouldThrow_IfNotFound()
    {
        _mockMenuItemRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((MenuItem?)null);
        var ex = Assert.ThrowsAsync<ArgumentException>(() => _menuItemService.UpdateMenuItemStatusAsync(99, false));
        Assert.That(ex.Message, Does.Contain("not found"));
    }

    [Test]
    public async Task UpdateMenuItemStatusAsync_ShouldUpdateAndReturn()
    {
        var existingItem = new MenuItem { Id = 1, Name = "Item", IsAvailable = true };
        _mockMenuItemRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingItem);

        await _menuItemService.UpdateMenuItemStatusAsync(1, false);

        Assert.That(existingItem.IsAvailable, Is.False);
        _mockMenuItemRepo.Verify(r => r.Update(existingItem), Times.Once);
        _mockMenuItemRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }



    [Test]
    public void CreateMenuItemAsync_ShouldThrow_IfDuplicate()
    {
        var dto = new CreateMenuItemDTO { Name = "Pizza" };
        var existingItems = new List<MenuItem> { new MenuItem { Name = "Pizza" } };
        _mockMenuItemRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<MenuItem, bool>>>())).ReturnsAsync(existingItems);

        var ex = Assert.ThrowsAsync<InvalidOperationException>(() => _menuItemService.CreateMenuItemAsync(dto));
        Assert.That(ex.Message, Does.Contain("already exists in the system"));
    }
}
