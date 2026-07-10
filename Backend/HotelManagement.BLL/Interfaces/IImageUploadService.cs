using HotelManagement.DAL.Entities;

namespace HotelManagement.BLL.Interfaces;

public interface IImageUploadService
{
    Task<UploadRequestResult> RequestUploadAsync(
        UploadEntityType entityType,
        string fileName,
        string declaredContentType,
        long declaredSizeBytes,
        string userEmail,
        int? existingEntityId = null);

    Task ConfirmUploadAsync(Guid sessionId, string userEmail);

    Task<UploadStatusResult> GetStatusAsync(Guid sessionId, string userEmail);

    Task AttachToEntityAsync(
        IEnumerable<string> urls,
        UploadEntityType entityType,
        int entityId,
        string userEmail);

    Task<int> CountPendingOrConfirmedForEntityAsync(
        UploadEntityType entityType,
        int? entityId);
}

public class UploadRequestResult
{
    public Guid SessionId { get; set; }
    public string UploadUrl { get; set; } = string.Empty;
    public string BlobUrl { get; set; } = string.Empty;
    public DateTimeOffset ExpiresOn { get; set; }
}

public class UploadStatusResult
{
    public UploadStatus Status { get; set; }
    public string? RejectionReason { get; set; }
}
