using ServiceDefaults.Messaging;

namespace BubbleTeaShop.Contracts;

public sealed class InvoiceFormedEvent(
    Guid id,
    DateTime occurredOnUtc,
    Guid orderId,
    Guid invoiceId) : IntegrationEvent(id, occurredOnUtc)
{
    public Guid OrderId { get; init; } = orderId;
    public Guid InvoiceId { get; init; } = invoiceId;
}
