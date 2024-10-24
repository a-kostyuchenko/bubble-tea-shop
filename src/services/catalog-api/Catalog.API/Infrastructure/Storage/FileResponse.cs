namespace Catalog.API.Infrastructure.Storage;

public sealed record FileResponse(Stream Stream, string ContentType);
