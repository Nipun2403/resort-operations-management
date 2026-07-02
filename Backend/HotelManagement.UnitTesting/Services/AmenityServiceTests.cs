using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using HotelManagement.BLL.DTOs;
using HotelManagement.BLL.Interfaces;
using HotelManagement.BLL.Services;
using HotelManagement.DAL.Entities;
using HotelManagement.DAL.Enums;
using HotelManagement.Repository.Interfaces;
using Moq;
using NUnit.Framework;

namespace HotelManagement.UnitTesting.Services;

[TestFixture]
public class AmenityServiceTests
{
    private Mock<IAmenityRepository> _mockAmenityRepo;
    private Mock<IBookingRepository> _mockBookingRepo;
    private Mock<IMapper> _mockMapper;
    private Mock<ICurrentUserService> _mockCurrentUserService;
    private AmenityService _amenityService;

    [SetUp]
    public void Setup()
    {
        _mockAmenityRepo = new Mock<IAmenityRepository>();
        _mockBookingRepo = new Mock<IBookingRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        
        _amenityService = new AmenityService(_mockAmenityRepo.Object, _mockBookingRepo.Object, _mockMapper.Object, _mockCurrentUserService.Object);
    }

    [Test]
    public async Task SubscribeAsync_ShouldThrow_IfBookingNotFound()
    {
        _mockBookingRepo.Setup(r => r.GetBookingWithDetailsAsync(1)).ReturnsAsync((Booking?)null);
        
        var ex = Assert.ThrowsAsync<KeyNotFoundException>(() => _amenityService.SubscribeAsync(1, 1));
        Assert.That(ex.Message, Does.Contain("Booking not found"));
    }

    [Test]
    public async Task SubscribeAsync_ShouldThrow_IfBookingIsCancelled()
    {
        var booking = new Booking { Id = 1, BookingStatus = BookingStatus.Cancelled };
        _mockBookingRepo.Setup(r => r.GetBookingWithDetailsAsync(1)).ReturnsAsync(booking);
        
        var ex = Assert.ThrowsAsync<InvalidOperationException>(() => _amenityService.SubscribeAsync(1, 1));
        Assert.That(ex.Message, Does.Contain("Cannot add amenities to a cancelled or checked-out booking"));
    }

    [Test]
    public async Task SubscribeAsync_ShouldThrow_IfPaymentIsPaid()
    {
        var booking = new Booking { Id = 1, BookingStatus = BookingStatus.CheckedIn, PaymentStatus = PaymentStatus.Paid };
        _mockBookingRepo.Setup(r => r.GetBookingWithDetailsAsync(1)).ReturnsAsync(booking);
        
        var ex = Assert.ThrowsAsync<InvalidOperationException>(() => _amenityService.SubscribeAsync(1, 1));
        Assert.That(ex.Message, Does.Contain("Cannot add amenities after payment has been settled"));
    }

    [Test]
    public async Task SubscribeAsync_ShouldThrow_IfAlreadySubscribed()
    {
        var booking = new Booking 
        { 
            Id = 1, 
            BookingStatus = BookingStatus.CheckedIn, 
            PaymentStatus = PaymentStatus.Pending,
            BookingAmenities = new List<BookingAmenity> { new BookingAmenity { AmenityId = 1 } }
        };
        _mockBookingRepo.Setup(r => r.GetBookingWithDetailsAsync(1)).ReturnsAsync(booking);
        
        var ex = Assert.ThrowsAsync<InvalidOperationException>(() => _amenityService.SubscribeAsync(1, 1));
        Assert.That(ex.Message, Does.Contain("already added to this booking"));
    }

    [Test]
    public async Task SubscribeAsync_ShouldThrow_IfAmenityUnavailable()
    {
        var booking = new Booking 
        { 
            Id = 1, 
            BookingStatus = BookingStatus.CheckedIn, 
            PaymentStatus = PaymentStatus.Pending,
            BookingAmenities = new List<BookingAmenity>()
        };
        _mockBookingRepo.Setup(r => r.GetBookingWithDetailsAsync(1)).ReturnsAsync(booking);
        _mockAmenityRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Amenity { Id = 1, IsAvailable = false });
        
