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
public class FeedbackControllerTests
{
    private Mock<IFeedbackService> _mockFeedbackService;
    private FeedbackController _controller;

    [SetUp]
    public void Setup()
    {
        _mockFeedbackService = new Mock<IFeedbackService>();
        _controller = new FeedbackController(_mockFeedbackService.Object);
    }

    [Test]
    public async Task GetAllFeedback_ShouldReturnOk()
    {
        var resultDto = new PaginatedResult<FeedbackDTO>();
        _mockFeedbackService.Setup(s => s.GetFeedbackAsync(1, 10, false, null, false)).ReturnsAsync(resultDto);

        var result = await _controller.GetAllFeedback(1, 10) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task GetFeedbackForBooking_ShouldReturnOk()
    {
        var listDto = new List<FeedbackDTO>();
        _mockFeedbackService.Setup(s => s.GetFeedbackForBookingAsync(1)).ReturnsAsync(listDto);

        var result = await _controller.GetFeedbackForBooking(1) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task SubmitFeedback_ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        _controller.ModelState.AddModelError("Rating", "Required");
        var result = await _controller.SubmitFeedback(new CreateFeedbackDTO()) as BadRequestObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task SubmitFeedback_ShouldReturnOk_WhenSuccessful()
    {
        var dto = new CreateFeedbackDTO();
        var resultDto = new FeedbackDTO();
        _mockFeedbackService.Setup(s => s.SubmitFeedbackAsync(dto)).ReturnsAsync(resultDto);

        var result = await _controller.SubmitFeedback(dto) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task SubmitFeedback_ShouldReturnBadRequest_OnArgumentException()
    {
        var dto = new CreateFeedbackDTO();
        _mockFeedbackService.Setup(s => s.SubmitFeedbackAsync(dto)).ThrowsAsync(new ArgumentException());

        var result = await _controller.SubmitFeedback(dto) as BadRequestObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task ModerateFeedback_ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        _controller.ModelState.AddModelError("IsHidden", "Required");
        var result = await _controller.ModerateFeedback(1, new ModerateFeedbackRequestDTO()) as BadRequestObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task ModerateFeedback_ShouldReturnOk_WhenSuccessful()
    {
        var dto = new ModerateFeedbackRequestDTO { IsHidden = true };
        var resultDto = new FeedbackDTO();
        _mockFeedbackService.Setup(s => s.ModerateFeedbackAsync(1, true)).ReturnsAsync(resultDto);

        var result = await _controller.ModerateFeedback(1, dto) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task ModerateFeedback_ShouldReturnNotFound_OnArgumentException()
    {
        var dto = new ModerateFeedbackRequestDTO { IsHidden = true };
        _mockFeedbackService.Setup(s => s.ModerateFeedbackAsync(1, true)).ThrowsAsync(new ArgumentException());

        var result = await _controller.ModerateFeedback(1, dto) as NotFoundObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(404));
    }
}
