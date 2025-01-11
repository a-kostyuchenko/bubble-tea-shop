using Microsoft.AspNetCore.Routing;

namespace BubbleTea.Common.Presentation.Endpoints;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
