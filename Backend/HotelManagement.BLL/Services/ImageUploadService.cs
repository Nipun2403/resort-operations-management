using System.Text.Json;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Azure.Storage.Sas;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using HotelManagement.BLL.Interfaces;
using HotelManagement.BLL.Options;
using HotelManagement.DAL.Context;
using HotelManagement.DAL.Entities;

namespace HotelManagement.BLL.Services;

public class ImageUploadService : IImageUploadService
{
    private readonly AzureStorageOptions _options;
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly BlobServiceClient _blobServiceClient;
    private readonly QueueServiceClient _queueServiceClient;

    public ImageUploadService(
        IOptions<AzureStorageOptions> options,
        IDbContextFactory<ApplicationDbContext> contextFactory,
        BlobServiceClient blobServiceClient,
        QueueServiceClient queueServiceClient)
    {
        _options = options.Value;
        _contextFactory = contextFactory;
        _blobServiceClient = blobServiceClient;
        _queueServiceClient = queueServiceClient;
    }

    public async Task<UploadRequestResult> RequestUploadAsync(
        UploadEntityType entityType,
        string fileName,
        string declaredContentType,
        long declaredSizeBytes,
        string userEmail,
        int? existingEntityId = null)
    {
        var ext = Path.GetExtension(fileName).ToUpperInvariant();

        if (!_options.AllowedExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException($"File extension '{ext}' is not allowed.");

        if (declaredSizeBytes > _options.MaxSizeBytes)
            throw new InvalidOperationException($"File size exceeds the maximum of {_options.MaxSizeBytes} bytes.");

        if (entityType == UploadEntityType.RoomType && existingEntityId.HasValue)
        {
            var pendingCount = await CountPendingOrConfirmedForEntityAsync(entityType, existingEntityId.Value);
            if (pendingCount >= _options.MaxImagesPerRoomType)
                throw new InvalidOperationException($"Room type can have at most {_options.MaxImagesPerRoomType} images.");
        }

        var entityTypeStr = entityType.ToString().ToLower();
        var blobName = $"{entityTypeStr}/{Guid.NewGuid()}{ext}";
        var blobUrl = $"{_options.AccountUrl}/{_options.ContainerName}/{blobName}";

        var session = new UploadSession
        {
            Id = Guid.NewGuid(),
            BlobName = blobName,
            BlobUrl = blobUrl,
            DeclaredContentType = declaredContentType,
            DeclaredSizeBytes = declaredSizeBytes,
            Status = UploadStatus.Pending,
            EntityType = entityType,
            UploadedByEmail = userEmail,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(_options.SasExpiryMinutes)
        };

        using var db = await _contextFactory.CreateDbContextAsync();
        db.Add(session);
        await db.SaveChangesAsync();

        var blobContainerClient = _blobServiceClient.GetBlobContainerClient(_options.ContainerName);
        var blobClient = blobContainerClient.GetBlobClient(blobName);

        var sasUri = GenerateServiceSas(blobClient, out var expiresOn);

        return new UploadRequestResult
        {
            SessionId = session.Id,
            UploadUrl = sasUri.AbsoluteUri,
            BlobUrl = blobUrl,
            ExpiresOn = expiresOn
        };
    }

    public async Task ConfirmUploadAsync(Guid sessionId, string userEmail)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        var session = await db.Set<UploadSession>().FindAsync(sessionId);

        if (session == null)
            throw new KeyNotFoundException("Upload session not found.");

        if (session.UploadedByEmail != userEmail)
            throw new UnauthorizedAccessException("You can only confirm your own uploads.");

        if (session.Status != UploadStatus.Pending)
            throw new InvalidOperationException($"Upload session is already {session.Status}.");

        var queueClient = _queueServiceClient.GetQueueClient(_options.QueueName);
        var message = JsonSerializer.Serialize(new { sessionId = sessionId.ToString() });
        await queueClient.SendMessageAsync(message);
    }

    public async Task<UploadStatusResult> GetStatusAsync(Guid sessionId, string userEmail)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        var session = await db.Set<UploadSession>().FindAsync(sessionId);

        if (session == null)
            throw new KeyNotFoundException("Upload session not found.");

        if (session.UploadedByEmail != userEmail)
            throw new UnauthorizedAccessException("You can only check your own uploads.");

        return new UploadStatusResult
        {
            Status = session.Status,
            RejectionReason = session.RejectionReason
        };
    }

    public async Task AttachToEntityAsync(
        IEnumerable<string> urls,
        UploadEntityType entityType,
        int entityId,
        string userEmail)
    {
        var storageHost = new Uri(_options.AccountUrl).Host;

        foreach (var url in urls)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                continue;

            if (uri.Host != storageHost)
                continue;

            using var db = await _contextFactory.CreateDbContextAsync();
            var session = await db.Set<UploadSession>()
                .FirstOrDefaultAsync(s => s.BlobUrl == url);

            if (session == null)
                throw new InvalidOperationException($"Upload session not found for URL: {url}");

            if (session.UploadedByEmail != userEmail)
                throw new UnauthorizedAccessException($"Upload session {session.Id} was created by a different user.");

            if (session.Status != UploadStatus.Confirmed && session.Status != UploadStatus.Attached)
                throw new InvalidOperationException($"Upload session {session.Id} is not confirmed (status: {session.Status}).");

            if (session.EntityType != entityType)
                throw new InvalidOperationException($"Upload session {session.Id} is for a different entity type.");

            if (session.AttachedEntityId.HasValue && session.AttachedEntityId.Value != entityId)
                throw new InvalidOperationException($"Upload session {session.Id} is already attached to a different entity.");

            session.Status = UploadStatus.Attached;
            session.AttachedEntityId = entityId;
            db.Set<UploadSession>().Update(session);
            await db.SaveChangesAsync();
        }
    }

    public async Task<int> CountPendingOrConfirmedForEntityAsync(UploadEntityType entityType, int? entityId)
    {
        if (!entityId.HasValue)
            return 0;

        using var db = await _contextFactory.CreateDbContextAsync();
        return await db.Set<UploadSession>()
            .CountAsync(s => s.EntityType == entityType
                && s.AttachedEntityId == entityId.Value
                && s.Status != UploadStatus.Expired
                && s.Status != UploadStatus.Rejected);
    }

    private Uri GenerateServiceSas(BlobClient blobClient, out DateTimeOffset expiresOn)
    {
        expiresOn = DateTimeOffset.UtcNow.AddMinutes(_options.SasExpiryMinutes);
        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _options.ContainerName,
            BlobName = blobClient.Name,
            Resource = "b",
            ExpiresOn = expiresOn
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Write | BlobSasPermissions.Create);

        return blobClient.GenerateSasUri(sasBuilder);
    }
}
