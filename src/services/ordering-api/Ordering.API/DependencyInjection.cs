using Asp.Versioning;
using BubbleTeaShop.Contracts;
using FluentValidation;
using Hangfire;
using Hangfire.MemoryStorage;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ordering.API.Features.Orders.CancelOrderSaga;
using Ordering.API.Infrastructure.Database;
using Ordering.API.Infrastructure.Database.Constants;
using Ordering.API.Infrastructure.EventBus;
using Ordering.API.Infrastructure.Inbox;
using Ordering.API.Infrastructure.Outbox;
using ServiceDefaults.Behaviors;
using ServiceDefaults.Endpoints;
using ServiceDefaults.Messaging;
using ServiceDefaults.OpenApi;

namespace Ordering.API;

internal static class DependencyInjection
{
    public static IServiceCollection AddOrderingServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDomainEventHandlers();
        services.AddIntegrationEventHandlers();
        services.AddApplication();
        services.AddEndpoints(AssemblyReference.Assembly);
        services.AddMessageQueue(configuration);
        services.AddApiVersioning();
        services.AddBackgroundJobs(configuration);
        services.AddApiDocumentation();

        return services;
    }
    
    private static void AddApiDocumentation(this IServiceCollection services)
    {
        services.AddOpenApi();
        services.ConfigureOptions<ScalarOptionsSetup>();
    }

    private static void AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(AssemblyReference.Assembly);
            
            cfg.AddOpenBehavior(typeof(RequestLoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(AssemblyReference.Assembly, includeInternalTypes: true);
    }
    
    private static void AddBackgroundJobs(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHangfire(globalConfiguration => globalConfiguration.UseMemoryStorage());

        services.AddHangfireServer(options =>
            options.SchedulePollingInterval = TimeSpan.FromSeconds(1));
        
        services.Configure<OutboxOptions>(configuration.GetSection(OutboxOptions.ConfigurationSection));
        services.Configure<InboxOptions>(configuration.GetSection(InboxOptions.ConfigurationSection));
        
        services.TryAddScoped<IOutboxProcessor, OutboxProcessor>();
        services.TryAddScoped<IInboxProcessor, InboxProcessor>();
    }
    
    private static void AddMessageQueue(this IServiceCollection services, IConfiguration configuration)
    {
        string instanceId = AssemblyReference.Assembly.GetName().Name?.ToLowerInvariant().Replace('.', '-')!;

        services.AddMassTransit(configurator =>
        {
            configurator.SetKebabCaseEndpointNameFormatter();

            configurator.AddConsumer<IntegrationEventConsumer<CheckOutCartStartedEvent>>()
                .Endpoint(e => e.InstanceId = instanceId);
            
            configurator.AddConsumer<IntegrationEventConsumer<PaymentFinishedEvent>>()
                .Endpoint(e => e.InstanceId = instanceId);
            
            configurator.AddConsumer<IntegrationEventConsumer<PaymentFailedEvent>>()
                .Endpoint(e => e.InstanceId = instanceId);

            configurator.AddSagaStateMachine<CancelOrderSaga, CancelOrderState>()
                .Endpoint(e => e.InstanceId = instanceId)
                .RedisRepository(configuration.GetConnectionString("cache"));
    
            configurator.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration.GetConnectionString("queue")!);
                cfg.ConfigureEndpoints(context);
            });
        });
        
        services.TryAddScoped<IEventBus, EventBus>();
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
    
    public static void AddDatabase(this WebApplicationBuilder builder)
    {
        builder.Services.TryAddSingleton<InsertOutboxMessagesInterceptor>();
        
        builder.AddNpgsqlDbContext<OrderingDbContext>(
            "order-db",
            _ => {},
            optionsBuilder =>
            {
                optionsBuilder.UseSnakeCaseNamingConvention();
        
                optionsBuilder.UseNpgsql(contextOptionsBuilder =>
                    contextOptionsBuilder.MigrationsHistoryTable(
                        HistoryRepository.DefaultTableName,
                        Schemas.Order));

                // TODO: find a better way to add interceptors
                optionsBuilder.AddInterceptors(
                    builder.Services
                        .BuildServiceProvider()
                        .GetRequiredService<InsertOutboxMessagesInterceptor>());
            });
        
        builder.EnrichNpgsqlDbContext<OrderingDbContext>();
        
        builder.AddNpgsqlDataSource("order-db");
        
        builder.Services.TryAddScoped<IDbConnectionFactory, DbConnectionFactory>();
    }
}
