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
public class AmenitiesControllerTests
{
    private Mock<IAmenityService> _mockAmenityService;
    private AmenitiesController _controller;

    [SetUp]
    public void Setup()
    {
        _mockAmenityService = new Mock<IAmenityService>();
        _controller = new AmenitiesController(_mockAmenityService.Object);
    }

    [Test]
    public async Task GetAmenities_ShouldReturnOk_WithPaginatedResult()
    {
        var resultDto = new PaginatedResult<AmenityDTO>
        {
            Data = new List<AmenityDTO> { new AmenityDTO { Id = 1, Name = "Spa" } },
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 10
        };
        _mockAmenityService.Setup(s => s.GetAllAmenitiesAsync(1, 10)).ReturnsAsync(resultDto);

        var result = await _controller.GetAmenities(1, 10) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
        Assert.That(result.Value, Is.EqualTo(resultDto));
    }

    [Test]
    public async Task GetAmenities_ShouldCapPageSizeAt100()
    {
        var resultDto = new PaginatedResult<AmenityDTO> { PageSize = 100 };
        _mockAmenityService.Setup(s => s.GetAllAmenitiesAsync(1, 100)).ReturnsAsync(resultDto);

        var result = await _controller.GetAmenities(1, 1000) as OkObjectResult;

        _mockAmenityService.Verify(s => s.GetAllAmenitiesAsync(1, 100), Times.Once);
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public async Task GetAmenity_ShouldReturnOk_WhenAmenityExists()
    {
        var amenityDto = new AmenityDTO { Id = 1, Name = "Spa" };
        _mockAmenityService.Setup(s => s.GetAmenityByIdAsync(1)).ReturnsAsync(amenityDto);

        var result = await _controller.GetAmenity(1) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
        Assert.That(result.Value, Is.EqualTo(amenityDto));
    }

    [Test]
    public async Task GetAmenity_ShouldReturnNotFound_WhenAmenityDoesNotExist()
    {
        _mockAmenityService.Setup(s => s.GetAmenityByIdAsync(1)).ReturnsAsync((AmenityDTO?)null);

        var result = await _controller.GetAmenity(1) as NotFoundObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(404));
    }

    [Test]
    public async Task CreateAmenity_ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        _controller.ModelState.AddModelError("Name", "Required");
        var request = new CreateUpdateAmenityDTO();

        var result = await _controller.CreateAmenity(request) as BadRequestObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task CreateAmenity_ShouldReturnCreatedAtAction_WhenValid()
    {
        var request = new CreateUpdateAmenityDTO { Name = "Pool" };
        var amenityDto = new AmenityDTO { Id = 1, Name = "Pool" };
        _mockAmenityService.Setup(s => s.CreateAmenityAsync(request)).ReturnsAsync(amenityDto);

        var result = await _controller.CreateAmenity(request) as CreatedAtActionResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.StatusCode, Is.EqualTo(201));
        Assert.That(result.ActionName, Is.EqualTo(nameof(AmenitiesController.GetAmenity)));
        Assert.That(result.RouteValues!["id"], Is.EqualTo(1));
        Assert.That(result.Value, Is.EqualTo(amenityDto));
    }

    [Test]
    public async Task UpdateAmenity_ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        _controller.ModelState.AddModelError("Name", "Required");
        var request = new CreateUpdateAmenityDTO();

        var result = await _controller.UpdateAmenity(1, request) as BadRequestObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task UpdateAmenity_ShouldReturnOk_WhenValid()
    {
        var request = new CreateUpdateAmenityDTO { Name = "Updated", IsAvailable = true };
        _mockAmenityService.Setup(s => s.UpdateAmenityAsync(1, request, true)).Returns(Task.CompletedTask);

        var result = await _controller.UpdateAmenity(1, request) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task UpdateAmenity_ShouldReturnNotFound_OnArgumentException()
    {
        var request = new CreateUpdateAmenityDTO { Name = "Updated", IsAvailable = true };
        _mockAmenityService.Setup(s => s.UpdateAmenityAsync(1, request, true)).ThrowsAsync(new ArgumentException("Amenity not found."));

        var result = await _controller.UpdateAmenity(1, request) as NotFoundObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(404));
        Assert.That(result.Value, Is.EqualTo("Amenity not found."));
    }
}
