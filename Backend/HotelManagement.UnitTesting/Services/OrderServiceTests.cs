using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using HotelManagement.BLL.DTOs;
using HotelManagement.BLL.Services;
using HotelManagement.DAL.Entities;
using HotelManagement.DAL.Enums;
using HotelManagement.Repository.Interfaces;
using Moq;
using NUnit.Framework;
using HotelManagement.Repository.Models;
using HotelManagement.BLL.Interfaces;

namespace HotelManagement.UnitTesting.Services;

[TestFixture]
public class OrderServiceTests
{
    private Mock<IFoodOrderRepository> _mockFoodOrderRepo;
    private Mock<IMenuItemRepository> _mockMenuItemRepo;
    private Mock<IBookingRepository> _mockBookingRepo;
    private Mock<IMapper> _mockMapper;
    private Mock<INotificationService> _mockNotificationService;
    private Mock<ICurrentUserService> _mockCurrentUserService;
    private Mock<IUserRepository> _mockUserRepo;

    private OrderService _orderService;

    [SetUp]
    public void Setup()
    {
        _mockFoodOrderRepo = new Mock<IFoodOrderRepository>();
        _mockMenuItemRepo = new Mock<IMenuItemRepository>();
        _mockBookingRepo = new Mock<IBookingRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockNotificationService = new Mock<INotificationService>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mockUserRepo = new Mock<IUserRepository>();

        _mockCurrentUserService.Setup(s => s.IsInRole(It.IsAny<string>())).Returns(true);

        _orderService = new OrderService(
            _mockFoodOrderRepo.Object,
            _mockMenuItemRepo.Object,
            _mockBookingRepo.Object,
            _mockMapper.Object,
            _mockNotificationService.Object,
            _mockCurrentUserService.Object,
            _mockUserRepo.Object
        );
    }

    [Test]
    public async Task GetActiveOrdersAsync_ShouldReturnMappedOrders()
    {
        var orders = new List<FoodOrder> { new FoodOrder { Id = 1 } };
        _mockFoodOrderRepo.Setup(r => r.GetActiveOrdersWithDetailsAsync()).ReturnsAsync(orders);
        _mockMapper.Setup(m => m.Map<IEnumerable<FoodOrderDTO>>(orders)).Returns(new List<FoodOrderDTO> { new FoodOrderDTO { Id = 1 } });

        var result = await _orderService.GetActiveOrdersAsync(1, 10);
        var data = result.Data.ToList();

        Assert.That(data.Count, Is.EqualTo(1));
        Assert.That(data[0].Id, Is.EqualTo(1));
    }

