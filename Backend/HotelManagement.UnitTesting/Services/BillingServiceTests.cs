using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq.Expressions;
using AutoMapper;
using HotelManagement.BLL.Interfaces;
using HotelManagement.BLL.Services;
using HotelManagement.BLL.DTOs;
using HotelManagement.DAL.Entities;
using HotelManagement.DAL.Enums;
using HotelManagement.Repository.Interfaces;
using Moq;
using NUnit.Framework;

namespace HotelManagement.UnitTesting.Services;

[TestFixture]
public class BillingServiceTests
{
    private Mock<IBookingRepository> _mockBookingRepo;
    private Mock<IReceiptRepository> _mockReceiptRepo;
    private Mock<IMapper> _mockMapper;
    private Mock<ICurrentUserService> _mockCurrentUserService;
    private Mock<IUserRepository> _mockUserRepo;
    private BillingService _billingService;

    [SetUp]
    public void Setup()
    {
        _mockBookingRepo = new Mock<IBookingRepository>();
        _mockReceiptRepo = new Mock<IReceiptRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mockUserRepo = new Mock<IUserRepository>();

        _mockCurrentUserService.Setup(s => s.IsInRole(It.IsAny<string>())).Returns(true); // default to staff

        _billingService = new BillingService(_mockBookingRepo.Object, _mockReceiptRepo.Object, _mockMapper.Object, _mockCurrentUserService.Object, _mockUserRepo.Object);
    }

    [Test]
    public void GenerateFolioAsync_ShouldThrow_IfBookingNotFound()
    {
        _mockBookingRepo.Setup(r => r.GetBookingWithDetailsAsync(99)).ReturnsAsync((Booking?)null);
        var ex = Assert.ThrowsAsync<KeyNotFoundException>(() => _billingService.GenerateFolioAsync(99));
        Assert.That(ex.Message, Does.Contain("Booking not found"));
    }

    [Test]
    public async Task GenerateFolioAsync_ShouldCalculateNights_Min1()
    {
        var booking = new Booking
        {
            Id = 1,
            CheckInDate = DateTime.UtcNow,
            CheckOutDate = DateTime.UtcNow, // 0 days
            BookingRooms = new List<BookingRoom> { new BookingRoom { LockedInPrice = 100m } },
            FoodOrders = new List<FoodOrder>()
        };
        _mockBookingRepo.Setup(r => r.GetBookingWithDetailsAsync(1)).ReturnsAsync(booking);

        var folio = await _billingService.GenerateFolioAsync(1);

        Assert.That(folio.NightsStayed, Is.EqualTo(1));
        Assert.That(folio.RoomBasePrice, Is.EqualTo(100m));
        Assert.That(folio.FoodTotal, Is.EqualTo(0m));
        Assert.That(folio.AmenityTotal, Is.EqualTo(0));
        Assert.That(folio.TotalBill, Is.EqualTo(100));
        Assert.That(folio.PaymentStatus, Is.EqualTo(PaymentStatus.Pending.ToString()));
    }

    [Test]
    public async Task GenerateFolioAsync_ShouldUseRoomPrice_IfLockedPriceIsZero()
    {
        var booking = new Booking
        {
            Id = 1,
            CheckInDate = DateTime.UtcNow,
            CheckOutDate = DateTime.UtcNow.AddDays(3),
            BookingRooms = new List<BookingRoom> { new BookingRoom { LockedInPrice = 0m, Room = new Room { RoomType = new RoomType { BasePrice = 150m } } } },
            FoodOrders = new List<FoodOrder>
            {
                new FoodOrder { OrderItems = new List<FoodOrderItem> { new FoodOrderItem { Quantity = 2, PriceAtPurchase = 20m, MenuItem = new MenuItem { Name = "Burger" } }, new FoodOrderItem { Quantity = 1, PriceAtPurchase = 30m, MenuItem = new MenuItem { Name = "Fries" } } } }
            }
        };
        _mockBookingRepo.Setup(r => r.GetBookingWithDetailsAsync(1)).ReturnsAsync(booking);

        var folio = await _billingService.GenerateFolioAsync(1);

        Assert.That(folio.NightsStayed, Is.EqualTo(3));
        Assert.That(folio.RoomBasePrice, Is.EqualTo(150m));
        Assert.That(folio.FoodTotal, Is.EqualTo(70)); // (2*20) + (1*30)
        Assert.That(folio.AmenityTotal, Is.EqualTo(0));
        Assert.That(folio.TotalBill, Is.EqualTo(450 + 70));
    }

