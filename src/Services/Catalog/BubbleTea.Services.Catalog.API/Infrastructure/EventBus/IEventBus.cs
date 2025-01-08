using BubbleTea.ServiceDefaults.Messaging;

namespace BubbleTea.Services.Catalog.API.Infrastructure.EventBus;

public interface IEventBus
{
    Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default)
        where T : IIntegrationEvent;
}