using BubbleTea.ServiceDefaults.Domain;

namespace BubbleTea.Services.Catalog.API.Infrastructure.Storage;

public interface IBlobService
{
    Task<Guid> UploadAsync(Stream stream, string contentType, CancellationToken cancellationToken = default);
    Task<Result<FileResponse>> DownloadAsync(Guid fileId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid fileId, CancellationToken cancellationToken = default);
}
