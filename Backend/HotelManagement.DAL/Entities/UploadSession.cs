namespace HotelManagement.DAL.Entities;

public enum UploadStatus { Pending, Confirmed, Rejected, Attached, Expired }
public enum UploadEntityType { Amenity, MenuItem, RoomType }

public class UploadSession
{
    public Guid Id { get; set; }
    public string BlobName { get; set; } = string.Empty;
    public string BlobUrl { get; set; } = string.Empty;
    public string DeclaredContentType { get; set; } = string.Empty;
    public long DeclaredSizeBytes { get; set; }
    public long? ActualSizeBytes { get; set; }
    public UploadStatus Status { get; set; } = UploadStatus.Pending;
    public UploadEntityType EntityType { get; set; }
    public int? AttachedEntityId { get; set; }
    public string UploadedByEmail { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ConfirmedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public string? RejectionReason { get; set; }
}
