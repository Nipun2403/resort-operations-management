using System.Text.Json;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Queues;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using HotelManagement.BLL.Options;
using HotelManagement.DAL.Context;
using HotelManagement.DAL.Entities;

namespace HotelManagement.BLL.Workers;

public class ImageValidationWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ImageValidationWorker> _logger;

    public ImageValidationWorker(IServiceScopeFactory scopeFactory, ILogger<ImageValidationWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation("ImageValidationWorker started.");

        while (!ct.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var options = scope.ServiceProvider.GetRequiredService<IOptions<AzureStorageOptions>>().Value;
                var blobServiceClient = scope.ServiceProvider.GetRequiredService<BlobServiceClient>();
                var queueServiceClient = scope.ServiceProvider.GetRequiredService<QueueServiceClient>();
                var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();

                var queueClient = queueServiceClient.GetQueueClient(options.QueueName);

                var messages = await queueClient.ReceiveMessagesAsync(
                    maxMessages: 1,
                    visibilityTimeout: TimeSpan.FromSeconds(30),
                    cancellationToken: ct);

                if (messages.Value.Length == 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2), ct);
                    continue;
                }

                var message = messages.Value[0];
                var body = message.Body.ToString();
                var data = JsonSerializer.Deserialize<Dictionary<string, string>>(body);

                if (data == null || !data.TryGetValue("sessionId", out var sessionIdStr) || !Guid.TryParse(sessionIdStr, out var sessionId))
                {
                    _logger.LogWarning("Invalid queue message: {Body}", body);
                    await queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt, ct);
                    continue;
                }

                await using var db = await dbFactory.CreateDbContextAsync(ct);
                var session = await db.Set<UploadSession>().FindAsync(new object[] { sessionId }, ct);

                if (session == null || session.Status != UploadStatus.Pending)
                {
                    _logger.LogWarning("Session {SessionId} not found or not pending.", sessionId);
                    await queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt, ct);
                    continue;
                }

                var containerClient = blobServiceClient.GetBlobContainerClient(options.ContainerName);
                var blobClient = containerClient.GetBlobClient(session.BlobName);

                try
                {
                    var response = await blobClient.DownloadStreamingAsync(
                        new BlobDownloadOptions { Range = new HttpRange(0, 512) }, cancellationToken: ct);

                    await using var stream = response.Value.Content;
                    using var memoryStream = new MemoryStream();
                    await stream.CopyToAsync(memoryStream, ct);
                    var header = memoryStream.ToArray();

                    var ext = Path.GetExtension(session.BlobName).ToUpperInvariant();
                    var valid = ValidateMagicBytes(header, ext);

                    if (!valid)
                    {
                        await RejectSession(db, blobClient, session, "File content does not match declared format.", _logger, ct);
                        await queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt, ct);
                        continue;
                    }

                    var properties = await blobClient.GetPropertiesAsync(cancellationToken: ct);
                    if (properties.Value.ContentLength > options.MaxSizeBytes)
                    {
                        await RejectSession(db, blobClient, session, $"File exceeds maximum size of {options.MaxSizeBytes} bytes.", _logger, ct);
                        await queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt, ct);
                        continue;
                    }

                    session.Status = UploadStatus.Confirmed;
                    session.ActualSizeBytes = properties.Value.ContentLength;
                    session.ConfirmedAtUtc = DateTime.UtcNow;
                    db.Set<UploadSession>().Update(session);
                    await db.SaveChangesAsync(ct);

                    _logger.LogInformation("Session {SessionId} confirmed.", sessionId);
                    await queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error validating session {SessionId}", sessionId);
                    await queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt, ct);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in ImageValidationWorker.");
                await Task.Delay(TimeSpan.FromSeconds(5), ct);
            }
        }
    }

    private static bool ValidateMagicBytes(byte[] header, string extension)
    {
        return extension.ToUpperInvariant() switch
        {
            ".JPG" or ".JPEG" => header.Length >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
            ".PNG" => header.Length >= 8
                && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47
                && header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A,
            ".WEBP" => header.Length >= 12
                && header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46
                && header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50,
            _ => false
        };
    }

    private static async Task RejectSession(
        ApplicationDbContext db,
        BlobClient blobClient,
        UploadSession session,
        string reason,
        ILogger<ImageValidationWorker> logger,
        CancellationToken ct)
    {
        session.Status = UploadStatus.Rejected;
        session.RejectionReason = reason;
        db.Set<UploadSession>().Update(session);
        await db.SaveChangesAsync(ct);

        try
        {
            await blobClient.DeleteIfExistsAsync(cancellationToken: ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to delete blob for rejected session {SessionId}", session.Id);
        }
    }
}
