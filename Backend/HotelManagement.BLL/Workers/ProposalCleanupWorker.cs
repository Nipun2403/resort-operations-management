using HotelManagement.Repository.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HotelManagement.BLL.Workers;

public class ProposalCleanupWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ProposalCleanupWorker> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(1);

    public ProposalCleanupWorker(IServiceScopeFactory scopeFactory, ILogger<ProposalCleanupWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ProposalCleanupWorker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_interval, stoppingToken);

                using var scope = _scopeFactory.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<IConciergeProposalRepository>();
                var deletedCount = await repo.CleanupExpiredAsync(stoppingToken);
                
                if (deletedCount > 0)
                {
                    _logger.LogInformation("Cleaned up {Count} expired proposals", deletedCount);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ProposalCleanupWorker");
            }
        }

        _logger.LogInformation("ProposalCleanupWorker stopped");
    }
}