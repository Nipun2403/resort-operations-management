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

public class BlobCleanupWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BlobCleanupWorker> _logger;

    public BlobCleanupWorker(IServiceScopeFactory scopeFactory, ILogger<BlobCleanupWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation("BlobCleanupWorker started.");

        try
        {
            await CleanupOrphanedBlobs(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Initial blob cleanup run failed.");
        }

        while (!ct.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromHours(1), ct);
                await CleanupOrphanedBlobs(ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in BlobCleanupWorker.");
            }
        }
    }

    private async Task CleanupOrphanedBlobs(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<AzureStorageOptions>>().Value;
        var blobServiceClient = scope.ServiceProvider.GetRequiredService<BlobServiceClient>();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var containerClient = blobServiceClient.GetBlobContainerClient(options.ContainerName);

        var blobPrefix = $"{options.AccountUrl.TrimEnd('/')}/{options.ContainerName}/";

        string? ExtractBlobName(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;
            return url.StartsWith(blobPrefix, StringComparison.OrdinalIgnoreCase)
                ? url[blobPrefix.Length..]
                : null;
        }

        var activeBlobNames = await db.Set<UploadSession>()
            .Where(s => s.Status == UploadStatus.Pending
                || s.Status == UploadStatus.Confirmed
                || s.Status == UploadStatus.Attached)
            .Select(s => s.BlobName)
            .ToHashSetAsync(ct);

        var amenityUrls = await db.Set<Amenity>()
            .Where(a => a.ImageUrl != null)
            .Select(a => a.ImageUrl!)
            .ToListAsync(ct);
        foreach (var url in amenityUrls)
        {
            var name = ExtractBlobName(url);
            if (name != null) activeBlobNames.Add(name);
        }

        var menuItemUrls = await db.Set<MenuItem>()
            .Where(m => m.ImageUrl != null)
            .Select(m => m.ImageUrl!)
            .ToListAsync(ct);
        foreach (var url in menuItemUrls)
        {
            var name = ExtractBlobName(url);
            if (name != null) activeBlobNames.Add(name);
        }

        var roomTypes = await db.Set<RoomType>()
            .Where(r => r.ImageUrls != null)
            .ToListAsync(ct);
        foreach (var rt in roomTypes)
        {
            if (rt.ImageUrls == null) continue;
            foreach (var url in rt.ImageUrls)
            {
                var name = ExtractBlobName(url);
                if (name != null) activeBlobNames.Add(name);
            }
        }

        var deletedCount = 0;

        await foreach (var blobItem in containerClient.GetBlobsAsync(cancellationToken: ct))
        {
            if (activeBlobNames.Contains(blobItem.Name))
                continue;

            var blobClient = containerClient.GetBlobClient(blobItem.Name);
            var deleted = await blobClient.DeleteIfExistsAsync(cancellationToken: ct);

            if (deleted)
            {
                var session = await db.Set<UploadSession>()
                    .FirstOrDefaultAsync(s => s.BlobName == blobItem.Name, ct);
                if (session != null)
                {
                    session.Status = UploadStatus.Expired;
                }
                deletedCount++;
            }
        }

        if (deletedCount > 0)
        {
            await db.SaveChangesAsync(ct);
            _logger.LogInformation("Blob cleanup: deleted {Count} orphaned blobs.", deletedCount);
        }
    }
}
