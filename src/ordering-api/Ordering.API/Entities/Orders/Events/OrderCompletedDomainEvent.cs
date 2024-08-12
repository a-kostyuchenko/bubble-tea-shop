using ServiceDefaults.Domain;

namespace Ordering.API.Entities.Orders.Events;

public sealed class OrderCompletedDomainEvent(Guid orderId) : DomainEvent
{
    public Guid OrderId { get; init; } = orderId;
}
