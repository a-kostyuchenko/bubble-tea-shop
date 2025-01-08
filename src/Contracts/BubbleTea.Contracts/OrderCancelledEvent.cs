using BubbleTea.ServiceDefaults.Messaging;

namespace BubbleTea.Contracts;

public sealed class OrderCancelledEvent(
    Guid id,
    DateTime occurredOnUtc,
    Guid orderId) : IntegrationEvent(id, occurredOnUtc)
{
    public Guid OrderId { get; init; } = orderId;
}