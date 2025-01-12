using BubbleTea.Common.Application.Caching;
using BubbleTea.Common.Application.Clock;
using BubbleTea.Common.Application.Data;
using BubbleTea.Common.Application.EventBus;
using BubbleTea.Common.Infrastructure.Caching;
using BubbleTea.Common.Infrastructure.Clock;
using BubbleTea.Common.Infrastructure.Data;
using BubbleTea.Common.Infrastructure.Outbox;
using Hangfire;
using Hangfire.PostgreSql;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

namespace BubbleTea.Common.Infrastructure;

public static class InfrastructureConfiguration
{
    public static WebApplicationBuilder AddDatabase<TDbContext>(
        this WebApplicationBuilder builder,
        string connectionName,
        string? schema = null) 
        where TDbContext : DbContext
    {
        builder.AddNpgsqlDbContext<TDbContext>(
            connectionName,
            _ => {},
            optionsBuilder =>
            {
                optionsBuilder.UseSnakeCaseNamingConvention();
        
                optionsBuilder.UseNpgsql(contextOptionsBuilder =>
                    contextOptionsBuilder.MigrationsHistoryTable(
                        HistoryRepository.DefaultTableName,
                        schema));

                // TODO: find a better way to add interceptors
                optionsBuilder.AddInterceptors(
                    builder.Services
                        .BuildServiceProvider()
                        .GetRequiredService<InsertOutboxMessagesInterceptor>());
            });
        
        builder.EnrichNpgsqlDbContext<TDbContext>();
        
        builder.AddNpgsqlDataSource(connectionName);

        return builder;
    }
    
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string serviceName,
        Action<IRegistrationConfigurator, string>[] moduleConfigureConsumers,
        string databaseConnection,
        string redisConnection,
        string queueConnection)
    {
        services.TryAddSingleton<IDateTimeProvider, DateTimeProvider>();

        services.TryAddSingleton<IEventBus, EventBus.EventBus>();

        services.TryAddSingleton<InsertOutboxMessagesInterceptor>();
        
        services.TryAddScoped<IDbConnectionFactory, DbConnectionFactory>();
        
        try
        {
            IConnectionMultiplexer connectionMultiplexer = ConnectionMultiplexer.Connect(redisConnection);
            services.AddSingleton(connectionMultiplexer);
            services.AddStackExchangeRedisCache(options =>
                options.ConnectionMultiplexerFactory = () => Task.FromResult(connectionMultiplexer));
        }
        catch
        {
            services.AddDistributedMemoryCache();
        }

        services.TryAddSingleton<ICacheService, CacheService>();

        services.AddMassTransit(configurator =>
        {
            string instanceId = serviceName.ToLowerInvariant().Replace('.', '-');
            
            foreach (Action<IRegistrationConfigurator, string> configureConsumers in moduleConfigureConsumers)
            {
                configureConsumers(configurator, instanceId);
            }
            
            configurator.SetKebabCaseEndpointNameFormatter();
    
            configurator.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(queueConnection);
                cfg.ConfigureEndpoints(context);
            });
        });
        
        services.TryAddScoped<IEventBus, EventBus.EventBus>();

        services.AddHangfire(config => config.UsePostgreSqlStorage(
            options => options.UseNpgsqlConnection(databaseConnection)));

        services.AddHangfireServer(options =>
            options.SchedulePollingInterval = TimeSpan.FromSeconds(1));

        return services;
    }
}
