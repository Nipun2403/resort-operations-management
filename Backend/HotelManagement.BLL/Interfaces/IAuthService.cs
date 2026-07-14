using HotelManagement.BLL.DTOs;

namespace HotelManagement.BLL.Interfaces;

public interface IAuthService
{
    string GenerateJwtToken(int userId, string email, string role, string firstName, string lastName);
    Task<AuthResponseDTO> RegisterAsync(RegisterRequestDTO request);
    Task<AuthResponseDTO> LoginAsync(LoginRequestDTO request);

    Task<UserProfileDTO> GetProfileByEmailAsync(string email);
    Task<UserProfileDTO> UpdateProfileAsync(string email, UpdateProfileDTO dto);
    Task ChangePasswordAsync(string email, ChangePasswordDTO dto);
}