using BubbleTea.Common.Application.EventBus;

namespace BubbleTea.Contracts;

public sealed class OrderPaymentRefundedEvent(
    Guid id,
    DateTime occurredOnUtc,
    Guid orderId) : IntegrationEvent(id, occurredOnUtc)
{
    public Guid OrderId { get; init; } = orderId;
}
