using ServiceDefaults.Domain;

namespace Payment.Domain.Payments;

public sealed class PaymentProcessedDomainEvent(Guid paymentId, Guid orderId) : DomainEvent
{
    public Guid PaymentId { get; init; } = paymentId;
    public Guid OrderId { get; init; } = orderId;
}
