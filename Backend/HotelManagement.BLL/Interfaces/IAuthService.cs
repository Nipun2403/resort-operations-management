using HotelManagement.BLL.DTOs;

namespace HotelManagement.BLL.Interfaces;

public interface IAuthService
{
    string GenerateJwtToken(string email, string role, string firstName, string lastName);
    Task<AuthResponseDTO> RegisterAsync(RegisterRequestDTO request);
    Task<AuthResponseDTO> LoginAsync(LoginRequestDTO request);
}