    [Test]
    public async Task GenerateFolioAsync_ShouldUseZeroPrice_IfNoLockedOrRoomPrice()
    {
        var booking = new Booking
        {
            Id = 1,
            CheckInDate = DateTime.UtcNow,
            CheckOutDate = DateTime.UtcNow.AddDays(5),
            BookingRooms = new List<BookingRoom> { new BookingRoom { LockedInPrice = 0m, Room = null } }, // No room assigned
            FoodOrders = new List<FoodOrder>()
        };
        _mockBookingRepo.Setup(r => r.GetBookingWithDetailsAsync(1)).ReturnsAsync(booking);

        var folio = await _billingService.GenerateFolioAsync(1);

        Assert.That(folio.RoomBasePrice, Is.EqualTo(0m));
    }

    [Test]
    public async Task GenerateFolioAsync_ShouldUseZeroPrice_IfRoomTypeIsNull()
    {
        var booking = new Booking
        {
            Id = 1,
            CheckInDate = DateTime.UtcNow,
            CheckOutDate = DateTime.UtcNow.AddDays(5),
            BookingRooms = new List<BookingRoom> { new BookingRoom { LockedInPrice = 0m, Room = new Room { RoomType = null! } } }, // Room exists, but RoomType is null
            FoodOrders = new List<FoodOrder>()
        };
        _mockBookingRepo.Setup(r => r.GetBookingWithDetailsAsync(1)).ReturnsAsync(booking);

        var folio = await _billingService.GenerateFolioAsync(1);

        Assert.That(folio.RoomBasePrice, Is.EqualTo(0m));
    }

    [Test]
    public async Task GenerateFolioAsync_ShouldSumFoodOrders()
    {
        var booking = new Booking
        {
            Id = 1,
            CheckInDate = DateTime.UtcNow,
            CheckOutDate = DateTime.UtcNow.AddDays(1),
            BookingRooms = new List<BookingRoom> { new BookingRoom { LockedInPrice = 100m } },
            FoodOrders = new List<FoodOrder>
            {
                new FoodOrder
                {
                    OrderItems = new List<FoodOrderItem>
                    {
                        new FoodOrderItem { Quantity = 2, PriceAtPurchase = 10m, MenuItem = new MenuItem { Name = "Burger" } },
                        new FoodOrderItem { Quantity = 1, PriceAtPurchase = 5m, MenuItem = new MenuItem { Name = "Fries" } }
                    }
                }
            }
        };
        _mockBookingRepo.Setup(r => r.GetBookingWithDetailsAsync(1)).ReturnsAsync(booking);

        var folio = await _billingService.GenerateFolioAsync(1);

        Assert.That(folio.FoodTotal, Is.EqualTo(25m)); // (2 * 10) + (1 * 5)
        Assert.That(folio.FoodItems.Count, Is.EqualTo(2));
        Assert.That(folio.FoodItems[0], Is.EqualTo("2x Burger"));
        Assert.That(folio.FoodItems[1], Is.EqualTo("1x Fries"));
    }

