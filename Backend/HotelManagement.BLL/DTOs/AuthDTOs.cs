using System.ComponentModel.DataAnnotations;

namespace HotelManagement.BLL.DTOs;

public class RegisterRequestDTO
{
    [Required, EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6)]
    [StringLength(100)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;
}

public class StaffRegisterRequestDTO : RegisterRequestDTO
{
    [Required]
    [StringLength(100)]
    public string Role { get; set; } = string.Empty;
}

public class LoginRequestDTO
{
    [Required, EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Password { get; set; } = string.Empty;
}

public class AuthResponseDTO
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Token { get; set; }
    public string? Role { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}
