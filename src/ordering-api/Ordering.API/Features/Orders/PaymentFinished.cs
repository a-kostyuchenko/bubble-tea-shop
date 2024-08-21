using BubbleTeaShop.Contracts;
using MediatR;
using ServiceDefaults.Domain;
using ServiceDefaults.Exceptions;
using ServiceDefaults.Messaging;

namespace Ordering.API.Features.Orders;

internal sealed class PaymentFinishedIntegrationEventHandler(ISender sender) 
    : IntegrationEventHandler<PaymentFinishedEvent>
{
    public override async Task Handle(
        PaymentFinishedEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        var command = new PayOrder.Command(integrationEvent.OrderId);

        Result result = await sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            throw new BubbleTeaShopException(nameof(PayOrder.Command), result.Error);
        }
    }
}
