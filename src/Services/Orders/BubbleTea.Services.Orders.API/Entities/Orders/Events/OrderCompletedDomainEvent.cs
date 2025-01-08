using BubbleTea.ServiceDefaults.Domain;

namespace BubbleTea.Services.Orders.API.Entities.Orders.Events;

public sealed class OrderCompletedDomainEvent(Guid orderId) : DomainEvent
{
    public Guid OrderId { get; init; } = orderId;
}
