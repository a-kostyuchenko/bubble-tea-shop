using Asp.Versioning;
using ServiceDefaults.Endpoints;
using ServiceDefaults.OpenApi;

namespace Payment.API;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1);
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'V";
            options.SubstituteApiVersionInUrl = true;
        });
        
        services.AddEndpoints(AssemblyReference.Assembly);
        
        services.AddApiDocumentation();
        
        return services;
    }
    
    private static void AddApiDocumentation(this IServiceCollection services)
    {
        services.AddOpenApi();
        services.ConfigureOptions<ScalarOptionsSetup>();
    }
}
