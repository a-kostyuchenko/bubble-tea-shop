using BubbleTea.Common.Domain;

namespace BubbleTea.Services.Catalog.API.Entities.Parameters;

public static class ParameterErrors
{
    public static Error NotFound(Guid parameterId) => Error.NotFound(
        "Parameter.NotFound",
        $"The parameter with the identifier {parameterId} was not found");
}
