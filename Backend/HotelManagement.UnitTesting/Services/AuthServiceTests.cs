using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using HotelManagement.BLL.DTOs;
using HotelManagement.BLL.Services;
using HotelManagement.DAL.Entities;
using HotelManagement.Repository.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;

namespace HotelManagement.UnitTesting.Services;

[TestFixture]
public class AuthServiceTests
{
    private Mock<IConfiguration> _mockConfig;
    private Mock<IUserRepository> _mockUserRepository;
    private AuthService _authService;

    [SetUp]
    public void Setup()
    {
        _mockConfig = new Mock<IConfiguration>();
        _mockUserRepository = new Mock<IUserRepository>();

        // Setup mock config for JWT
        _mockConfig.Setup(c => c["Jwt:Key"]).Returns("very_long_super_secret_testing_key_for_hmac_12345!");
        _mockConfig.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
        _mockConfig.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");

        _authService = new AuthService(_mockConfig.Object, _mockUserRepository.Object);
    }

    [Test]
    public void GenerateJwtToken_ShouldReturnValidToken()
    {
        // Act
        var token = _authService.GenerateJwtToken(1, "test@example.com", "Admin", "John", "Doe");

        // Assert
        Assert.That(token, Is.Not.Null);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        Assert.That(jwtToken.Issuer, Is.EqualTo("TestIssuer"));
        Assert.That(jwtToken.Audiences, Contains.Item("TestAudience"));
    }

    [Test]
    public void GenerateJwtToken_ShouldUseFallbacks_WhenConfigIsNull()
    {
        // Arrange - Override Setup mocks to return null
        _mockConfig.Setup(c => c["Jwt:Key"]).Returns((string?)null);
        _mockConfig.Setup(c => c["Jwt:Issuer"]).Returns((string?)null);
        _mockConfig.Setup(c => c["Jwt:Audience"]).Returns((string?)null);

        // Act
        var token = _authService.GenerateJwtToken(1, "test@example.com", "Admin", "John", "Doe");

        // Assert
        Assert.That(token, Is.Not.Null);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        Assert.That(jwtToken.Issuer, Is.EqualTo("HotelManagementAPI")); // The fallback value
        Assert.That(jwtToken.Audiences, Contains.Item("HotelManagementClients")); // The fallback value
    }

    [Test]
    public async Task RegisterAsync_ShouldReturnError_IfUserExists()
    {
        // Arrange
        var request = new RegisterRequestDTO { Email = "existing@example.com", Password = "Pass123", FirstName = "A", LastName = "B" };
        _mockUserRepository.Setup(repo => repo.GetByEmailAsync(request.Email))
            .ReturnsAsync(new User { Email = "existing@example.com" });

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("already exists"));
    }

    [Test]
    public async Task RegisterAsync_ShouldRegisterSuccessfully_IfNewUser()
    {
        // Arrange
        var request = new RegisterRequestDTO { Email = "new@example.com", Password = "Pass123", FirstName = "A", LastName = "B" };
        _mockUserRepository.Setup(repo => repo.GetByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);
            
        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Message, Does.Contain("successfully"));
        _mockUserRepository.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Once);
        _mockUserRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task LoginAsync_ShouldReturnError_IfUserNotFound()
    {
        // Arrange
        var request = new LoginRequestDTO { Email = "missing@example.com", Password = "Pass123" };
        _mockUserRepository.Setup(repo => repo.GetByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("Invalid email or password"));
    }

    [Test]
    public async Task LoginAsync_ShouldReturnError_IfPasswordIsIncorrect()
    {
        // Arrange
        var request = new LoginRequestDTO { Email = "user@example.com", Password = "WrongPassword" };
        var existingUser = new User 
        { 
            Email = "user@example.com", 
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword") 
        };
        _mockUserRepository.Setup(repo => repo.GetByEmailAsync(request.Email))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("Invalid email or password"));
    }

    [Test]
    public async Task LoginAsync_ShouldSucceed_IfCredentialsAreValid()
    {
        // Arrange
        var request = new LoginRequestDTO { Email = "user@example.com", Password = "CorrectPassword" };
        var existingUser = new User 
        { 
            Email = "user@example.com", 
            Role = "RegisteredUser",
            FirstName = "John",
            LastName = "Doe",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword") 
        };
        _mockUserRepository.Setup(repo => repo.GetByEmailAsync(request.Email))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        Assert.That(result.Success, Is.True);
        Assert.That(result.Token, Is.Not.Null.And.Not.Empty);
        Assert.That(result.Role, Is.EqualTo("RegisteredUser"));
        Assert.That(result.FirstName, Is.EqualTo("John"));
        Assert.That(result.LastName, Is.EqualTo("Doe"));
    }
}
