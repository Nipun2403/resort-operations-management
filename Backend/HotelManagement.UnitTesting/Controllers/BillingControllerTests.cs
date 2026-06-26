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
public class BillingControllerTests
{
    private Mock<IBillingService> _mockBillingService;
    private BillingController _controller;

    [SetUp]
    public void Setup()
    {
        _mockBillingService = new Mock<IBillingService>();
        _controller = new BillingController(_mockBillingService.Object);
    }

    [Test]
    public async Task GetGlobalBilling_ShouldReturnOk_WithParsedDates()
    {
        var resultDto = new PaginatedResult<object> { TotalCount = 0 };
        _mockBillingService.Setup(s => s.GetGlobalBillingAsync(It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<string>(), false, 1, 10, null, false))
            .ReturnsAsync(resultDto);

        var result = await _controller.GetGlobalBilling(null, "01-01-2023", "31-01-2023", null) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
        Assert.That(result.Value, Is.EqualTo(resultDto));
    }

    [Test]
    public async Task GetGlobalBilling_ShouldReturnOk_WithInvalidDates()
    {
        var resultDto = new PaginatedResult<object> { TotalCount = 0 };
        _mockBillingService.Setup(s => s.GetGlobalBillingAsync(It.IsAny<string>(), null, null, It.IsAny<string>(), false, 1, 10, null, false))
            .ReturnsAsync(resultDto);

        var result = await _controller.GetGlobalBilling(null, "invalid-date", "another-invalid", null) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
        Assert.That(result.Value, Is.EqualTo(resultDto));
    }

    [Test]
    public async Task GetReceipts_ShouldReturnOk()
    {
        var resultDto = new PaginatedResult<ReceiptDTO>();
        _mockBillingService.Setup(s => s.GetReceiptsAsync(null, null, 1, 10, null, false)).ReturnsAsync(resultDto);

        var result = await _controller.GetReceipts(null, null, 1, 10) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task GetBillingFolio_ShouldReturnOk_WhenSuccessful()
    {
        var folio = new BillingFolioDTO();
        _mockBillingService.Setup(s => s.GenerateFolioAsync(1)).ReturnsAsync(folio);

        var result = await _controller.GetBillingFolio(1) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task GetBillingFolio_ShouldReturnNotFound_OnGeneralException()
    {
        _mockBillingService.Setup(s => s.GenerateFolioAsync(1)).ThrowsAsync(new Exception("Some error"));

        var result = await _controller.GetBillingFolio(1) as NotFoundObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(404));
    }

    [Test]
    public void GetBillingFolio_ShouldRethrow_KeyNotFoundException()
    {
        _mockBillingService.Setup(s => s.GenerateFolioAsync(1)).ThrowsAsync(new KeyNotFoundException("Not found"));

        Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.GetBillingFolio(1));
    }

    [Test]
    public async Task ProcessPayment_ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        _controller.ModelState.AddModelError("Amount", "Required");
        var result = await _controller.ProcessPayment(1, new PaymentRequestDTO()) as BadRequestObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task ProcessPayment_ShouldReturnOk_WhenSuccessful()
    {
        var request = new PaymentRequestDTO();
        _mockBillingService.Setup(s => s.ProcessPaymentAsync(1, request)).Returns(Task.CompletedTask);

        var result = await _controller.ProcessPayment(1, request) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task ProcessPayment_ShouldReturnBadRequest_OnInvalidOperationException()
    {
        var request = new PaymentRequestDTO();
        _mockBillingService.Setup(s => s.ProcessPaymentAsync(1, request)).ThrowsAsync(new InvalidOperationException("Invalid state"));

        var result = await _controller.ProcessPayment(1, request) as BadRequestObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task ProcessPayment_ShouldReturnBadRequest_OnGeneralException()
    {
        var request = new PaymentRequestDTO();
        _mockBillingService.Setup(s => s.ProcessPaymentAsync(1, request)).ThrowsAsync(new Exception("General error"));

        var result = await _controller.ProcessPayment(1, request) as BadRequestObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public void ProcessPayment_ShouldRethrow_UnauthorizedAccessException()
    {
        var request = new PaymentRequestDTO();
        _mockBillingService.Setup(s => s.ProcessPaymentAsync(1, request)).ThrowsAsync(new UnauthorizedAccessException("No access"));

        Assert.ThrowsAsync<UnauthorizedAccessException>(() => _controller.ProcessPayment(1, request));
    }
}
