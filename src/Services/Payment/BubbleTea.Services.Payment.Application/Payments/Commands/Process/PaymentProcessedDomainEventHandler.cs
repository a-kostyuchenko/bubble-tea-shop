using BubbleTea.Common.Application.Messaging;
using BubbleTea.Contracts;
using BubbleTea.Services.Payment.Domain.Payments;
using BubbleTea.Services.Payment.Application.Abstractions.EventBus;

namespace BubbleTea.Services.Payment.Application.Payments.Commands.Process;

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
                domainEvent.PaymentId,
                domainEvent.Customer,
                domainEvent.Items),
            cancellationToken);
    }
}