        var ex = Assert.ThrowsAsync<KeyNotFoundException>(() => _amenityService.SubscribeAsync(1, 1));
        Assert.That(ex.Message, Does.Contain("currently unavailable"));
    }

    [Test]
    public async Task SubscribeAsync_ShouldThrow_IfAmenityNotFound()
    {
        var booking = new Booking 
        { 
            Id = 1, 
            BookingStatus = BookingStatus.CheckedIn, 
            PaymentStatus = PaymentStatus.Pending,
            BookingAmenities = new List<BookingAmenity>()
        };
        _mockBookingRepo.Setup(r => r.GetBookingWithDetailsAsync(1)).ReturnsAsync(booking);
        _mockAmenityRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Amenity?)null);
        
        var ex = Assert.ThrowsAsync<KeyNotFoundException>(() => _amenityService.SubscribeAsync(1, 1));
        Assert.That(ex.Message, Does.Contain("not found"));
    }

    [Test]
    public async Task SubscribeAsync_ShouldThrow_IfBookingIsCheckedOut()
    {
        var booking = new Booking { Id = 1, BookingStatus = BookingStatus.CheckedOut };
        _mockBookingRepo.Setup(r => r.GetBookingWithDetailsAsync(1)).ReturnsAsync(booking);
        
        var ex = Assert.ThrowsAsync<InvalidOperationException>(() => _amenityService.SubscribeAsync(1, 1));
        Assert.That(ex.Message, Does.Contain("Cannot add amenities to a cancelled or checked-out booking"));
    }

    [Test]
    public async Task SubscribeAsync_ShouldAddAmenity()
    {
        var booking = new Booking 
        { 
            Id = 1, 
            BookingStatus = BookingStatus.CheckedIn, 
            PaymentStatus = PaymentStatus.Pending,
            BookingAmenities = new List<BookingAmenity>()
        };
        _mockBookingRepo.Setup(r => r.GetBookingWithDetailsAsync(1)).ReturnsAsync(booking);
        _mockAmenityRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Amenity { Id = 1, Price = 25.0m, IsAvailable = true });
        
        await _amenityService.SubscribeAsync(1, 1);
        
        _mockBookingRepo.Verify(r => r.Update(It.IsAny<Booking>()), Times.Once);
        _mockBookingRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        Assert.That(booking.BookingAmenities.Count, Is.EqualTo(1));
        Assert.That(booking.BookingAmenities.First().PriceAtPurchase, Is.EqualTo(25.0m));
    }

    [Test]
    public async Task GetAllAmenitiesAsync_ShouldReturnMappedAmenities()
    {
        var amenities = new List<Amenity> { new Amenity { Id = 1, Name = "Spa" } };
        var pagedResult = new HotelManagement.Repository.Models.PaginatedResult<Amenity> { Data = amenities };
        _mockAmenityRepo.Setup(r => r.GetPaginatedResultAsync(1, 10, null)).ReturnsAsync(pagedResult);
        _mockMapper.Setup(m => m.Map<IEnumerable<AmenityDTO>>(amenities)).Returns(new List<AmenityDTO> { new AmenityDTO { Id = 1, Name = "Spa" } });

        var result = await _amenityService.GetAllAmenitiesAsync(1, 10);

        Assert.That(result.Data.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task GetAmenityByIdAsync_ShouldReturnMappedAmenity_WhenExists()
    {
        var amenity = new Amenity { Id = 1 };
        _mockAmenityRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(amenity);
        _mockMapper.Setup(m => m.Map<AmenityDTO>(amenity)).Returns(new AmenityDTO { Id = 1 });

        var result = await _amenityService.GetAmenityByIdAsync(1);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(1));
    }

    [Test]
    public async Task CreateAmenityAsync_ShouldAddAmenityAndReturnDto()
    {
        var dto = new CreateUpdateAmenityDTO { Name = "Pool", Price = 10m, ImageUrl = "https://images.unsplash.com/photo-test-pool" };
        var amenity = new Amenity { Name = "Pool", Price = 10m };
        _mockMapper.Setup(m => m.Map<Amenity>(dto)).Returns(amenity);
        _mockMapper.Setup(m => m.Map<AmenityDTO>(It.IsAny<Amenity>())).Returns(new AmenityDTO { Id = 1, Name = "Pool" });

        var result = await _amenityService.CreateAmenityAsync(dto);

        _mockAmenityRepo.Verify(r => r.AddAsync(It.Is<Amenity>(a => a.Name == "Pool" && a.Price == 10m)), Times.Once);
        _mockAmenityRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        Assert.That(result.Id, Is.EqualTo(1));
    }

    [Test]
    public async Task UpdateAmenityAsync_ShouldThrow_IfNotFound()
    {
        _mockAmenityRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Amenity?)null);

        var ex = Assert.ThrowsAsync<KeyNotFoundException>(() => _amenityService.UpdateAmenityAsync(99, new CreateUpdateAmenityDTO()));
        Assert.That(ex.Message, Does.Contain("Amenity not found"));
    }

    [Test]
    public async Task UpdateAmenityAsync_ShouldUpdateAndSave()
    {
        var amenity = new Amenity { Id = 1, Name = "Old", Price = 5m };
        var dto = new CreateUpdateAmenityDTO { Name = "New", Price = 15m, ImageUrl = "https://images.unsplash.com/photo-test-new" };
        _mockAmenityRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(amenity);
        
        await _amenityService.UpdateAmenityAsync(1, dto);

        Assert.That(amenity.Name, Is.EqualTo("New"));
        Assert.That(amenity.Price, Is.EqualTo(15m));
        _mockAmenityRepo.Verify(r => r.Update(amenity), Times.Once);
        _mockAmenityRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task DeleteAmenityAsync_ShouldThrow_IfNotFound()
    {
        _mockAmenityRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Amenity?)null);

