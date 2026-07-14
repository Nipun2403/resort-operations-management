using HotelManagement.BLL.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HotelManagement.BLL.Workers;

public class FeedbackReminderWorker(IServiceScopeFactory scopeFactory, ILogger<FeedbackReminderWorker> logger) : BackgroundService
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(24);

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        using var timer = new PeriodicTimer(CheckInterval);

        while (await timer.WaitForNextTickAsync(ct))
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var reminderService = scope.ServiceProvider.GetRequiredService<IFeedbackReminderService>();
                await reminderService.ProcessDueRemindersAsync(ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error in FeedbackReminderWorker.");
            }
        }
    }
}
