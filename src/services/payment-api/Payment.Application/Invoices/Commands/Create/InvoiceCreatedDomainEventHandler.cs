using BubbleTeaShop.Contracts;
using Payment.Application.Abstractions.EventBus;
using Payment.Domain.Invoices;
using ServiceDefaults.Messaging;

namespace Payment.Application.Invoices.Commands.Create;

internal sealed class InvoiceCreatedDomainEventHandler(IEventBus eventBus) 
    : DomainEventHandler<InvoiceCreatedDomainEvent>
{
    public override async Task Handle(
        InvoiceCreatedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        await eventBus.PublishAsync(new InvoiceFormedEvent(
            domainEvent.Id,
            domainEvent.OccurredOnUtc,
            domainEvent.OrderId,
            domainEvent.InvoiceId), cancellationToken);
    }
}
