using ServiceDefaults.Domain;

namespace Catalog.API.Infrastructure.Storage;

public static class FileErrors
{
    public static Error NotFound(Guid fileId) => Error.NotFound(
        "File.NotFound",
        $"The file with the identifier {fileId} was not found");
}
