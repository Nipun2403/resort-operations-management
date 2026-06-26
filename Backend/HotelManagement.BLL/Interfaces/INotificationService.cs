namespace HotelManagement.BLL.Interfaces;

public interface INotificationService
{
    Task SendHousekeepingAlertAsync(string message);
    Task SendMaintenanceAlertAsync(string message);
    Task SendKitchenAlertAsync(string message);
    Task SendGeneralAlertAsync(string role, string message);
}
