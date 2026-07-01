using HotelManagement.DAL.Context;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.API.Services;

public class IdempotencyCleanupService(IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromHours(6));
        while (await timer.WaitForNextTickAsync(ct))
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var cutoff = DateTime.UtcNow.AddHours(-48);
            await db.IdempotentRequests
                .Where(r => r.CreatedAt < cutoff)
                .ExecuteDeleteAsync(ct);
        }
    }
}
