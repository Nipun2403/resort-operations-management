using Azure.Storage.Blobs;
using Azure.Storage.Queues;

namespace HotelManagement.API.Utilities;

public interface IAzureCredentialFactory
{
    BlobServiceClient CreateBlobServiceClient();
    QueueServiceClient CreateQueueServiceClient();
}
