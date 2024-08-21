using MassTransit;
using Payment.Application.Abstractions.EventBus;
using ServiceDefaults.Messaging;

namespace Payment.Infrastructure.EventBus;

internal sealed class EventBus(IBus bus) : IEventBus
{
    public async Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default) 
        where T : IIntegrationEvent => 
        await bus.Publish(integrationEvent, cancellationToken);
}
