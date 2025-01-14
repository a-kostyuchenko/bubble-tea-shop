using BubbleTea.Common.Application.EventBus;
using BubbleTea.Common.Application.Messaging;
using BubbleTea.Contracts;
using BubbleTea.Services.Payment.Domain.Invoices;

namespace BubbleTea.Services.Payment.Application.Invoices.Commands.Create;

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
