using BubbleTea.Common.Application.EventBus;

namespace BubbleTea.Services.Orders.API.Infrastructure.EventBus;

public interface IEventBus
{
    Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default)
        where T : IIntegrationEvent;
}
