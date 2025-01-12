using BubbleTea.Common.Presentation.OpenApi;
using Microsoft.Extensions.DependencyInjection;

namespace BubbleTea.Common.Presentation.Endpoints;

public static class OpenApiExtensions
{
    public static IServiceCollection AddDocumentation(this IServiceCollection services)
    {
        services.AddOpenApi();
        services.ConfigureOptions<ScalarOptionsSetup>();
        
        return services;
    }
}
