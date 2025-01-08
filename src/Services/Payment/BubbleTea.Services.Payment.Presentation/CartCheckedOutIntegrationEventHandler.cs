using BubbleTea.Contracts;
using BubbleTea.ServiceDefaults.Domain;
using BubbleTea.ServiceDefaults.Exceptions;
using BubbleTea.ServiceDefaults.Messaging;
using MediatR;
using BubbleTea.Services.Payment.Application.Abstractions.EventBus;
using BubbleTea.Services.Payment.Application.Payments.Commands.Process;

namespace BubbleTea.Services.Payment.Presentation;

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
            integrationEvent.CardHolderName,
            integrationEvent.Items);

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
