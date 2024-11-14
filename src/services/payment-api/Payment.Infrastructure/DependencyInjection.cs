using BubbleTeaShop.Contracts;
using Hangfire;
using Hangfire.MemoryStorage;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Payment.Application.Abstractions.Data;
using Payment.Application.Abstractions.EventBus;
using Payment.Application.Abstractions.Payments;
using Payment.Domain.Invoices;
using Payment.Domain.Payments;
using Payment.Infrastructure.Database;
using Payment.Infrastructure.Database.Constants;
using Payment.Infrastructure.Database.Repositories;
using Payment.Infrastructure.Inbox;
using Payment.Infrastructure.Outbox;
using Payment.Infrastructure.Payments;
using ServiceDefaults.Messaging;

namespace Payment.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDomainEventHandlers();
        services.AddIntegrationEventHandlers();
        services.AddMessageQueue(configuration);
        services.AddBackgroundJobs(configuration);
        
        services.TryAddScoped<IPaymentRepository, PaymentRepository>();
        services.TryAddScoped<IInvoiceRepository, InvoiceRepository>();
        services.TryAddScoped<IPaymentService, PaymentService>();
        
        return services;
    }
    
    private static void AddDomainEventHandlers(this IServiceCollection services)
    {
        Type[] domainEventHandlers = Application.AssemblyReference.Assembly
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
        Type[] integrationEventHandlers = Presentation.AssemblyReference.Assembly
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
        
        builder.AddNpgsqlDbContext<PaymentDbContext>(
            "payment-db",
            _ => {},
            optionsBuilder =>
            {
                optionsBuilder.UseSnakeCaseNamingConvention();
        
                optionsBuilder.UseNpgsql(contextOptionsBuilder =>
                    contextOptionsBuilder.MigrationsHistoryTable(
                        HistoryRepository.DefaultTableName,
                        Schemas.Payment));

                // TODO: find a better way to add interceptors
                optionsBuilder.AddInterceptors(
                    builder.Services
                        .BuildServiceProvider()
                        .GetRequiredService<InsertOutboxMessagesInterceptor>());
            });
        
        builder.EnrichNpgsqlDbContext<PaymentDbContext>();
        
        builder.AddNpgsqlDataSource("payment-db");
        
        builder.Services.TryAddScoped<IDbConnectionFactory, DbConnectionFactory>();
        
        builder.Services.TryAddScoped<IUnitOfWork>(sp => sp.GetRequiredService<PaymentDbContext>());
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
            
            configurator.AddConsumer<IntegrationEventConsumer<PaymentProcessedEvent>>()
                .Endpoint(e => e.InstanceId = instanceId);
    
            configurator.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration.GetConnectionString("queue")!);
                cfg.ConfigureEndpoints(context);
            });
        });
        
        services.TryAddScoped<IEventBus, EventBus.EventBus>();
    }
}
