namespace HotelManagement.BLL.Interfaces;

public interface ICurrentUserService
{
    string? GetUserEmail();
    string? GetUserName();
    bool IsInRole(string role);
    bool IsAuthenticated();
}
