using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using ServiceDefaults.Domain;

namespace ServiceDefaults.Behaviors;

public sealed class RequestLoggingBehavior<TRequest, TResponse>(
    ILogger<RequestLoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        string serviceName = GetServiceName(typeof(TRequest).FullName!);
        string requestName = typeof(TRequest).Name;

        Activity.Current?.SetTag("request.service", serviceName);
        Activity.Current?.SetTag("request.name", requestName);

        logger.LogInformation("Processing request {RequestName}", requestName);

        TResponse result = await next();

        if (result.IsSuccess)
        {
            logger.LogInformation("Completed request {RequestName}", requestName);
        }
        else
        {
            logger.LogError("Completed request {RequestName} with error {Error}", requestName, result.Error);
        }

        return result;
    }

    private static string GetServiceName(string requestName) => requestName.Split(".")[0];
}