    [Test]
    public async Task GenerateFolioAsync_ShouldIncludeAmenityOrders()
    {
        // Arrange
        var booking = new Booking
        {
            Id = 1,
            CheckInDate = DateTime.UtcNow.AddDays(-1),
            CheckOutDate = DateTime.UtcNow,
            BookingStatus = BookingStatus.CheckedOut,
            PaymentStatus = PaymentStatus.Pending,
            BookingRooms = new List<BookingRoom> { new BookingRoom { LockedInPrice = 100, Room = new Room { RoomNumber = "101" } } },
            BookingAmenities = new List<BookingAmenity>
            {
                new BookingAmenity { Amenity = new Amenity { Name = "Pool" }, PriceAtPurchase = 25 },
                new BookingAmenity { Amenity = new Amenity { Name = "Gym" }, PriceAtPurchase = 10 }
            }
        };

        _mockBookingRepo.Setup(r => r.GetBookingWithDetailsAsync(1)).ReturnsAsync(booking);

        // Act
        var folio = await _billingService.GenerateFolioAsync(1);

        // Assert
        Assert.That(folio.RoomTotal, Is.EqualTo(100)); // 1 night * 100
        Assert.That(folio.AmenityTotal, Is.EqualTo(35)); // 25 + 10
        Assert.That(folio.TotalBill, Is.EqualTo(135)); // 100 + 35
        Assert.That(folio.AmenityItems.Count, Is.EqualTo(2));
        Assert.That(folio.AmenityItems, Contains.Item("Pool"));
    }

