using ServiceDefaults.Domain;

namespace Catalog.API.Entities.BubbleTeas;

public static class BubbleTeaErrors
{
    public static Error NotFound(Guid bubbleTeaId) => Error.NotFound(
        "BubbleTea.NotFound",
        $"The bubble tea with the identifier {bubbleTeaId} was not found");
}
