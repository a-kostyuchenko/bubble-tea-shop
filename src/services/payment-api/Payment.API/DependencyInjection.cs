using Asp.Versioning;
using ServiceDefaults.Endpoints;

namespace Payment.API;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options => options.CustomSchemaIds(s => s.FullName?.Replace("+", ".")));
        
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
        
        return services;
    }
}