    [Test]
    public async Task GetGlobalBillingAsync_ShouldReturnAggregatedMetrics()
    {
        var receipts = new List<Receipt>
        {
            new Receipt { AmountPaid = 100m },
            new Receipt { AmountPaid = 50m }
        };
        var bookings = new List<Booking>
        {
            new Booking { Id = 1, PaymentStatus = PaymentStatus.Pending, BookingRooms = new List<BookingRoom> { new BookingRoom { LockedInPrice = 200m } }, CheckInDate = DateTime.UtcNow, CheckOutDate = DateTime.UtcNow.AddDays(1) },
            new Booking { Id = 2, PaymentStatus = PaymentStatus.Paid, BookingRooms = new List<BookingRoom> { new BookingRoom { LockedInPrice = 100m } }, CheckInDate = DateTime.UtcNow, CheckOutDate = DateTime.UtcNow.AddDays(1) }
        };

        var pagedResult = new HotelManagement.Repository.Models.PaginatedResult<Booking> { Data = bookings };
        _mockBookingRepo.Setup(r => r.GetPaginatedResultAsync(1, int.MaxValue, null)).ReturnsAsync(pagedResult);

        var result = await _billingService.GetGlobalBillingAsync("Pending", DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(2), "John", false, 1, 10);

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task GetGlobalBillingAsync_ShouldReturnDetailedFolios()
    {
        var bookings = new List<Booking>
        {
            new Booking { Id = 1, PaymentStatus = PaymentStatus.Pending, BookingRooms = new List<BookingRoom> { new BookingRoom { LockedInPrice = 200m } }, CheckInDate = DateTime.UtcNow, CheckOutDate = DateTime.UtcNow.AddDays(1), FoodOrders = new List<FoodOrder>(), BookingAmenities = new List<BookingAmenity>() }
        };

        var pagedResult = new HotelManagement.Repository.Models.PaginatedResult<Booking> { Data = bookings };
        _mockBookingRepo.Setup(r => r.GetPaginatedResultAsync(1, int.MaxValue, null)).ReturnsAsync(pagedResult);
        _mockBookingRepo.Setup(r => r.GetBookingWithDetailsAsync(1)).ReturnsAsync(bookings[0]);

        var result = await _billingService.GetGlobalBillingAsync(null, null, null, null, true, 1, 10);

        Assert.That(result, Is.TypeOf<HotelManagement.Repository.Models.PaginatedResult<HotelManagement.BLL.DTOs.BillingFolioDTO>>());
    }

    [Test]
    public void ProcessPaymentAsync_ShouldThrow_IfBookingNotFound()
    {
        _mockBookingRepo.Setup(r => r.GetBookingWithDetailsAsync(99)).ReturnsAsync((Booking?)null);
        var ex = Assert.ThrowsAsync<KeyNotFoundException>(() => _billingService.ProcessPaymentAsync(99, new HotelManagement.BLL.DTOs.PaymentRequestDTO { Amount = 100m }));
        Assert.That(ex.Message, Does.Contain("Booking not found"));
    }

    [Test]
    public void ProcessPaymentAsync_ShouldThrow_IfAlreadyPaid()
    {
        var booking = new Booking { Id = 1, PaymentStatus = PaymentStatus.Paid };
        _mockBookingRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(booking);
        var ex = Assert.ThrowsAsync<InvalidOperationException>(() => _billingService.ProcessPaymentAsync(1, new HotelManagement.BLL.DTOs.PaymentRequestDTO { Amount = 100m }));
        Assert.That(ex.Message, Does.Contain("has already been paid"));
    }

    [Test]
    public async Task ProcessPaymentAsync_ShouldProcessPaymentSuccessfully()
    {
        var booking = new Booking
        {
            Id = 1,
            PaymentStatus = PaymentStatus.Pending,
            BookingRooms = new List<BookingRoom> { new BookingRoom { LockedInPrice = 100m } },
            CheckInDate = DateTime.UtcNow,
            CheckOutDate = DateTime.UtcNow.AddDays(1),
            FoodOrders = new List<FoodOrder>(),
            BookingAmenities = new List<BookingAmenity>()
        };
        _mockBookingRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(booking);
        _mockBookingRepo.Setup(r => r.GetBookingWithDetailsAsync(1)).ReturnsAsync(booking);

        await _billingService.ProcessPaymentAsync(1, new HotelManagement.BLL.DTOs.PaymentRequestDTO { Amount = 100m, PaymentMethod = "Card", TransactionId = "123" });

        Assert.That(booking.PaymentStatus, Is.EqualTo(PaymentStatus.Paid));
        _mockBookingRepo.Verify(r => r.Update(booking), Times.Once);
        _mockBookingRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        _mockReceiptRepo.Verify(r => r.AddAsync(It.IsAny<Receipt>()), Times.Once);
        _mockReceiptRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void ProcessPaymentAsync_ShouldThrow_IfAmountMismatch()
    {
        var booking = new Booking
        {
            Id = 1,
            PaymentStatus = PaymentStatus.Pending,
            BookingRooms = new List<BookingRoom> { new BookingRoom { LockedInPrice = 100m } },
            CheckInDate = DateTime.UtcNow,
            CheckOutDate = DateTime.UtcNow.AddDays(1),
            FoodOrders = new List<FoodOrder>(),
            BookingAmenities = new List<BookingAmenity>()
        };
        _mockBookingRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(booking);
        _mockBookingRepo.Setup(r => r.GetBookingWithDetailsAsync(1)).ReturnsAsync(booking);

        var ex = Assert.ThrowsAsync<InvalidOperationException>(() => _billingService.ProcessPaymentAsync(1, new HotelManagement.BLL.DTOs.PaymentRequestDTO { Amount = 50m, PaymentMethod = "Card", TransactionId = "123" }));
        Assert.That(ex.Message, Does.Contain("Payment amount (50) does not match the total amount due"));
    }

    [Test]
    public async Task GetGlobalBillingAsync_ShouldApplySortAndReturnAggregatedMetrics()
    {
        var bookings = new List<Booking>
        {
            new Booking { Id = 1, GuestName = "Alice", PaymentStatus = PaymentStatus.Pending, BookingRooms = new List<BookingRoom> { new BookingRoom { LockedInPrice = 200m } }, CheckInDate = DateTime.UtcNow, CheckOutDate = DateTime.UtcNow.AddDays(1) },
            new Booking { Id = 2, GuestName = "Bob", BookingRooms = new List<BookingRoom> { new BookingRoom { Room = new Room { RoomNumber = "101" }, LockedInPrice = 100m } }, PaymentStatus = PaymentStatus.Paid, CheckInDate = DateTime.UtcNow, CheckOutDate = DateTime.UtcNow.AddDays(1) }
        };

        var pagedResult = new HotelManagement.Repository.Models.PaginatedResult<Booking> { Data = bookings };
        _mockBookingRepo.Setup(r => r.GetPaginatedResultAsync(1, int.MaxValue, null)).ReturnsAsync(pagedResult);

        var result = await _billingService.GetGlobalBillingAsync("Pending", DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(2), "101", false, 1, 10, "GuestName", true);

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task GetReceiptsAsync_ShouldReturnMappedReceipts()
    {
        var receipts = new List<Receipt> { new Receipt { Id = 1, AmountPaid = 100m } };
        var pagedResult = new HotelManagement.Repository.Models.PaginatedResult<Receipt> { Data = receipts, TotalCount = 1, PageNumber = 1, PageSize = 10 };
        _mockReceiptRepo.Setup(r => r.GetPaginatedResultAsync(1, 10, null, It.IsAny<Func<System.Linq.IQueryable<Receipt>, System.Linq.IOrderedQueryable<Receipt>>>())).ReturnsAsync(pagedResult);
        _mockMapper.Setup(m => m.Map<IEnumerable<ReceiptDTO>>(receipts)).Returns(new List<ReceiptDTO> { new ReceiptDTO { Id = 1, AmountPaid = 100m } });

        var result = await _billingService.GetReceiptsAsync(null, null, 1, 10, "AmountPaid", true);
        //         Assert.That(result.Data, Is.Not.Empty);
        Assert.That(result.Data, Is.Not.Empty);
        Assert.That(result.Data.First().AmountPaid, Is.EqualTo(100m));
    }

    [Test]
    public async Task GenerateFolioAsync_ShouldReturnRefunded_IfCancelledAndPaid()
    {
        var booking = new Booking
        {
            Id = 1,
            CheckInDate = DateTime.UtcNow,
            CheckOutDate = DateTime.UtcNow.AddDays(1),
            BookingStatus = BookingStatus.Cancelled,
            PaymentStatus = PaymentStatus.Paid,
            BookingRooms = new List<BookingRoom>(),
            FoodOrders = new List<FoodOrder>(),
            BookingAmenities = new List<BookingAmenity>()
        };
        _mockBookingRepo.Setup(r => r.GetBookingWithDetailsAsync(1)).ReturnsAsync(booking);

        var folio = await _billingService.GenerateFolioAsync(1);

        Assert.That(folio.PaymentStatus, Is.EqualTo("Refunded"));
    }

    [Test]
    public async Task GenerateFolioAsync_ShouldReturnCancelled_IfCancelledAndNotPaid()
    {
        var booking = new Booking
        {
            Id = 1,
            CheckInDate = DateTime.UtcNow,
            CheckOutDate = DateTime.UtcNow.AddDays(1),
            BookingStatus = BookingStatus.Cancelled,
            PaymentStatus = PaymentStatus.Pending,
            BookingRooms = new List<BookingRoom>(),
            FoodOrders = new List<FoodOrder>(),
            BookingAmenities = new List<BookingAmenity>()
        };
        _mockBookingRepo.Setup(r => r.GetBookingWithDetailsAsync(1)).ReturnsAsync(booking);

        var folio = await _billingService.GenerateFolioAsync(1);

        Assert.That(folio.PaymentStatus, Is.EqualTo("Cancelled"));
    }



    [Test]
    public async Task GetGlobalBillingAsync_DetailedFalse_ReturnsDashboardDTOs()
    {
        var bookings = new List<Booking>
    {
        new Booking { Id = 1, CheckInDate = DateTime.Today, CheckOutDate = DateTime.Today.AddDays(2), BookingRooms = new List<BookingRoom> { new BookingRoom { LockedInPrice = 100, Room = null } } },
        new Booking { Id = 2, CheckInDate = DateTime.Today, CheckOutDate = DateTime.Today.AddDays(2), BookingRooms = new List<BookingRoom> { new BookingRoom { LockedInPrice = 100, Room = new Room { RoomNumber = "101" } } } }
    };
        var pagedResult = new HotelManagement.Repository.Models.PaginatedResult<Booking> { Data = bookings, TotalCount = 2 };
        _mockBookingRepo.Setup(r => r.GetPaginatedResultAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Expression<Func<Booking, bool>>>(), It.IsAny<Func<IQueryable<Booking>, IOrderedQueryable<Booking>>>())).ReturnsAsync(pagedResult);

        var result = (HotelManagement.Repository.Models.PaginatedResult<BillingDashboardDTO>)await _billingService.GetGlobalBillingAsync(null, null, null, null, false, 1, 10);

        Assert.That(result.Data.Count(), Is.EqualTo(2));
        var list = result.Data.ToList();

        // Check that the IDs mapped correctly
        Assert.That(list[0].BookingId, Is.EqualTo(1));
        Assert.That(list[1].BookingId, Is.EqualTo(2));

        // Check that the BaseRoomTotal was calculated correctly (100 price * 2 days = 200)
        Assert.That(list[0].BaseRoomTotal, Is.EqualTo(200m));
        Assert.That(list[1].BaseRoomTotal, Is.EqualTo(200m));
    }

    [Test]
    public async Task GetReceiptsAsync_WithNoSort_ReturnsMappedReceipts()
    {
        var receipts = new List<Receipt> { new Receipt { Id = 1, AmountPaid = 100m } };
        var pagedResult = new HotelManagement.Repository.Models.PaginatedResult<Receipt> { Data = receipts, TotalCount = 1, PageNumber = 1, PageSize = 10 };
        _mockReceiptRepo.Setup(r => r.GetPaginatedResultAsync(1, 10, null, null)).ReturnsAsync(pagedResult);
        _mockMapper.Setup(m => m.Map<IEnumerable<ReceiptDTO>>(receipts)).Returns(new List<ReceiptDTO> { new ReceiptDTO { Id = 1, AmountPaid = 100m } });

        var result = await _billingService.GetReceiptsAsync(null, null, 1, 10, null, false);

        Assert.That(result.Data, Is.Not.Empty);
    }

    [Test]
    public async Task GetGlobalBillingAsync_InvalidPaymentStatus_IgnoresFilter()
    {
        var bookings = new List<Booking>
        {
            new Booking { Id = 1, PaymentStatus = PaymentStatus.Pending, CheckInDate = DateTime.UtcNow, CheckOutDate = DateTime.UtcNow.AddDays(1) }
        };
        var pagedResult = new HotelManagement.Repository.Models.PaginatedResult<Booking> { Data = bookings };
        _mockBookingRepo.Setup(r => r.GetPaginatedResultAsync(1, int.MaxValue, null)).ReturnsAsync(pagedResult);

        var result = await _billingService.GetGlobalBillingAsync("InvalidStatus123", null, null, null, false, 1, 10);

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task GetGlobalBillingAsync_ShouldFilterByGuestName()
    {
        var bookings = new List<Booking>
        {
            new Booking { Id = 1, GuestName = "Alice", CheckInDate = DateTime.UtcNow, CheckOutDate = DateTime.UtcNow.AddDays(1) },
            new Booking { Id = 2, GuestName = "Bob", CheckInDate = DateTime.UtcNow, CheckOutDate = DateTime.UtcNow.AddDays(1) }
        };
        var pagedResult = new HotelManagement.Repository.Models.PaginatedResult<Booking> { Data = bookings };
        _mockBookingRepo.Setup(r => r.GetPaginatedResultAsync(1, int.MaxValue, null)).ReturnsAsync(pagedResult);

        var result = (HotelManagement.Repository.Models.PaginatedResult<BillingDashboardDTO>)await _billingService.GetGlobalBillingAsync(null, null, null, "Alice", false, 1, 10);

        var list = result.Data.ToList();
        Assert.That(list.Count, Is.EqualTo(1));
        Assert.That(list[0].GuestName, Is.EqualTo("Alice"));
    }

    [Test]
    public async Task GenerateFolioAsync_NoFoodOrAmenities_ShouldReturnEmptyLists()
    {
        var booking = new Booking
        {
            Id = 1,
            CheckInDate = DateTime.UtcNow,
            CheckOutDate = DateTime.UtcNow.AddDays(1),
            BookingRooms = new List<BookingRoom> { new BookingRoom { LockedInPrice = 100m } },
            FoodOrders = new List<FoodOrder>(),
            BookingAmenities = new List<BookingAmenity>()
        };
        _mockBookingRepo.Setup(r => r.GetBookingWithDetailsAsync(1)).ReturnsAsync(booking);

        var folio = await _billingService.GenerateFolioAsync(1);

        Assert.That(folio.FoodItems, Is.Empty);
        Assert.That(folio.AmenityItems, Is.Empty);
    }

    [Test]
    public void ProcessPaymentAsync_ShouldThrow_IfAmountIsZeroOrLess()
    {
        var requestDto = new HotelManagement.BLL.DTOs.PaymentRequestDTO { Amount = 0 };
        var ex = Assert.ThrowsAsync<ArgumentException>(() => _billingService.ProcessPaymentAsync(1, requestDto));
        Assert.That(ex.Message, Does.Contain("must be greater than zero"));
    }

    [Test]
    public async Task GetGlobalBillingAsync_ShouldFilterByRoomNumber()
    {
        var bookings = new List<Booking>
    {
        new Booking { Id = 1, GuestName = "Alice", BookingRooms = new List<BookingRoom> { new BookingRoom { Room = new Room { RoomNumber = "101" } } }, CheckInDate = DateTime.UtcNow, CheckOutDate = DateTime.UtcNow.AddDays(1) },
        new Booking { Id = 2, GuestName = "Bob", BookingRooms = new List<BookingRoom> { new BookingRoom { Room = new Room { RoomNumber = "202" } } }, CheckInDate = DateTime.UtcNow, CheckOutDate = DateTime.UtcNow.AddDays(1) }
    };
        var pagedResult = new HotelManagement.Repository.Models.PaginatedResult<Booking> { Data = bookings };
        _mockBookingRepo.Setup(r => r.GetPaginatedResultAsync(1, int.MaxValue, null)).ReturnsAsync(pagedResult);

        var result = (HotelManagement.Repository.Models.PaginatedResult<BillingDashboardDTO>)await _billingService.GetGlobalBillingAsync(null, null, null, "202", false, 1, 10);

        var list = result.Data.ToList();
        Assert.That(list.Count, Is.EqualTo(1));
        Assert.That(list[0].GuestName, Is.EqualTo("Bob"));
    }

    [Test]
    public async Task GetGlobalBillingAsync_ShouldSortByTotalBill_AscendingAndDescending()
    {
        var bookings = new List<Booking> { new Booking { Id = 1, BookingRooms = new List<BookingRoom>(), FoodOrders = new List<FoodOrder>(), BookingAmenities = new List<BookingAmenity>() } };
        var pagedResult = new HotelManagement.Repository.Models.PaginatedResult<Booking> { Data = bookings };
        _mockBookingRepo.Setup(r => r.GetPaginatedResultAsync(1, int.MaxValue, null)).ReturnsAsync(pagedResult);

        // Act - Ascending
        var resultAsc = await _billingService.GetGlobalBillingAsync(null, null, null, null, false, 1, 10, "totalbill", false);
        // Act - Descending
        var resultDesc = await _billingService.GetGlobalBillingAsync(null, null, null, null, false, 1, 10, "totalbill", true);

        Assert.That(resultAsc, Is.Not.Null);
        Assert.That(resultDesc, Is.Not.Null);
    }

    [Test]
    public async Task GetReceiptsAsync_ShouldFilterByBothDates()
    {
        var pagedResult = new HotelManagement.Repository.Models.PaginatedResult<Receipt> { Data = new List<Receipt>(), TotalCount = 0 };
        _mockReceiptRepo.Setup(r => r.GetPaginatedResultAsync(1, 10, It.IsAny<Expression<Func<Receipt, bool>>>(), null)).ReturnsAsync(pagedResult);
        _mockMapper.Setup(m => m.Map<IEnumerable<ReceiptDTO>>(It.IsAny<IEnumerable<Receipt>>())).Returns(new List<ReceiptDTO>());

        await _billingService.GetReceiptsAsync(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1), 1, 10);

        _mockReceiptRepo.Verify(r => r.GetPaginatedResultAsync(1, 10, It.IsNotNull<Expression<Func<Receipt, bool>>>(), null), Times.Once);
    }

    [Test]
    public async Task GetReceiptsAsync_ShouldFilterByStartDateOnly()
    {
        var pagedResult = new HotelManagement.Repository.Models.PaginatedResult<Receipt> { Data = new List<Receipt>(), TotalCount = 0 };
        _mockReceiptRepo.Setup(r => r.GetPaginatedResultAsync(1, 10, It.IsAny<Expression<Func<Receipt, bool>>>(), null)).ReturnsAsync(pagedResult);
        _mockMapper.Setup(m => m.Map<IEnumerable<ReceiptDTO>>(It.IsAny<IEnumerable<Receipt>>())).Returns(new List<ReceiptDTO>());

        await _billingService.GetReceiptsAsync(DateTime.UtcNow.AddDays(-1), null, 1, 10);

        _mockReceiptRepo.Verify(r => r.GetPaginatedResultAsync(1, 10, It.IsNotNull<Expression<Func<Receipt, bool>>>(), null), Times.Once);
    }

    [Test]
    public async Task GetReceiptsAsync_ShouldFilterByEndDateOnly()
    {
        var pagedResult = new HotelManagement.Repository.Models.PaginatedResult<Receipt> { Data = new List<Receipt>(), TotalCount = 0 };
        _mockReceiptRepo.Setup(r => r.GetPaginatedResultAsync(1, 10, It.IsAny<Expression<Func<Receipt, bool>>>(), null)).ReturnsAsync(pagedResult);
        _mockMapper.Setup(m => m.Map<IEnumerable<ReceiptDTO>>(It.IsAny<IEnumerable<Receipt>>())).Returns(new List<ReceiptDTO>());

        await _billingService.GetReceiptsAsync(null, DateTime.UtcNow.AddDays(1), 1, 10);

        _mockReceiptRepo.Verify(r => r.GetPaginatedResultAsync(1, 10, It.IsNotNull<Expression<Func<Receipt, bool>>>(), null), Times.Once);
    }

    [Test]
    public async Task GetReceiptByIdAsync_ShouldReturnReceipt_WhenExists()
    {
        var receipt = new Receipt { Id = 1 };
        _mockReceiptRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(receipt);
        _mockMapper.Setup(m => m.Map<ReceiptDTO>(receipt)).Returns(new ReceiptDTO { Id = 1 });

        var result = await _billingService.GetReceiptByIdAsync(1);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(1));
    }

    [Test]
    public void GetReceiptByIdAsync_ShouldThrow_WhenNotFound()
    {
        _mockReceiptRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Receipt?)null);

        var ex = Assert.ThrowsAsync<KeyNotFoundException>(() => _billingService.GetReceiptByIdAsync(99));
        Assert.That(ex.Message, Does.Contain("Receipt not found"));
    }
}

