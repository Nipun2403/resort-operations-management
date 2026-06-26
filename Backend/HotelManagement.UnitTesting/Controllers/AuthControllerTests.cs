using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using HotelManagement.API.Controllers;
using HotelManagement.BLL.DTOs;
using HotelManagement.BLL.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace HotelManagement.UnitTesting.Controllers;

[TestFixture]
public class AuthControllerTests
{
    private Mock<IAuthService> _mockAuthService;
    private AuthController _controller;

    [SetUp]
    public void Setup()
    {
        _mockAuthService = new Mock<IAuthService>();
        _controller = new AuthController(_mockAuthService.Object);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    [Test]
    public async Task Register_ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        _controller.ModelState.AddModelError("Email", "Required");
        var request = new RegisterRequestDTO();

        var result = await _controller.Register(request) as BadRequestObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task Register_ShouldReturnConflict_WhenRegistrationFails()
    {
        var request = new RegisterRequestDTO();
        _mockAuthService.Setup(s => s.RegisterAsync(request))
            .ReturnsAsync(new AuthResponseDTO { Success = false, Message = "Email already in use" });

        var result = await _controller.Register(request) as ConflictObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(409));
        Assert.That(result.Value, Is.EqualTo("Email already in use"));
    }

    [Test]
    public async Task Register_ShouldReturnOk_WhenRegistrationSucceeds()
    {
        var request = new RegisterRequestDTO();
        _mockAuthService.Setup(s => s.RegisterAsync(request))
            .ReturnsAsync(new AuthResponseDTO { Success = true, Message = "Success" });

        var result = await _controller.Register(request) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task Login_ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        _controller.ModelState.AddModelError("Email", "Required");
        var request = new LoginRequestDTO();

        var result = await _controller.Login(request) as BadRequestObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(400));
    }

    [Test]
    public async Task Login_ShouldReturnUnauthorized_WhenLoginFails()
    {
        var request = new LoginRequestDTO();
        _mockAuthService.Setup(s => s.LoginAsync(request))
            .ReturnsAsync(new AuthResponseDTO { Success = false, Message = "Invalid credentials" });

        var result = await _controller.Login(request) as UnauthorizedObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(401));
        Assert.That(result.Value, Is.EqualTo("Invalid credentials"));
    }

    [Test]
    public async Task Login_ShouldReturnOk_WhenLoginSucceeds()
    {
        var request = new LoginRequestDTO();
        _mockAuthService.Setup(s => s.LoginAsync(request))
            .ReturnsAsync(new AuthResponseDTO { Success = true, Token = "jwt", Role = "Admin", FirstName = "John", LastName = "Doe" });

        var result = await _controller.Login(request) as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public void GetMe_ShouldReturnOk_WithUserClaims()
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "JohnDoe")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        
        _controller.ControllerContext.HttpContext.User = claimsPrincipal;

        var result = _controller.GetMe() as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public void GetMe_ShouldReturnOk_WithUnauthenticatedUser()
    {
        var claimsPrincipal = new ClaimsPrincipal(); // No identity
        _controller.ControllerContext.HttpContext.User = claimsPrincipal;

        var result = _controller.GetMe() as OkObjectResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
    }
}
