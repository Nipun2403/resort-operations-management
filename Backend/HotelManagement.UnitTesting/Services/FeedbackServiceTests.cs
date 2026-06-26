using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using HotelManagement.BLL.DTOs;
using HotelManagement.BLL.Interfaces;
using HotelManagement.BLL.Services;
using HotelManagement.DAL.Entities;
using HotelManagement.Repository.Interfaces;
using Moq;
using NUnit.Framework;
using HotelManagement.Repository.Models;

namespace HotelManagement.UnitTesting.Services;

[TestFixture]
public class FeedbackServiceTests
{
    private Mock<IGenericRepository<Feedback>> _mockFeedbackRepo;
    private Mock<IBookingRepository> _mockBookingRepo;
    private Mock<IMapper> _mockMapper;
    private Mock<ICurrentUserService> _mockCurrentUserService;
    private FeedbackService _feedbackService;

    [SetUp]
    public void Setup()
    {
        _mockFeedbackRepo = new Mock<IGenericRepository<Feedback>>();
        _mockBookingRepo = new Mock<IBookingRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();

        _feedbackService = new FeedbackService(
            _mockFeedbackRepo.Object,
            _mockBookingRepo.Object,
            _mockMapper.Object,
            _mockCurrentUserService.Object
        );
    }

    [Test]
    public async Task GetFeedbackAsync_ShouldReturnMappedFeedback()
    {
        var feedback = new List<Feedback> { new Feedback { Id = 1 } };
        var pagedFeedback = new PaginatedResult<Feedback> { TotalCount = 1, PageNumber = 1, PageSize = 10, Data = feedback };
        _mockFeedbackRepo.Setup(r => r.GetPaginatedResultAsync(1, 10, It.IsAny<Expression<Func<Feedback, bool>>?>(), null)).ReturnsAsync(pagedFeedback);
        _mockMapper.Setup(m => m.Map<IEnumerable<FeedbackDTO>>(feedback)).Returns(new List<FeedbackDTO> { new FeedbackDTO { Id = 1 } });

        var result = await _feedbackService.GetFeedbackAsync(1, 10);

        Assert.That(result.Data.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task GetFeedbackAsync_WithIncludeHidden_False_AppliesFilter()
    {
        var pagedFeedback = new PaginatedResult<Feedback> { TotalCount = 0, PageNumber = 1, PageSize = 10, Data = new List<Feedback>() };
        _mockFeedbackRepo.Setup(r => r.GetPaginatedResultAsync(1, 10, It.IsNotNull<Expression<Func<Feedback, bool>>>(), It.IsAny<Func<IQueryable<Feedback>, IOrderedQueryable<Feedback>>>())).ReturnsAsync(pagedFeedback);
        _mockMapper.Setup(m => m.Map<IEnumerable<FeedbackDTO>>(It.IsAny<IEnumerable<Feedback>>())).Returns(new List<FeedbackDTO>());

        await _feedbackService.GetFeedbackAsync(1, 10, includeHidden: false);

        _mockFeedbackRepo.Verify(r => r.GetPaginatedResultAsync(1, 10, It.IsNotNull<Expression<Func<Feedback, bool>>>(), It.IsAny<Func<IQueryable<Feedback>, IOrderedQueryable<Feedback>>>()), Times.Once);
    }

    [Test]
    public async Task GetFeedbackAsync_WithIncludeHidden_True_DoesNotApplyFilter()
    {
        var pagedFeedback = new PaginatedResult<Feedback> { TotalCount = 0, PageNumber = 1, PageSize = 10, Data = new List<Feedback>() };
        _mockFeedbackRepo.Setup(r => r.GetPaginatedResultAsync(1, 10, null, It.IsAny<Func<IQueryable<Feedback>, IOrderedQueryable<Feedback>>>())).ReturnsAsync(pagedFeedback);
        _mockMapper.Setup(m => m.Map<IEnumerable<FeedbackDTO>>(It.IsAny<IEnumerable<Feedback>>())).Returns(new List<FeedbackDTO>());

        await _feedbackService.GetFeedbackAsync(1, 10, includeHidden: true);

        _mockFeedbackRepo.Verify(r => r.GetPaginatedResultAsync(1, 10, null, It.IsAny<Func<IQueryable<Feedback>, IOrderedQueryable<Feedback>>>()), Times.Once);
    }

    [Test]
    public async Task GetFeedbackAsync_WithSortBy_AppliesSorting()
    {
        var pagedFeedback = new PaginatedResult<Feedback> { TotalCount = 0, PageNumber = 1, PageSize = 10, Data = new List<Feedback>() };
        _mockFeedbackRepo.Setup(r => r.GetPaginatedResultAsync(1, 10, It.IsAny<Expression<Func<Feedback, bool>>>(), It.IsNotNull<Func<IQueryable<Feedback>, IOrderedQueryable<Feedback>>>())).ReturnsAsync(pagedFeedback);
        _mockMapper.Setup(m => m.Map<IEnumerable<FeedbackDTO>>(It.IsAny<IEnumerable<Feedback>>())).Returns(new List<FeedbackDTO>());

        await _feedbackService.GetFeedbackAsync(1, 10, false, "Rating", true);

        _mockFeedbackRepo.Verify(r => r.GetPaginatedResultAsync(1, 10, It.IsAny<Expression<Func<Feedback, bool>>>(), It.IsNotNull<Func<IQueryable<Feedback>, IOrderedQueryable<Feedback>>>()), Times.Once);
    }

    [Test]
    public async Task GetFeedbackForBookingAsync_ShouldReturnMappedFeedback()
    {
        var feedback = new List<Feedback> { new Feedback { Id = 1, BookingId = 5 } };
        _mockFeedbackRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Feedback, bool>>>())).ReturnsAsync(feedback);
        _mockMapper.Setup(m => m.Map<IEnumerable<FeedbackDTO>>(feedback)).Returns(new List<FeedbackDTO> { new FeedbackDTO { Id = 1, BookingId = 5 } });

        var result = (await _feedbackService.GetFeedbackForBookingAsync(5)).ToList();

        Assert.That(result.Count, Is.EqualTo(1));
    }

