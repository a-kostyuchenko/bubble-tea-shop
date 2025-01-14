using System.Globalization;
using Asp.Versioning;
using BubbleTea.Common.Application.EventBus;
using BubbleTea.Common.Application.Messaging;
using BubbleTea.Common.Application.Slugs;
using BubbleTea.Common.Infrastructure.Inbox;
using BubbleTea.Common.Infrastructure.Outbox;
using BubbleTea.Common.Presentation.Endpoints;
using Microsoft.Extensions.DependencyInjection.Extensions;
using BubbleTea.Services.Catalog.API.Entities.Products;
using BubbleTea.Services.Catalog.API.Infrastructure.Inbox;
using BubbleTea.Services.Catalog.API.Infrastructure.Outbox;
using BubbleTea.Services.Catalog.API.Infrastructure.Storage;
using static BubbleTea.Common.Application.Slugs.HandleTransforms;
using static BubbleTea.Common.Application.Slugs.HandleToSlugConversions;

namespace BubbleTea.Services.Catalog.API;

public static class CatalogModule
{
    public static IServiceCollection AddCatalogModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDomainEventHandlers();
        
        services.AddIntegrationEventHandlers();
        
        services.AddEndpoints(AssemblyReference.Assembly);
        
        services.AddInfrastructure(configuration);

        services.AddDocumentation();
        
        services.AddApiVersioning();
        
        services.AddSlugs();
        
        return services;
    }

    private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddSingleton<IBlobService, BlobService>();
        
        services.Configure<OutboxOptions>(configuration.GetSection(OutboxOptions.ConfigurationSection));
        services.Configure<InboxOptions>(configuration.GetSection(InboxOptions.ConfigurationSection));
        
        services.TryAddScoped<IOutboxProcessor, OutboxProcessor>();
        services.TryAddScoped<IInboxProcessor, InboxProcessor>();
    }
    
    private static void AddSlugs(this IServiceCollection services)
    {
        services.AddSingleton<ProductNameToSlug>(_ => name =>
            new Handle(name)
                .Transform(ToLowercase(CultureInfo.InvariantCulture), IntoLetterAndDigitRuns)
                .ToSlug(Hyphenate));
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
