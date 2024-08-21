using BubbleTeaShop.Contracts;
using MediatR;
using Payment.Application.Payments.Commands.Create;
using ServiceDefaults.Domain;
using ServiceDefaults.Exceptions;
using ServiceDefaults.Messaging;

namespace Payment.Presentation;

internal sealed class CartCheckedOutIntegrationEventHandler(ISender sender) 
    : IntegrationEventHandler<CartCheckedOutEvent>
{
    public override async Task Handle(
        CartCheckedOutEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        var command = new CreatePaymentCommand(
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
            throw new BubbleTeaShopException(nameof(CreatePaymentCommand), result.Error);
        }
    }
}
