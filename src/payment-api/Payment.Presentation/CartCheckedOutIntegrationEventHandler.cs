using BubbleTeaShop.Contracts;
using MediatR;
using Payment.Application.Abstractions.EventBus;
using Payment.Application.Payments.Commands.Process;
using ServiceDefaults.Domain;
using ServiceDefaults.Exceptions;
using ServiceDefaults.Messaging;

namespace Payment.Presentation;

internal sealed class CartCheckedOutIntegrationEventHandler(ISender sender, IEventBus eventBus) 
    : IntegrationEventHandler<CheckOutCartStartedEvent>
{
    public override async Task Handle(
        CheckOutCartStartedEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        var command = new ProcessPaymentCommand(
            integrationEvent.CartId,
            integrationEvent.TotalAmount,
            integrationEvent.Currency,
            integrationEvent.CardNumber,
            integrationEvent.ExpiryMonth,
            integrationEvent.ExpiryYear,
            integrationEvent.CVV,
            integrationEvent.CardHolderName);

        Result result = await sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            await eventBus.PublishAsync(new PaymentFailedEvent(
                Guid.NewGuid(), 
                integrationEvent.OccurredOnUtc,
                integrationEvent.CartId),
                cancellationToken);
            
            throw new BubbleTeaShopException(nameof(ProcessPaymentCommand), result.Error);
        }
    }
}
