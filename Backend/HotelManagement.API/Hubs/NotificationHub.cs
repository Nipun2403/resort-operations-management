using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace HotelManagement.API.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        // Get user role from JWT claims
        var role = Context.User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
        
        if (!string.IsNullOrEmpty(role))
        {
            // Group connections by Role (e.g. "HousekeepingGroup", "MaintenanceGroup")
            await Groups.AddToGroupAsync(Context.ConnectionId, $"{role}Group");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var role = Context.User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
        
        if (!string.IsNullOrEmpty(role))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"{role}Group");
        }

        await base.OnDisconnectedAsync(exception);
    }
}
