using AutoMapper;
using HotelManagement.BLL.DTOs;
using HotelManagement.BLL.Profiles;
using HotelManagement.BLL.Services;
using HotelManagement.DAL.Entities;
using HotelManagement.Repository.Interfaces;
using Moq;
using NUnit.Framework;
using HotelManagement.Repository.Models;
using System.Linq.Expressions;

namespace HotelManagement.UnitTesting.Services;

[TestFixture]
public class AuditLogServiceTests
{
    private Mock<IAuditLogRepository> _mockAuditLogRepo;
    private Mock<IMapper> _mockMapper;
    private AuditLogService _auditLogService;

    [SetUp]
    public void Setup()
    {
        _mockAuditLogRepo = new Mock<IAuditLogRepository>();
        _mockMapper = new Mock<IMapper>();

        _auditLogService = new AuditLogService(_mockAuditLogRepo.Object, _mockMapper.Object);
    }

    [Test]
    public async Task GetAuditLogsAsync_ReturnsPaginatedResult()
    {
        // Arrange
        var logs = new List<AuditLog>
        {
            new AuditLog { Id = 1, Action = "Added" },
            new AuditLog { Id = 2, Action = "Modified" }
        };
        var paginatedLogs = new PaginatedResult<AuditLog>
        {
            TotalCount = 2,
            PageNumber = 1,
            PageSize = 10,
            Data = logs
        };
        _mockAuditLogRepo.Setup(r => r.GetPaginatedResultAsync(1, 10, null)).ReturnsAsync(paginatedLogs);

        var dtos = new List<AuditLogDTO>
        {
            new AuditLogDTO { Id = 1 },
            new AuditLogDTO { Id = 2 }
        };
        _mockMapper.Setup(m => m.Map<IEnumerable<AuditLogDTO>>(logs)).Returns(dtos);

        // Act
        var result = await _auditLogService.GetAuditLogsAsync(1, 10);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.TotalCount, Is.EqualTo(2));
        Assert.That(result.Data.Count(), Is.EqualTo(2));
        Assert.That(result.Data.First().Id, Is.EqualTo(1));
        Assert.That(result.Data.Last().Id, Is.EqualTo(2));
    }

    [Test]
    public async Task GetAuditLogByIdAsync_LogExists_ReturnsMappedDTO()
    {
        // Arrange
        var log = new AuditLog { Id = 1, Action = "Deleted" };
        _mockAuditLogRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(log);

        var dto = new AuditLogDTO { Id = 1, Action = "Deleted" };
        _mockMapper.Setup(m => m.Map<AuditLogDTO>(log)).Returns(dto);

        // Act
        var result = await _auditLogService.GetAuditLogByIdAsync(1);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(1));
        Assert.That(result.Action, Is.EqualTo("Deleted"));
    }

    [Test]
    public async Task GetAuditLogByIdAsync_LogDoesNotExist_ReturnsNull()
    {
        // Arrange
        _mockAuditLogRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((AuditLog?)null);

        // Act
        var result = await _auditLogService.GetAuditLogByIdAsync(1);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetAuditLogsAsync_WithSortBy_ReturnsPaginatedResult()
    {
        var logs = new List<AuditLog> { new AuditLog { Id = 1, Action = "Added" } };
        var paginatedLogs = new PaginatedResult<AuditLog> { TotalCount = 1, PageNumber = 1, PageSize = 10, Data = logs };
        
        _mockAuditLogRepo.Setup(r => r.GetPaginatedResultAsync(
            1, 10, null, 
            It.IsAny<Func<IQueryable<AuditLog>, IOrderedQueryable<AuditLog>>>()))
            .ReturnsAsync(paginatedLogs);
            
        _mockMapper.Setup(m => m.Map<IEnumerable<AuditLogDTO>>(logs)).Returns(new List<AuditLogDTO> { new AuditLogDTO { Id = 1 } });

        var result = await _auditLogService.GetAuditLogsAsync(1, 10, "Action", true);

        Assert.That(result.Data.Count(), Is.EqualTo(1));
    }
}
