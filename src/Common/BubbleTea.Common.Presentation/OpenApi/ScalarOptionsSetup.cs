using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;

namespace BubbleTea.Common.Presentation.OpenApi;

public sealed class ScalarOptionsSetup(IConfiguration configuration) : IConfigureOptions<ScalarOptions>
{
    public void Configure(ScalarOptions options)
    {
        options
            .WithTitle(configuration["OpenApi:Title"]!)
            .WithTheme(ScalarTheme.DeepSpace)
            // .WithOpenApiRoutePattern(configuration["OpenApi:RoutePattern"]!)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    }
}