    [Test]
    public void SubmitFeedbackAsync_ShouldThrow_IfBookingNotFound()
    {
        var dto = new CreateFeedbackDTO { BookingId = 99 };
        _mockBookingRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Booking?)null);

        var ex = Assert.ThrowsAsync<KeyNotFoundException>(() => _feedbackService.SubmitFeedbackAsync(dto));
        Assert.That(ex.Message, Does.Contain("Booking not found"));
    }

    [Test]
    public void SubmitFeedbackAsync_ShouldThrow_IfUnauthorized()
    {
        var dto = new CreateFeedbackDTO { BookingId = 1 };
        var booking = new Booking { Id = 1, GuestEmail = "guest@example.com" };
        _mockBookingRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(booking);
        
        _mockCurrentUserService.Setup(c => c.IsInRole("RegisteredUser")).Returns(true);
        _mockCurrentUserService.Setup(c => c.IsInRole("Admin")).Returns(false);
        _mockCurrentUserService.Setup(c => c.IsInRole("FrontDesk")).Returns(false);
        _mockCurrentUserService.Setup(c => c.GetUserEmail()).Returns("other@example.com"); // Not the guest email

        var ex = Assert.ThrowsAsync<UnauthorizedAccessException>(() => _feedbackService.SubmitFeedbackAsync(dto));
        Assert.That(ex.Message, Does.Contain("own bookings"));
    }

    [TestCase(0)]
    [TestCase(6)]
    public void SubmitFeedbackAsync_ShouldThrow_IfRatingOutOfBounds(int rating)
    {
        var dto = new CreateFeedbackDTO { BookingId = 1, Rating = rating };
        _mockBookingRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Booking { Id = 1 });

        var ex = Assert.ThrowsAsync<ArgumentException>(() => _feedbackService.SubmitFeedbackAsync(dto));
        Assert.That(ex.Message, Does.Contain("between 1 and 5"));
    }

    [Test]
    public void SubmitFeedbackAsync_ShouldThrow_IfFeedbackExists()
    {
        var dto = new CreateFeedbackDTO { BookingId = 1, Rating = 5 };
        _mockBookingRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Booking { Id = 1 });
        var existing = new List<Feedback> { new Feedback { Id = 5, BookingId = 1 } };
        _mockFeedbackRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Feedback, bool>>>())).ReturnsAsync(existing);

        var ex = Assert.ThrowsAsync<ArgumentException>(() => _feedbackService.SubmitFeedbackAsync(dto));
        Assert.That(ex.Message, Does.Contain("already exists"));
    }

    [Test]
    public async Task SubmitFeedbackAsync_ShouldAddFeedback_IfValid()
    {
        var dto = new CreateFeedbackDTO { BookingId = 1, Rating = 5, Comments = "Great" };
        var feedback = new Feedback { BookingId = 1, Rating = 5, Comments = "Great" };

        _mockBookingRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Booking { Id = 1 });
        _mockMapper.Setup(m => m.Map<Feedback>(dto)).Returns(feedback);
        _mockMapper.Setup(m => m.Map<FeedbackDTO>(feedback)).Returns(new FeedbackDTO { Id = 10, Rating = 5 });

        var result = await _feedbackService.SubmitFeedbackAsync(dto);

        Assert.That(result.Id, Is.EqualTo(10));
        Assert.That(result.Rating, Is.EqualTo(5));
        
        _mockFeedbackRepo.Verify(r => r.AddAsync(feedback), Times.Once);
        _mockFeedbackRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        Assert.That(feedback.CreatedAt.Date, Is.EqualTo(DateTime.UtcNow.Date)); // Check if CreatedAt was set
    }

    [Test]
    public void ModerateFeedbackAsync_ShouldThrow_IfNotFound()
    {
        _mockFeedbackRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Feedback?)null);

        var ex = Assert.ThrowsAsync<KeyNotFoundException>(() => _feedbackService.ModerateFeedbackAsync(99, true));
        Assert.That(ex.Message, Does.Contain("Feedback not found"));
    }

    [Test]
    public async Task ModerateFeedbackAsync_ShouldUpdateIsHidden()
    {
        var feedback = new Feedback { Id = 1, IsHidden = false };
        _mockFeedbackRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(feedback);
        _mockMapper.Setup(m => m.Map<FeedbackDTO>(feedback)).Returns(new FeedbackDTO { Id = 1, IsHidden = true });

        var result = await _feedbackService.ModerateFeedbackAsync(1, true);

        Assert.That(feedback.IsHidden, Is.True);
        _mockFeedbackRepo.Verify(r => r.Update(feedback), Times.Once);
        _mockFeedbackRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        Assert.That(result.IsHidden, Is.True);
    }

    [Test]
    public async Task SubmitFeedbackAsync_ShouldAllow_IfGuestEmailMatches_RegisteredUser()
    {
        var booking = new Booking 
        { 
            Id = 1, 
            GuestEmail = "me@example.com"
        };
        var dto = new CreateFeedbackDTO { BookingId = 1, Rating = 5, Comments = "Great" };

        _mockBookingRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(booking);
        _mockCurrentUserService.Setup(s => s.IsInRole("RegisteredUser")).Returns(true);
        _mockCurrentUserService.Setup(s => s.IsInRole("Admin")).Returns(false);
        _mockCurrentUserService.Setup(s => s.IsInRole("FrontDesk")).Returns(false);
        _mockCurrentUserService.Setup(s => s.GetUserEmail()).Returns("me@example.com");

        _mockFeedbackRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Feedback, bool>>>())).ReturnsAsync(Enumerable.Empty<Feedback>());

        var feedback = new Feedback { Id = 1 };
        _mockMapper.Setup(m => m.Map<Feedback>(dto)).Returns(feedback);
        _mockMapper.Setup(m => m.Map<FeedbackDTO>(feedback)).Returns(new FeedbackDTO { Id = 1 });

        var result = await _feedbackService.SubmitFeedbackAsync(dto);

        Assert.That(result, Is.Not.Null);
        _mockFeedbackRepo.Verify(r => r.AddAsync(feedback), Times.Once);
    }

    [Test]
    public async Task SubmitFeedbackAsync_ShouldAllow_IfUserIsFrontDesk()
    {
        var booking = new Booking { Id = 1, GuestEmail = "other@example.com" };
        var dto = new CreateFeedbackDTO { BookingId = 1, Rating = 5, Comments = "Great" };

        _mockBookingRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(booking);
        _mockCurrentUserService.Setup(s => s.IsInRole("RegisteredUser")).Returns(true);
        _mockCurrentUserService.Setup(s => s.IsInRole("Admin")).Returns(false);
        _mockCurrentUserService.Setup(s => s.IsInRole("FrontDesk")).Returns(true);
        _mockCurrentUserService.Setup(s => s.GetUserEmail()).Returns("frontdesk@example.com");

        _mockFeedbackRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Feedback, bool>>>())).ReturnsAsync(Enumerable.Empty<Feedback>());

        var feedback = new Feedback { Id = 1 };
        _mockMapper.Setup(m => m.Map<Feedback>(dto)).Returns(feedback);
        _mockMapper.Setup(m => m.Map<FeedbackDTO>(feedback)).Returns(new FeedbackDTO { Id = 1 });

        var result = await _feedbackService.SubmitFeedbackAsync(dto);

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task SubmitFeedbackAsync_ShouldAllow_IfNotRegisteredUser()
    {
        var booking = new Booking { Id = 1, GuestEmail = "other@example.com" };
        var dto = new CreateFeedbackDTO { BookingId = 1, Rating = 5, Comments = "Great" };

        _mockBookingRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(booking);
        _mockCurrentUserService.Setup(s => s.IsInRole("RegisteredUser")).Returns(false);

        _mockFeedbackRepo.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Feedback, bool>>>())).ReturnsAsync(Enumerable.Empty<Feedback>());

        var feedback = new Feedback { Id = 1 };
        _mockMapper.Setup(m => m.Map<Feedback>(dto)).Returns(feedback);
        _mockMapper.Setup(m => m.Map<FeedbackDTO>(feedback)).Returns(new FeedbackDTO { Id = 1 });

        var result = await _feedbackService.SubmitFeedbackAsync(dto);

        Assert.That(result, Is.Not.Null);
    }
}
