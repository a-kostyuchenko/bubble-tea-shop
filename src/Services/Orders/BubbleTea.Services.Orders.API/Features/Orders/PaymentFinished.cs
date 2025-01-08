using BubbleTea.Contracts;
using MediatR;
using BubbleTea.ServiceDefaults.Domain;
using BubbleTea.ServiceDefaults.Exceptions;
using BubbleTea.ServiceDefaults.Messaging;

namespace BubbleTea.Services.Orders.API.Features.Orders;

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
