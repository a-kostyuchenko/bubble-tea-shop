using BubbleTea.Contracts;
using MediatR;
using BubbleTea.ServiceDefaults.Domain;
using BubbleTea.ServiceDefaults.Exceptions;
using BubbleTea.ServiceDefaults.Messaging;

namespace BubbleTea.Services.Orders.API.Features.Orders;

internal sealed class PaymentFailedIntegrationEventHandler(ISender sender) 
    : IntegrationEventHandler<PaymentFailedEvent>
{
    public override async Task Handle(
        PaymentFailedEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        var command = new MarkOrderAsFailed.Command(integrationEvent.OrderId);

        Result result = await sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            throw new BubbleTeaShopException(nameof(MarkOrderAsFailed.Command), result.Error);
        }
    }
}
