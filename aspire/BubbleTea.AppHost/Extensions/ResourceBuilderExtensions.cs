using System.Diagnostics;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BubbleTea.AppHost.Extensions;

public static class ResourceBuilderExtensions
{
    public static IResourceBuilder<T> WithOpenApiDocs<T>(this IResourceBuilder<T> builder,
        string name,
        string displayName,
        string openApiUiPath)
    where T : IResourceWithEndpoints
    {
        builder.WithCommand(
            name,
            displayName,
            executeCommand: _ =>
            {
                try
                {
                    EndpointReference endpoint = builder.GetEndpoint("https");
                    
                    string url = $"{endpoint.Url}/{openApiUiPath}";
                    
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                    
                    return Task.FromResult(new ExecuteCommandResult { Success = true });
                }
                catch (Exception e)
                {
                    return Task.FromResult(new ExecuteCommandResult { Success = false, ErrorMessage = e.Message});
                }
            },
            updateState: context => context.ResourceSnapshot.HealthStatus == HealthStatus.Healthy ?
                ResourceCommandState.Enabled : ResourceCommandState.Disabled,
            iconName: "Document",
            iconVariant: IconVariant.Filled);
        
        return builder;
    }
}
