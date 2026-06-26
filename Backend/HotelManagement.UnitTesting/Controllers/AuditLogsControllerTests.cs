using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelManagement.API.Controllers;
using HotelManagement.BLL.DTOs;
using HotelManagement.BLL.Interfaces;
using HotelManagement.Repository.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace HotelManagement.UnitTesting.Controllers;

[TestFixture]
public class AuditLogsControllerTests
{
    private Mock<IAuditLogService> _mockAuditLogService;
    private AuditLogsController _controller;

    [SetUp]
    public void Setup()
    {
        _mockAuditLogService = new Mock<IAuditLogService>();
        _controller = new AuditLogsController(_mockAuditLogService.Object);
    }

    [Test]
    public async Task GetAuditLogs_ShouldReturnOk_WithPaginatedResult()
    {
        var resultDto = new PaginatedResult<AuditLogDTO>
        {
            Data = new List<AuditLogDTO> { new AuditLogDTO { Id = 1, Action = "Update" } },
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 10
        };
        _mockAuditLogService.Setup(s => s.GetAuditLogsAsync(1, 10, "Action", false)).ReturnsAsync(resultDto);

        var result = await _controller.GetAuditLogs(1, 10, "Action", false) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
        Assert.That(result.Value, Is.EqualTo(resultDto));
    }

    [Test]
    public async Task GetAuditLogs_ShouldCapPageSizeAt100()
    {
        var resultDto = new PaginatedResult<AuditLogDTO> { PageSize = 100 };
        _mockAuditLogService.Setup(s => s.GetAuditLogsAsync(1, 100, null, false)).ReturnsAsync(resultDto);

        var result = await _controller.GetAuditLogs(1, 1000) as OkObjectResult;

        _mockAuditLogService.Verify(s => s.GetAuditLogsAsync(1, 100, null, false), Times.Once);
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task GetAuditLog_ShouldReturnOk_WhenLogExists()
    {
        var dto = new AuditLogDTO { Id = 1 };
        _mockAuditLogService.Setup(s => s.GetAuditLogByIdAsync(1)).ReturnsAsync(dto);

        var result = await _controller.GetAuditLog(1) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
        Assert.That(result.Value, Is.EqualTo(dto));
    }

    [Test]
    public async Task GetAuditLog_ShouldReturnNotFound_WhenLogDoesNotExist()
    {
        _mockAuditLogService.Setup(s => s.GetAuditLogByIdAsync(1)).ReturnsAsync((AuditLogDTO?)null);

        var result = await _controller.GetAuditLog(1) as NotFoundObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(404));
    }
}
