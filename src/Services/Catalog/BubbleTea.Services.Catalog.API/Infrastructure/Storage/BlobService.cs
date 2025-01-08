using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using BubbleTea.ServiceDefaults.Domain;

namespace BubbleTea.Services.Catalog.API.Infrastructure.Storage;

internal sealed class BlobService(BlobServiceClient blobServiceClient, IConfiguration configuration) : IBlobService
{
    private readonly string _containerName = configuration["Storage:Container"]!;
    
    public async Task<Guid> UploadAsync(Stream stream, string contentType, CancellationToken cancellationToken = default)
    {
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_containerName);

        await containerClient.CreateIfNotExistsAsync(
            PublicAccessType.Blob,
            cancellationToken: cancellationToken);

        var fileId = Guid.NewGuid();
        
        BlobClient blobClient = containerClient.GetBlobClient(fileId.ToString());
        
        await blobClient.UploadAsync(
            stream,
            new BlobHttpHeaders { ContentType = contentType },
            cancellationToken: cancellationToken);

        return fileId;
    }

    public async Task<Result<FileResponse>> DownloadAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
        
        BlobClient blobClient = containerClient.GetBlobClient(fileId.ToString());

        Response<BlobDownloadResult>? response = await blobClient.DownloadContentAsync(cancellationToken: cancellationToken);

        if (!response.HasValue)
        {
            return Result.Failure<FileResponse>(FileErrors.NotFound(fileId));
        }

        return new FileResponse(response.Value.Content.ToStream(), response.Value.Details.ContentType);
    }

    public async Task DeleteAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
        
        BlobClient blobClient = containerClient.GetBlobClient(fileId.ToString());

        await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }
}
