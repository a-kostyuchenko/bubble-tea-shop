using BubbleTea.ServiceDefaults.Messaging;

namespace BubbleTea.Services.Payment.Application.Abstractions.EventBus;

public interface IEventBus
{
    Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default)
        where T : IIntegrationEvent;
}