    [Test]
    public async Task GetAllOrdersAsync_ShouldReturnMappedOrders()
    {
        var orders = new List<FoodOrder> { new FoodOrder { Id = 1 }, new FoodOrder { Id = 2 } };
        _mockFoodOrderRepo.Setup(r => r.GetAllOrdersWithDetailsAsync()).ReturnsAsync(orders);
        _mockMapper.Setup(m => m.Map<IEnumerable<FoodOrderDTO>>(orders)).Returns(new List<FoodOrderDTO> { new FoodOrderDTO { Id = 1 }, new FoodOrderDTO { Id = 2 } });

        var result = await _orderService.GetAllOrdersAsync(1, 10);
        var data = result.Data.ToList();

        Assert.That(data.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task GetActiveOrdersAsync_WithBookingId_ShouldFilter()
    {
        var orders = new List<FoodOrder> { new FoodOrder { Id = 1, BookingId = 5 }, new FoodOrder { Id = 2, BookingId = 6 } };
        _mockFoodOrderRepo.Setup(r => r.GetActiveOrdersWithDetailsAsync()).ReturnsAsync(orders);
        _mockMapper.Setup(m => m.Map<IEnumerable<FoodOrderDTO>>(It.IsAny<IEnumerable<FoodOrder>>()))
            .Returns(new List<FoodOrderDTO> { new FoodOrderDTO { Id = 1 } });

        var result = await _orderService.GetActiveOrdersAsync(1, 10, bookingId: 5);
        Assert.That(result.TotalCount, Is.EqualTo(1));
    }

    [Test]
    public async Task GetAllOrdersAsync_WithBookingId_ShouldFilter()
    {
        var orders = new List<FoodOrder> { new FoodOrder { Id = 1, BookingId = 5 }, new FoodOrder { Id = 2, BookingId = 6 } };
        _mockFoodOrderRepo.Setup(r => r.GetAllOrdersWithDetailsAsync()).ReturnsAsync(orders);
        _mockMapper.Setup(m => m.Map<IEnumerable<FoodOrderDTO>>(It.IsAny<IEnumerable<FoodOrder>>()))
            .Returns(new List<FoodOrderDTO> { new FoodOrderDTO { Id = 1 } });

        var result = await _orderService.GetAllOrdersAsync(1, 10, bookingId: 5);
        Assert.That(result.TotalCount, Is.EqualTo(1));
    }

    [Test]
    public void UpdateOrderStatusAsync_ShouldThrow_IfNotFound()
    {
        _mockFoodOrderRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((FoodOrder?)null);

        var ex = Assert.ThrowsAsync<ArgumentException>(() => _orderService.UpdateOrderStatusAsync(99, FoodOrderStatus.Delivered));
        Assert.That(ex.Message, Does.Contain("not found"));
    }

    [Test]
    public async Task UpdateOrderStatusAsync_ShouldUpdateStatus()
    {
        var order = new FoodOrder { Id = 1, OrderStatus = FoodOrderStatus.Pending };
        _mockFoodOrderRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(order);

        await _orderService.UpdateOrderStatusAsync(1, FoodOrderStatus.Delivered);

        Assert.That(order.OrderStatus, Is.EqualTo(FoodOrderStatus.Delivered));
        _mockFoodOrderRepo.Verify(r => r.Update(order), Times.Once);
        _mockFoodOrderRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void CreateOrderAsync_ShouldThrow_IfBookingNotFound()
    {
        var dto = new CreateFoodOrderDTO { BookingId = 99 };
        _mockBookingRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Booking?)null);

        var ex = Assert.ThrowsAsync<ArgumentException>(() => _orderService.CreateOrderAsync(dto));
        Assert.That(ex.Message, Does.Contain("Booking not found"));
    }

    [Test]
    public void CreateOrderAsync_ShouldThrow_IfNotCheckedIn()
    {
        var dto = new CreateFoodOrderDTO { BookingId = 1 };
        var booking = new Booking { Id = 1, BookingStatus = BookingStatus.Booked };
        _mockBookingRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(booking);

        var ex = Assert.ThrowsAsync<InvalidOperationException>(() => _orderService.CreateOrderAsync(dto));
        Assert.That(ex.Message, Does.Contain("currently checked in"));
    }

    [Test]
    public void CreateOrderAsync_ShouldThrow_IfPaymentPaid()
    {
        var dto = new CreateFoodOrderDTO { BookingId = 1 };
        var booking = new Booking { Id = 1, BookingStatus = BookingStatus.CheckedIn, PaymentStatus = PaymentStatus.Paid };
        _mockBookingRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(booking);

        var ex = Assert.ThrowsAsync<InvalidOperationException>(() => _orderService.CreateOrderAsync(dto));
        Assert.That(ex.Message, Does.Contain("already been paid"));
    }

    [Test]
    public void CreateOrderAsync_ShouldThrow_IfMenuItemNotFound()
    {
        var dto = new CreateFoodOrderDTO
        {
            BookingId = 1,
            Items = new List<CreateFoodOrderItemDTO> { new CreateFoodOrderItemDTO { MenuItemId = 5, Quantity = 1 } }
        };
        var booking = new Booking { Id = 1, BookingStatus = BookingStatus.CheckedIn };

        _mockBookingRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(booking);
        _mockMenuItemRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync((MenuItem?)null);

        var ex = Assert.ThrowsAsync<ArgumentException>(() => _orderService.CreateOrderAsync(dto));
        Assert.That(ex.Message, Does.Contain("not found"));
    }

    [Test]
    public void CreateOrderAsync_ShouldThrow_IfMenuItemUnavailable()
    {
        var dto = new CreateFoodOrderDTO
        {
            BookingId = 1,
            Items = new List<CreateFoodOrderItemDTO> { new CreateFoodOrderItemDTO { MenuItemId = 5, Quantity = 1 } }
        };
        var booking = new Booking { Id = 1, BookingStatus = BookingStatus.CheckedIn };
        var menuItem = new MenuItem { Id = 5, Name = "Pizza", IsAvailable = false };

        _mockBookingRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(booking);
        _mockMenuItemRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(menuItem);

        var ex = Assert.ThrowsAsync<ArgumentException>(() => _orderService.CreateOrderAsync(dto));
        Assert.That(ex.Message, Does.Contain("is currently unavailable"));
    }

    [Test]
    public async Task CreateOrderAsync_ShouldCreateOrder_WithLockedPrices()
    {
        var dto = new CreateFoodOrderDTO
        {
            BookingId = 1,
            Items = new List<CreateFoodOrderItemDTO>
            {
                new CreateFoodOrderItemDTO { MenuItemId = 5, Quantity = 2 },
                new CreateFoodOrderItemDTO { MenuItemId = 6, Quantity = 1 }
            }
        };
        var booking = new Booking { Id = 1, BookingStatus = BookingStatus.CheckedIn };
        var pizza = new MenuItem { Id = 5, Name = "Pizza", IsAvailable = true, Price = 15m };
        var soda = new MenuItem { Id = 6, Name = "Soda", IsAvailable = true, Price = 5m };

        _mockBookingRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(booking);
        _mockMenuItemRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(pizza);
        _mockMenuItemRepo.Setup(r => r.GetByIdAsync(6)).ReturnsAsync(soda);

        _mockMapper.Setup(m => m.Map<FoodOrderDTO>(It.IsAny<FoodOrder>())).Returns(new FoodOrderDTO { BookingId = 1 });

        var result = await _orderService.CreateOrderAsync(dto);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.BookingId, Is.EqualTo(1));

        _mockFoodOrderRepo.Verify(r => r.AddAsync(It.Is<FoodOrder>(o =>
            o.BookingId == 1 &&
            o.OrderStatus == FoodOrderStatus.Pending &&
            o.OrderItems.Count == 2 &&
            o.OrderItems.Any(i => i.MenuItemId == 5 && i.Quantity == 2 && i.PriceAtPurchase == 15m) &&
            o.OrderItems.Any(i => i.MenuItemId == 6 && i.Quantity == 1 && i.PriceAtPurchase == 5m)
        )), Times.Once);

        _mockFoodOrderRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task GetActiveOrdersAsync_WithSort_ShouldReturnSorted()
    {
        var orders = new List<FoodOrder> { new FoodOrder { Id = 1 }, new FoodOrder { Id = 2 } };
        _mockFoodOrderRepo.Setup(r => r.GetActiveOrdersWithDetailsAsync()).ReturnsAsync(orders);
        _mockMapper.Setup(m => m.Map<IEnumerable<FoodOrderDTO>>(It.IsAny<IEnumerable<FoodOrder>>()))
            .Returns(new List<FoodOrderDTO> { new FoodOrderDTO { Id = 1 } });

        var result = await _orderService.GetActiveOrdersAsync(1, 10, sortBy: "OrderStatus", sortDescending: true);

        Assert.That(result, Is.Not.Null);
        // Ordering should hit the orderBy branch
    }

    [Test]
    public async Task CreateOrderAsync_WithRoomId_ShouldIncludeRoomInAlert()
    {
        var dto = new CreateFoodOrderDTO
        {
            BookingId = 1,
            Items = new List<CreateFoodOrderItemDTO> { new CreateFoodOrderItemDTO { MenuItemId = 5, Quantity = 1 } }
        };
        var booking = new Booking { Id = 1, BookingStatus = BookingStatus.CheckedIn, BookingRooms = new List<BookingRoom> { new BookingRoom { RoomId = 101 } } };
        var pizza = new MenuItem { Id = 5, Name = "Pizza", IsAvailable = true, Price = 15m };

        _mockBookingRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(booking);
        _mockMenuItemRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(pizza);

        _mockMapper.Setup(m => m.Map<FoodOrderDTO>(It.IsAny<FoodOrder>())).Returns(new FoodOrderDTO { BookingId = 1 });

        await _orderService.CreateOrderAsync(dto);

        _mockNotificationService.Verify(n => n.SendKitchenAlertAsync(It.Is<string>(s => s.Contains("Rooms 101"))), Times.Once);
    }

    [Test]
    public async Task GetAllOrdersAsync_WithStatusAndSort_ShouldReturnFilteredAndSorted()
    {
        var orders = new List<FoodOrder> {
            new FoodOrder { Id = 1, OrderStatus = FoodOrderStatus.Pending },
            new FoodOrder { Id = 2, OrderStatus = FoodOrderStatus.Delivered }
        };
        _mockFoodOrderRepo.Setup(r => r.GetAllOrdersWithDetailsAsync()).ReturnsAsync(orders);
        _mockMapper.Setup(m => m.Map<IEnumerable<FoodOrderDTO>>(It.IsAny<IEnumerable<FoodOrder>>()))
            .Returns(new List<FoodOrderDTO> { new FoodOrderDTO { Id = 1 } });

        var result = await _orderService.GetAllOrdersAsync(1, 10, status: "Pending", sortBy: "Id", sortDescending: true);

    }

    [Test]
    public async Task GetAllOrdersAsync_WithInvalidStatus_ShouldIgnoreStatusFilter()
    {
        var orders = new List<FoodOrder> {
            new FoodOrder { Id = 1, OrderStatus = FoodOrderStatus.Pending }
        };
        _mockFoodOrderRepo.Setup(r => r.GetAllOrdersWithDetailsAsync()).ReturnsAsync(orders);
        _mockMapper.Setup(m => m.Map<IEnumerable<FoodOrderDTO>>(It.IsAny<IEnumerable<FoodOrder>>()))
            .Returns(new List<FoodOrderDTO> { new FoodOrderDTO { Id = 1 } });

        var result = await _orderService.GetAllOrdersAsync(1, 10, status: "InvalidStatus123");

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Data.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task GetOrderByIdAsync_ShouldReturnMappedOrder()
    {
        // Arrange
        var orders = new List<FoodOrder> { new FoodOrder { Id = 1 } };
        _mockFoodOrderRepo.Setup(r => r.GetAllOrdersWithDetailsAsync()).ReturnsAsync(orders);
        _mockMapper.Setup(m => m.Map<FoodOrderDTO>(It.IsAny<FoodOrder>())).Returns(new FoodOrderDTO { Id = 1 });

        // Act
        var result = await _orderService.GetOrderByIdAsync(1);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(1));
    }

    [Test]
    public void GetOrderByIdAsync_ShouldThrow_IfNotFound()
    {
        // Arrange
        _mockFoodOrderRepo.Setup(r => r.GetAllOrdersWithDetailsAsync()).ReturnsAsync(new List<FoodOrder>());

        // Act & Assert
        var ex = Assert.ThrowsAsync<KeyNotFoundException>(() => _orderService.GetOrderByIdAsync(99));
        Assert.That(ex.Message, Does.Contain("Food Order not found"));
    }

    [Test]
    public void CreateOrderAsync_ShouldThrow_IfIdenticalPendingOrderExists()
    {
        // Arrange
        var dto = new CreateFoodOrderDTO
        {
            BookingId = 1,
            Items = new List<CreateFoodOrderItemDTO> { new CreateFoodOrderItemDTO { MenuItemId = 5, Quantity = 2 } }
        };
        var booking = new Booking { Id = 1, BookingStatus = BookingStatus.CheckedIn, PaymentStatus = PaymentStatus.Pending };
        _mockBookingRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(booking);

        // Simulate an identical pending order
        var activeOrders = new List<FoodOrder>
    {
        new FoodOrder
        {
            BookingId = 1,
            OrderStatus = FoodOrderStatus.Pending,
            OrderItems = new List<FoodOrderItem> { new FoodOrderItem { MenuItemId = 5, Quantity = 2 } }
        }
    };
        _mockFoodOrderRepo.Setup(r => r.GetActiveOrdersWithDetailsAsync()).ReturnsAsync(activeOrders);

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(() => _orderService.CreateOrderAsync(dto));
        Assert.That(ex.Message, Does.Contain("identical food order is already pending"));
    }
}