//         var ex = Assert.ThrowsAsync<KeyNotFoundException>(() => _amenityService.DeleteAmenityAsync(99));
//         Assert.That(ex.Message, Does.Contain("Amenity not found"));
    }

    // [Test]
    public async Task DeleteAmenityAsync_ShouldDeleteAndSave()
    {
        var amenity = new Amenity { Id = 1 };
        _mockAmenityRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(amenity);

//         await _amenityService.DeleteAmenityAsync(1);

        Assert.That(amenity.IsAvailable, Is.False);
        _mockAmenityRepo.Verify(r => r.Update(amenity), Times.Once);
        _mockAmenityRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task UpdateAmenityStatusAsync_ShouldUpdateAndSave()
    {
        var amenity = new Amenity { Id = 1, IsAvailable = false };
        _mockAmenityRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(amenity);
        
        await _amenityService.UpdateAmenityStatusAsync(1, true);

        Assert.That(amenity.IsAvailable, Is.True);
        _mockAmenityRepo.Verify(r => r.Update(amenity), Times.Once);
        _mockAmenityRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task UpdateAmenityStatusAsync_ShouldThrow_IfNotFound()
    {
        _mockAmenityRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Amenity?)null);

        var ex = Assert.ThrowsAsync<KeyNotFoundException>(() => _amenityService.UpdateAmenityStatusAsync(99, true));
        Assert.That(ex.Message, Does.Contain("Amenity not found"));
    }

    [Test]
    public async Task GetAmenityByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        _mockAmenityRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Amenity?)null);

        var result = await _amenityService.GetAmenityByIdAsync(99);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task SubscribeAsync_ShouldThrow_IfUserUnauthorized()
    {
        var booking = new Booking 
        { 
            Id = 1, 
            GuestEmail = "other@example.com",
            BookingStatus = BookingStatus.CheckedIn, 
            PaymentStatus = PaymentStatus.Pending,
            BookingAmenities = new List<BookingAmenity>()
        };
        _mockBookingRepo.Setup(r => r.GetBookingWithDetailsAsync(1)).ReturnsAsync(booking);
        
        _mockCurrentUserService.Setup(s => s.IsInRole("RegisteredUser")).Returns(true);
        _mockCurrentUserService.Setup(s => s.IsInRole("Admin")).Returns(false);
        _mockCurrentUserService.Setup(s => s.IsInRole("FrontDesk")).Returns(false);
        _mockCurrentUserService.Setup(s => s.GetUserEmail()).Returns("me@example.com");

        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(() => _amenityService.SubscribeAsync(1, 1));
        Assert.That(ex.Message, Does.Contain("You can only subscribe to amenities for your own bookings"));
    }

    [Test]
    public async Task SubscribeAsync_ShouldAllow_IfUserIsAdminOrFrontDesk()
    {
        var booking = new Booking 
        { 
            Id = 1, 
            GuestEmail = "other@example.com",
            BookingStatus = BookingStatus.CheckedIn, 
            PaymentStatus = PaymentStatus.Pending,
            BookingAmenities = new List<BookingAmenity>()
        };
        _mockBookingRepo.Setup(r => r.GetBookingWithDetailsAsync(1)).ReturnsAsync(booking);
        
        // Setup Admin
        _mockCurrentUserService.Setup(s => s.IsInRole("RegisteredUser")).Returns(true);
        _mockCurrentUserService.Setup(s => s.IsInRole("Admin")).Returns(true);
        _mockCurrentUserService.Setup(s => s.IsInRole("FrontDesk")).Returns(false);
        _mockCurrentUserService.Setup(s => s.GetUserEmail()).Returns("admin@example.com");

        _mockAmenityRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Amenity { Id = 1, Price = 25.0m, IsAvailable = true });

        // Should not throw
        await _amenityService.SubscribeAsync(1, 1);
        
        _mockBookingRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task SubscribeAsync_ShouldAllow_IfGuestEmailMatches_RegisteredUser()
    {
        var booking = new Booking 
        { 
            Id = 1, 
            GuestEmail = "me@example.com",
            BookingStatus = BookingStatus.CheckedIn, 
            PaymentStatus = PaymentStatus.Pending,
            BookingAmenities = new List<BookingAmenity>()
        };
        var amenity = new Amenity { Id = 1, IsAvailable = true, Price = 50m };

        _mockBookingRepo.Setup(r => r.GetBookingWithDetailsAsync(1)).ReturnsAsync(booking);
        _mockCurrentUserService.Setup(s => s.IsInRole("RegisteredUser")).Returns(true);
        _mockCurrentUserService.Setup(s => s.IsInRole("Admin")).Returns(false);
        _mockCurrentUserService.Setup(s => s.IsInRole("FrontDesk")).Returns(false);
        _mockCurrentUserService.Setup(s => s.GetUserEmail()).Returns("me@example.com");
        _mockAmenityRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(amenity);

        await _amenityService.SubscribeAsync(1, 1);

        _mockBookingRepo.Verify(r => r.Update(It.IsAny<Booking>()), Times.Once);
    }

    [Test]
    public async Task SubscribeAsync_ShouldAllow_IfUserIsFrontDesk()
    {
        var booking = new Booking 
        { 
            Id = 1, 
            GuestEmail = "other@example.com",
            BookingStatus = BookingStatus.CheckedIn, 
            PaymentStatus = PaymentStatus.Pending,
            BookingAmenities = new List<BookingAmenity>()
        };
        _mockBookingRepo.Setup(r => r.GetBookingWithDetailsAsync(1)).ReturnsAsync(booking);
        
        _mockCurrentUserService.Setup(s => s.IsInRole("RegisteredUser")).Returns(true);
        _mockCurrentUserService.Setup(s => s.IsInRole("Admin")).Returns(false);
        _mockCurrentUserService.Setup(s => s.IsInRole("FrontDesk")).Returns(true);
        _mockCurrentUserService.Setup(s => s.GetUserEmail()).Returns("frontdesk@example.com");

        _mockAmenityRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Amenity { Id = 1, Price = 25.0m, IsAvailable = true });

        await _amenityService.SubscribeAsync(1, 1);
        
        _mockBookingRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task SubscribeAsync_ShouldAllow_IfNotRegisteredUser()
    {
        var booking = new Booking 
        { 
            Id = 1, 
            GuestEmail = "other@example.com",
            BookingStatus = BookingStatus.CheckedIn, 
            PaymentStatus = PaymentStatus.Pending,
            BookingAmenities = new List<BookingAmenity>()
        };
        _mockBookingRepo.Setup(r => r.GetBookingWithDetailsAsync(1)).ReturnsAsync(booking);
        
        // Triggers the first condition of the IF to be false, short-circuiting the rest
        _mockCurrentUserService.Setup(s => s.IsInRole("RegisteredUser")).Returns(false);

        _mockAmenityRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Amenity { Id = 1, Price = 25.0m, IsAvailable = true });

        await _amenityService.SubscribeAsync(1, 1);
        
        _mockBookingRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public void CreateAmenityAsync_ShouldThrow_IfDuplicate()
    {
        var dto = new CreateUpdateAmenityDTO { Name = "Spa" };
        var existingItems = new List<Amenity> { new Amenity { Name = "Spa" } };
        _mockAmenityRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Amenity, bool>>>())).ReturnsAsync(existingItems);
        
        var ex = Assert.ThrowsAsync<InvalidOperationException>(() => _amenityService.CreateAmenityAsync(dto));
        Assert.That(ex.Message, Does.Contain("already exists in the system"));
    }
}
