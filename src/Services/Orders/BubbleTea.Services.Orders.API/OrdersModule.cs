using Asp.Versioning;
using BubbleTea.Common.Application.EventBus;
using BubbleTea.Common.Application.Messaging;
using BubbleTea.Common.Infrastructure.Inbox;
using BubbleTea.Common.Infrastructure.Outbox;
using BubbleTea.Common.Presentation.Endpoints;
using BubbleTea.Contracts;
using MassTransit;
using Microsoft.Extensions.DependencyInjection.Extensions;
using BubbleTea.Services.Orders.API.Features.Orders.CancelOrderSaga;
using BubbleTea.Services.Orders.API.Infrastructure.Inbox;
using BubbleTea.Services.Orders.API.Infrastructure.Outbox;

namespace BubbleTea.Services.Orders.API;

internal static class OrdersModule
{
    public static IServiceCollection AddOrdersModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDomainEventHandlers();
            
        services.AddIntegrationEventHandlers();
        
        services.AddEndpoints(AssemblyReference.Assembly);
        
        services.AddInfrastructure(configuration);
        
        services.AddApiVersioning();

        services.AddDocumentation();

        return services;
    }

    private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OutboxOptions>(configuration.GetSection(OutboxOptions.ConfigurationSection));
        services.Configure<InboxOptions>(configuration.GetSection(InboxOptions.ConfigurationSection));
        
        services.TryAddScoped<IOutboxProcessor, OutboxProcessor>();
        services.TryAddScoped<IInboxProcessor, InboxProcessor>();
    }

    public static Action<IRegistrationConfigurator, string> ConfigureConsumers(string redisConnection)
    {
        return (registration, instanceId) =>
        {
            registration.AddSagaStateMachine<CancelOrderSaga, CancelOrderState>()
                .Endpoint(e => e.InstanceId = instanceId)
                .RedisRepository(redisConnection);
            
            registration.AddConsumer<IntegrationEventConsumer<CheckOutCartStartedEvent>>()
                .Endpoint(e => e.InstanceId = instanceId);
            
            registration.AddConsumer<IntegrationEventConsumer<PaymentFinishedEvent>>()
                .Endpoint(e => e.InstanceId = instanceId);
            
            registration.AddConsumer<IntegrationEventConsumer<PaymentFailedEvent>>()
                .Endpoint(e => e.InstanceId = instanceId);
        };
    }
    
    private static void AddApiVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1);
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'V";
            options.SubstituteApiVersionInUrl = true;
        });
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
    
    private static void AddIntegrationEventHandlers(this IServiceCollection services)
    {
        Type[] integrationEventHandlers = AssemblyReference.Assembly
            .GetTypes()
            .Where(type => type.IsAssignableTo(typeof(IIntegrationEventHandler)) &&
                           !type.IsAssignableTo(typeof(IdempotentIntegrationEventHandler<>)))
            .ToArray();

        foreach (Type integrationEventHandler in integrationEventHandlers)
        {
            services.TryAddScoped(integrationEventHandler);

            Type integrationEvent = integrationEventHandler
                .GetInterfaces()
                .Single(i => i.IsGenericType)
                .GetGenericArguments()
                .Single();

            Type idempotentHandler =
                typeof(IdempotentIntegrationEventHandler<>).MakeGenericType(integrationEvent);

            services.Decorate(integrationEventHandler, idempotentHandler);
        }
    }
}
