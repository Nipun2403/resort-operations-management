namespace HotelManagement.DAL.Context;

public interface IAuditUserProvider
{
    string? GetUserEmail();
    string? GetUserName();
}
