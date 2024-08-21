using ServiceDefaults.Messaging;

namespace BubbleTeaShop.Contracts;

public sealed class PaymentProcessedEvent(
    Guid id,
    DateTime occurredOnUtc,
    Guid orderId,
    Guid paymentId) : IntegrationEvent(id, occurredOnUtc)
{
    public Guid OrderId { get; init; } = orderId;
    public Guid PaymentId { get; init; } = paymentId;
}