using BubbleTea.Common.Application.EventBus;

namespace BubbleTea.Contracts;

public sealed class InvoiceFormedEvent(
    Guid id,
    DateTime occurredOnUtc,
    Guid orderId,
    Guid invoiceId) : IntegrationEvent(id, occurredOnUtc)
{
    public Guid OrderId { get; init; } = orderId;
    public Guid InvoiceId { get; init; } = invoiceId;
}
