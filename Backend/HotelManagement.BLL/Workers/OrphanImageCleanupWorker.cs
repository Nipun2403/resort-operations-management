using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using HotelManagement.BLL.Options;
using HotelManagement.DAL.Context;
using HotelManagement.DAL.Entities;

namespace HotelManagement.BLL.Workers;

public class OrphanImageCleanupWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OrphanImageCleanupWorker> _logger;

    public OrphanImageCleanupWorker(IServiceScopeFactory scopeFactory, ILogger<OrphanImageCleanupWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation("OrphanImageCleanupWorker started.");

        using var timer = new PeriodicTimer(TimeSpan.FromHours(1));

        while (await timer.WaitForNextTickAsync(ct))
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var options = scope.ServiceProvider.GetRequiredService<IOptions<AzureStorageOptions>>().Value;
                var blobServiceClient = scope.ServiceProvider.GetRequiredService<BlobServiceClient>();
                var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();

                await using var db = await dbFactory.CreateDbContextAsync(ct);
                var now = DateTime.UtcNow;

                var expiredPending = await db.Set<UploadSession>()
                    .Where(s => s.Status == UploadStatus.Pending && s.CreatedAtUtc < now.AddHours(-1))
                    .ToListAsync(ct);

                foreach (var session in expiredPending)
                {
                    session.Status = UploadStatus.Expired;
                }

                var expiredOrphaned = await db.Set<UploadSession>()
                    .Where(s => (s.Status == UploadStatus.Confirmed || s.Status == UploadStatus.Rejected)
                        && s.AttachedEntityId == null
                        && s.CreatedAtUtc < now.AddHours(-24))
                    .ToListAsync(ct);

                foreach (var session in expiredOrphaned)
                {
                    session.Status = UploadStatus.Expired;
                }

                var allExpired = expiredPending.Concat(expiredOrphaned).ToList();
                if (allExpired.Count > 0)
                {
                    await db.SaveChangesAsync(ct);

                    var containerClient = blobServiceClient.GetBlobContainerClient(options.ContainerName);

                    foreach (var session in allExpired)
                    {
                        try
                        {
                            var blobClient = containerClient.GetBlobClient(session.BlobName);
                            await blobClient.DeleteIfExistsAsync(cancellationToken: ct);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to delete blob {BlobName} for expired session {SessionId}", session.BlobName, session.Id);
                        }
                    }

                    _logger.LogInformation("Orphan cleanup: expired {Count} sessions and deleted blobs.", allExpired.Count);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OrphanImageCleanupWorker.");
            }
        }
    }
}
