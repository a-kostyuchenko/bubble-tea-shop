using Aspirant.Hosting;
using HealthChecks.NpgSql;
using HealthChecks.RabbitMQ;
using HealthChecks.Redis;
using HealthChecks.Uris;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AppHost.Extensions;

/// <summary>
/// Ref: https://github.com/davidfowl/WaitForDependenciesAspire/tree/main/WaitForDependencies.Aspire.Hosting
/// </summary>
public static class Extensions
{
    public static IResourceBuilder<RabbitMQServerResource> WithHealthCheck(
        this IResourceBuilder<RabbitMQServerResource> builder) =>
        builder.WithAnnotation(HealthCheckAnnotation.Create(
            cs => new RabbitMQHealthCheck(new RabbitMQHealthCheckOptions { ConnectionUri = new Uri(cs) })));

    public static IResourceBuilder<RedisResource> WithHealthCheck(this IResourceBuilder<RedisResource> builder) =>
        builder.WithAnnotation(HealthCheckAnnotation.Create(cs => new RedisHealthCheck(cs)));

    public static IResourceBuilder<PostgresServerResource> WithHealthCheck(
        this IResourceBuilder<PostgresServerResource> builder) =>
        builder.WithAnnotation(HealthCheckAnnotation.Create(
            cs => new NpgSqlHealthCheck(new NpgSqlHealthCheckOptions(cs))));

    public static IResourceBuilder<T> WithHealthCheck<T>(
        this IResourceBuilder<T> builder,
        string? endpointName = null,
        string path = "health",
        Action<UriHealthCheckOptions>? configure = null)
        where T : IResourceWithEndpoints =>
        builder.WithAnnotation(new HealthCheckAnnotation((resource, _) =>
        {
            if (resource is not IResourceWithEndpoints resourceWithEndpoints)
            {
                return Task.FromResult<IHealthCheck?>(null);
            }

            EndpointReference? endpoint = endpointName is null
                ? resourceWithEndpoints.GetEndpoints().FirstOrDefault(e => e.Scheme is "http" or "https")
                : resourceWithEndpoints.GetEndpoint(endpointName);

            string? url = endpoint?.Url;

            if (url is null)
            {
                return Task.FromResult<IHealthCheck?>(null);
            }

            var options = new UriHealthCheckOptions();

            options.AddUri(new Uri(new Uri(url), path));

            configure?.Invoke(options);

            var client = new HttpClient();
            return Task.FromResult<IHealthCheck?>(new UriHealthCheck(options, () => client));
        }));
}
