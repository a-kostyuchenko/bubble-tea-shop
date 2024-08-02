using Catalog.API.Database;
using Catalog.API.EventBus;
using Catalog.API.Outbox;
using FluentValidation;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ServiceDefaults.Endpoints;
using ServiceDefaults.Messaging;

namespace Catalog.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDomainEventHandlers();
        
        services.TryAddScoped<IDbConnectionFactory, DbConnectionFactory>();

        services.AddHangfire(globalConfiguration => globalConfiguration.UseMemoryStorage());

        services.AddHangfireServer(options =>
            options.SchedulePollingInterval = TimeSpan.FromSeconds(1));

        services.TryAddScoped<IEventBus, EventBus.EventBus>();
        
        services.Configure<OutboxOptions>(configuration.GetSection(OutboxOptions.ConfigurationSection));
        
        services.TryAddScoped<IOutboxProcessor, OutboxProcessor>();
        
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(AssemblyReference.Assembly);
        });

        services.AddValidatorsFromAssembly(AssemblyReference.Assembly, includeInternalTypes: true);
        
        services.AddEndpoints(AssemblyReference.Assembly);
        
        return services;
    }
    
    private static void AddDomainEventHandlers(this IServiceCollection services)
    {
        Type[] domainEventHandlers = AssemblyReference.Assembly
            .GetTypes()
            .Where(type => type.IsAssignableTo(typeof(IDomainEventHandler)) &&
                           !type.IsAssignableTo(typeof(IdempotentDomainEventHandler<>)))
            .ToArray();

        foreach (Type domainEventHandler in domainEventHandlers)
        {
            services.TryAddScoped(domainEventHandler);

            Type domainEvent = domainEventHandler
                .GetInterfaces()
                .Single(i => i.IsGenericType)
                .GetGenericArguments()
                .Single();

            Type idempotentHandler = typeof(IdempotentDomainEventHandler<>).MakeGenericType(domainEvent);

            services.Decorate(domainEventHandler, idempotentHandler);
        }
    }
}
