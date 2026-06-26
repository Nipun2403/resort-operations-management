using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HotelManagement.BLL.DTOs;
using HotelManagement.BLL.Interfaces;
using HotelManagement.DAL.Entities;
using HotelManagement.Repository.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace HotelManagement.BLL.Services;

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly IUserRepository _userRepository;

    public AuthService(IConfiguration configuration, IUserRepository userRepository)
    {
        _configuration = configuration;
        _userRepository = userRepository;
    }

    public string GenerateJwtToken(string email, string role, string firstName, string lastName)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        // In production, this key must be securely stored (e.g. Azure Key Vault, Environment Variables)
        var keyString = _configuration["Jwt:Key"] ?? "super_secret_fallback_key_that_should_be_long_enough_for_hmacsha256_hotel_management";
        var key = Encoding.ASCII.GetBytes(keyString);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, email),
                new Claim(ClaimTypes.Role, role),
                new Claim(ClaimTypes.GivenName, firstName),
                new Claim(ClaimTypes.Surname, lastName)
            }),
            Expires = DateTime.UtcNow.AddHours(4), // Short lifespan for stateless JWT
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = _configuration["Jwt:Issuer"] ?? "HotelManagementAPI",
            Audience = _configuration["Jwt:Audience"] ?? "HotelManagementClients"
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public async Task<AuthResponseDTO> RegisterAsync(RegisterRequestDTO request)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
            return new AuthResponseDTO { Success = false, Message = "A user with this email already exists." };

        var user = new User
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Role = "RegisteredUser"
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        return new AuthResponseDTO { Success = true, Message = "User registered successfully." };
    }

    public async Task<AuthResponseDTO> LoginAsync(LoginRequestDTO request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return new AuthResponseDTO { Success = false, Message = "Invalid email or password." };

        var token = GenerateJwtToken(user.Email, user.Role, user.FirstName, user.LastName);

        return new AuthResponseDTO
        {
            Success = true,
            Message = "Login successful.",
            Token = token,
            Role = user.Role,
            FirstName = user.FirstName,
            LastName = user.LastName
        };
    }
}
