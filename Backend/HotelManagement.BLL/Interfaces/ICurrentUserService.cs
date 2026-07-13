namespace HotelManagement.BLL.Interfaces;

public interface ICurrentUserService
{
    string? GetUserEmail();
    string? GetUserName();
    int? GetUserId();
    bool IsInRole(string role);
    bool IsAuthenticated();
}
