using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using HotelManagement.BLL.DTOs;
using HotelManagement.BLL.Exceptions;
using HotelManagement.BLL.Interfaces;
using HotelManagement.BLL.Options;
using HotelManagement.BLL.Services;
using HotelManagement.BLL.Services.Concierge;
using HotelManagement.DAL.Entities;
using HotelManagement.DAL.Enums;
using HotelManagement.Repository.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace HotelManagement.UnitTesting.Services;

[TestFixture]
public class ConciergeServiceTests
{
    private Mock<ICurrentUserService> _mockCurrentUserService;
    private Mock<IBookingService> _mockBookingService;
    private Mock<IOrderService> _mockOrderService;
    private Mock<IHousekeepingService> _mockHousekeepingService;
    private Mock<IMaintenanceService> _mockMaintenanceService;
    private Mock<IBillingService> _mockBillingService;
    private Mock<IMenuItemRepository> _mockMenuItemRepository;
    private Mock<IFoodOrderRepository> _mockFoodOrderRepository;
    private Mock<IConversationRepository> _mockConversationRepo;
    private Mock<IConciergeProposalRepository> _mockProposalRepo;
    private Mock<HotelManagement.Repository.Interfaces.IConciergeActionLogRepository> _mockAuditLog;
    private Mock<IOptions<OpenAIOptions>> _mockOpenAIOptions;
    private Mock<ILogger<ConciergeService>> _mockLogger;
    private Mock<IMapper> _mockMapper;

    private ConciergeService _conciergeService;

    [SetUp]
    public void Setup()
    {
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mockBookingService = new Mock<IBookingService>();
        _mockOrderService = new Mock<IOrderService>();
        _mockHousekeepingService = new Mock<IHousekeepingService>();
        _mockMaintenanceService = new Mock<IMaintenanceService>();
        _mockBillingService = new Mock<IBillingService>();
        _mockMenuItemRepository = new Mock<IMenuItemRepository>();
        _mockFoodOrderRepository = new Mock<IFoodOrderRepository>();
        _mockConversationRepo = new Mock<IConversationRepository>();
        _mockProposalRepo = new Mock<IConciergeProposalRepository>();
        _mockAuditLog = new Mock<HotelManagement.Repository.Interfaces.IConciergeActionLogRepository>();
        _mockOpenAIOptions = new Mock<IOptions<OpenAIOptions>>();
        _mockLogger = new Mock<ILogger<ConciergeService>>();

        _mockMapper = new Mock<IMapper>();

        _mockOpenAIOptions.Setup(o => o.Value).Returns(new OpenAIOptions
        {
            ApiKey = "test-key",
            Model = "gpt-4o-mini"
        });

        _mockCurrentUserService.Setup(c => c.GetUserId()).Returns(1);
        _mockCurrentUserService.Setup(c => c.GetUserEmail()).Returns("test@test.com");

        _conciergeService = new ConciergeService(
            _mockCurrentUserService.Object,
            _mockBookingService.Object,
            _mockOrderService.Object,
            _mockHousekeepingService.Object,
            _mockMaintenanceService.Object,
            _mockBillingService.Object,
            _mockMenuItemRepository.Object,
            _mockFoodOrderRepository.Object,
            _mockConversationRepo.Object,
            _mockProposalRepo.Object,
            _mockAuditLog.Object,
            _mockOpenAIOptions.Object,
            _mockLogger.Object
        );
    }

    [Test]
    public async Task ConfirmProposalsAsync_ShouldThrow_WhenProposalExpired()
    {
        var proposalIds = new List<string> { Guid.NewGuid().ToString() };
        var expiredProposal = new ConciergeProposal
        {
            Id = Guid.Parse(proposalIds[0]),
            Status = "pending",
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1)
        };

        _mockProposalRepo.Setup(p => p.GetByIdsAsync(It.IsAny<List<Guid>>(), 1, "conv1"))
            .ReturnsAsync(new List<ConciergeProposal> { expiredProposal });

        var result = await _conciergeService.ConfirmProposalsAsync("conv1", proposalIds, CancellationToken.None);

