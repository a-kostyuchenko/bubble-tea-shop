using ServiceDefaults.Domain;

namespace Ordering.API.Entities.Orders.Events;

public sealed class OrderCancelledDomainEvent(Guid orderId) : DomainEvent
{
    public Guid OrderId { get; init; } = orderId;
}
