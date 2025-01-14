using BubbleTea.Common.Application.EventBus;
using BubbleTea.Common.Application.Messaging;
using BubbleTea.Common.Presentation.Endpoints;
using BubbleTea.Contracts;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using BubbleTea.Services.Payment.Application.Abstractions.Data;
using BubbleTea.Services.Payment.Application.Abstractions.Payments;
using BubbleTea.Services.Payment.Domain.Invoices;
using BubbleTea.Services.Payment.Domain.Payments;
using BubbleTea.Services.Payment.Infrastructure.Database;
using BubbleTea.Services.Payment.Infrastructure.Database.Repositories;
using BubbleTea.Services.Payment.Infrastructure.Inbox;
using BubbleTea.Services.Payment.Infrastructure.Outbox;
using BubbleTea.Services.Payment.Infrastructure.Payments;

namespace BubbleTea.Services.Payment.Infrastructure;

public static class PaymentModule
{
    public static IServiceCollection AddPaymentModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDomainEventHandlers();
        
        services.AddIntegrationEventHandlers();

        services.AddEndpoints(Presentation.AssemblyReference.Assembly);
        
        services.AddInfrastructure(configuration);
        
        return services;
    }

    private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddScoped<IPaymentRepository, PaymentRepository>();
        services.TryAddScoped<IInvoiceRepository, InvoiceRepository>();
        services.TryAddScoped<IPaymentService, PaymentService>();
        
        services.TryAddScoped<IUnitOfWork>(sp => sp.GetRequiredService<PaymentDbContext>());

        services.Configure<OutboxOptions>(configuration.GetSection(OutboxOptions.ConfigurationSection));
        services.Configure<InboxOptions>(configuration.GetSection(InboxOptions.ConfigurationSection));
        
        services.TryAddScoped<IOutboxProcessor, OutboxProcessor>();
        services.TryAddScoped<IInboxProcessor, InboxProcessor>();
    }

    public static void ConfigureConsumers(IRegistrationConfigurator registrationConfigurator, string instanceId)
    {
        registrationConfigurator.AddConsumer<IntegrationEventConsumer<CheckOutCartStartedEvent>>()
            .Endpoint(e => e.InstanceId = instanceId);
            
        registrationConfigurator.AddConsumer<IntegrationEventConsumer<PaymentProcessedEvent>>()
            .Endpoint(e => e.InstanceId = instanceId);
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
}
