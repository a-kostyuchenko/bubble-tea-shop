using BubbleTea.Common.Application.EventBus;
using BubbleTea.Services.Payment.Application.Abstractions.EventBus;
using MassTransit;

namespace BubbleTea.Common.Infrastructure.EventBus;

internal sealed class EventBus(IBus bus) : IEventBus
{
    public async Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default)
        where T : IIntegrationEvent
    {
        await bus.Publish(integrationEvent, cancellationToken);
    }
}
