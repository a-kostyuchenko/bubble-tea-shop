using BubbleTeaShop.Contracts;
using Payment.Application.Abstractions.EventBus;
using Payment.Domain.Payments;
using ServiceDefaults.Messaging;

namespace Payment.Application.Payments.Commands.Process;

internal sealed class PaymentProcessedDomainEventHandler(IEventBus eventBus) 
    : DomainEventHandler<PaymentProcessedDomainEvent>
{
    public override async Task Handle(
        PaymentProcessedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        await eventBus.PublishAsync(new PaymentProcessedEvent(
                domainEvent.Id,
                domainEvent.OccurredOnUtc,
                domainEvent.OrderId,
                domainEvent.PaymentId),
            cancellationToken);
    }
}
