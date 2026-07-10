namespace HotelManagement.BLL.Options;

public class AzureStorageOptions
{
    public const string SectionName = "AzureStorage";

    public string AccountUrl { get; set; } = string.Empty;
    public string ContainerName { get; set; } = "images";
    public string QueueName { get; set; } = "image-validation-queue";
    public string AccountKey { get; set; } = string.Empty;
    public int SasExpiryMinutes { get; set; } = 15;
    public long MaxSizeBytes { get; set; } = 10 * 1024 * 1024; // 10 MB
    public int MaxImagesPerRoomType { get; set; } = 5;
    public string[] AllowedExtensions { get; set; } = { ".jpg", ".jpeg", ".png", ".webp" };
}
