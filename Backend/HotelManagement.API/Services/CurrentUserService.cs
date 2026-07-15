using System.Security.Claims;
using HotelManagement.BLL.Interfaces;
using HotelManagement.DAL.Context;
using Microsoft.AspNetCore.Http;

namespace HotelManagement.API.Services;

public class CurrentUserService : ICurrentUserService, IAuditUserProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? GetUserEmail()
    {
        return _httpContextAccessor.HttpContext?.User?.Identity?.Name;
    }

    public string? GetUserName()
    {
        var principal = _httpContextAccessor.HttpContext?.User;
        if (principal == null) return null;

        var firstName = principal.FindFirstValue(ClaimTypes.GivenName);
        var lastName = principal.FindFirstValue(ClaimTypes.Surname);
        
        if (string.IsNullOrEmpty(firstName) && string.IsNullOrEmpty(lastName)) return null;
        return $"{firstName} {lastName}".Trim();
    }

    public bool IsInRole(string role)
    {
        return _httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;
    }

    public bool IsAuthenticated()
    {
        return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
    }

    public int? GetUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(userIdClaim, out var userId))
            return userId;
        return null;
    }
}
