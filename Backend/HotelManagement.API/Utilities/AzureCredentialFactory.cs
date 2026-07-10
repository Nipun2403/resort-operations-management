using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Microsoft.Extensions.Options;
using HotelManagement.BLL.Options;

namespace HotelManagement.API.Utilities;

public class AzureCredentialFactory : IAzureCredentialFactory
{
    private readonly AzureStorageOptions _options;
    private readonly StorageSharedKeyCredential _credential;

    public AzureCredentialFactory(IOptions<AzureStorageOptions> options)
    {
        _options = options.Value;
        var accountName = new Uri(_options.AccountUrl).Host.Split('.')[0];
        _credential = new StorageSharedKeyCredential(accountName, _options.AccountKey);
    }

    public BlobServiceClient CreateBlobServiceClient()
    {
        return new BlobServiceClient(new Uri(_options.AccountUrl), _credential);
    }

    public QueueServiceClient CreateQueueServiceClient()
    {
        var queueUrl = _options.AccountUrl.Replace(".blob.core.windows.net", ".queue.core.windows.net");
        return new QueueServiceClient(new Uri(queueUrl), _credential);
    }
}
