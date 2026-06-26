using HotelManagement.API.Hubs;
using HotelManagement.BLL.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace HotelManagement.API.Services;

public class SignalRNotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public SignalRNotificationService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendHousekeepingAlertAsync(string message)
    {
        await _hubContext.Clients.Group("HousekeepingGroup").SendAsync("ReceiveAlert", message);
        await _hubContext.Clients.Group("MaintenanceGroup").SendAsync("ReceiveAlert", message); // Demo: cross-broadcast
    }

    public async Task SendMaintenanceAlertAsync(string message)
    {
        await _hubContext.Clients.Group("MaintenanceGroup").SendAsync("ReceiveAlert", message);
    }

    public async Task SendKitchenAlertAsync(string message)
    {
        await _hubContext.Clients.Group("KitchenGroup").SendAsync("ReceiveAlert", message);
    }

    public async Task SendGeneralAlertAsync(string role, string message)
    {
        await _hubContext.Clients.Group($"{role}Group").SendAsync("ReceiveAlert", message);
    }
}