        Assert.That(result.IsComplete, Is.False);
        Assert.That(result.Reply, Does.Contain("expired"));
    }

    [Test]
    public async Task ConfirmProposalsAsync_ShouldThrow_WhenProposalNotFound()
    {
        var proposalIds = new List<string> { Guid.NewGuid().ToString() };

        _mockProposalRepo.Setup(p => p.GetByIdsAsync(It.IsAny<List<Guid>>(), 1, "conv1"))
            .ReturnsAsync(new List<ConciergeProposal>());

        var result = await _conciergeService.ConfirmProposalsAsync("conv1", proposalIds, CancellationToken.None);

        Assert.That(result.IsComplete, Is.False);
        Assert.That(result.Reply, Does.Contain("not found"));
    }

    [Test]
    public async Task CreateFoodOrderAsync_ShouldFail_WhenMenuItemNotFound()
    {
        var args = new HotelManagement.BLL.Services.Concierge.CreateFoodOrderToolArgs
        {
            Items = new List<HotelManagement.BLL.Services.Concierge.FoodOrderItemToolArg>
            {
                new() { MenuItemId = 999, Quantity = 1 }
            }
        };

        _mockMenuItemRepository.Setup(m => m.GetByIdAsync(999)).ReturnsAsync((MenuItem?)null);

        var result = await _conciergeService.CreateFoodOrderAsync(args, new GuestContextDTO(), CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Does.Contain("not found"));
    }

    [Test]
    public void CreateFoodOrderAsync_ShouldFail_WhenMenuItemUnavailable()
    {
        var args = new HotelManagement.BLL.Services.Concierge.CreateFoodOrderToolArgs
        {
            Items = new List<HotelManagement.BLL.Services.Concierge.FoodOrderItemToolArg>
            {
                new() { MenuItemId = 1, Quantity = 1 }
            }
        };

        _mockMenuItemRepository.Setup(m => m.GetByIdAsync(1))
            .ReturnsAsync(new MenuItem { Id = 1, Name = "Burger", IsAvailable = false });

        var result = _conciergeService.CreateFoodOrderAsync(args, new GuestContextDTO(), CancellationToken.None);

        Assert.ThrowsAsync<ConciergeValidationException>(async () => await result);
    }

    [Test]
    public void CreateFoodOrderAsync_ShouldFail_WhenNotCheckedIn()
    {
        var args = new HotelManagement.BLL.Services.Concierge.CreateFoodOrderToolArgs
        {
            Items = new List<HotelManagement.BLL.Services.Concierge.FoodOrderItemToolArg>
            {
                new() { MenuItemId = 1, Quantity = 1 }
            }
        };

        var context = new GuestContextDTO
        {
            BookingId = 1,
            RoomId = 1,
            BookingStatus = "Booked"
        };

        _mockMenuItemRepository.Setup(m => m.GetByIdAsync(1))
            .ReturnsAsync(new MenuItem { Id = 1, Name = "Burger", IsAvailable = true });

        var result = _conciergeService.CreateFoodOrderAsync(args, context, CancellationToken.None);

        Assert.ThrowsAsync<ConciergeValidationException>(async () => await result);
    }

    [Test]
    public async Task CreateHousekeepingRequestAsync_ShouldFail_WhenNoRoom()
    {
        var args = new HotelManagement.BLL.Services.Concierge.CreateHousekeepingToolArgs
        {
            Description = "Extra towels",
            IsEmergency = false
        };

        var context = new GuestContextDTO
        {
            RoomId = null
        };

        var result = await _conciergeService.CreateHousekeepingRequestAsync(args, context, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Does.Contain("room"));
    }

    [Test]
    public async Task CreateMaintenanceTicketAsync_ShouldFail_WhenNoRoom()
    {
        var args = new HotelManagement.BLL.Services.Concierge.CreateMaintenanceToolArgs
        {
            Description = "Broken AC",
            IsEmergency = true
        };

        var context = new GuestContextDTO
        {
            RoomId = null
        };

        var result = await _conciergeService.CreateMaintenanceTicketAsync(args, context, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Does.Contain("room"));
    }
}