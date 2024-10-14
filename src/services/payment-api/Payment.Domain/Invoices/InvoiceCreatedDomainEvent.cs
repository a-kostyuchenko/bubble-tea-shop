using ServiceDefaults.Domain;

namespace Payment.Domain.Invoices;

public sealed class InvoiceCreatedDomainEvent(
    Guid invoiceId,
    Guid orderId) : DomainEvent
{
    public Guid InvoiceId { get; init; } = invoiceId;
    public Guid OrderId { get; init; } = orderId;
}
