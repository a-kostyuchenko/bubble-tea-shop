using BubbleTea.Common.Domain;

namespace BubbleTea.Services.Orders.API.Entities.Orders.Events;

public sealed class OrderCancelledDomainEvent(Guid orderId) : DomainEvent
{
    public Guid OrderId { get; init; } = orderId;
}
