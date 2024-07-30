using Microsoft.AspNetCore.Routing;

namespace ServiceDefaults.Endpoints;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
