using BubbleTea.Common.Application.EventBus;
using MassTransit;
using BubbleTea.Services.Payment.Application.Abstractions.EventBus;

namespace BubbleTea.Services.Payment.Infrastructure.EventBus;

internal sealed class EventBus(IBus bus) : IEventBus
{
    public async Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default) 
        where T : IIntegrationEvent => 
        await bus.Publish(integrationEvent, cancellationToken);
}
