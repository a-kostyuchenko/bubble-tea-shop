using BubbleTea.ServiceDefaults.Domain;

namespace BubbleTea.Services.Orders.API.Entities.Orders.Events;

public sealed class OrderCreatedDomainEvent(Guid orderId) : DomainEvent
{
    public Guid OrderId { get; init; } = orderId;
}
