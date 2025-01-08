using Microsoft.AspNetCore.Routing;

namespace BubbleTea.ServiceDefaults.Endpoints;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
