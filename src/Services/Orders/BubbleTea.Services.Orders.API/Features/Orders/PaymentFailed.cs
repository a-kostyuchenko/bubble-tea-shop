using BubbleTea.Common.Application.EventBus;
using BubbleTea.Common.Application.Exceptions;
using BubbleTea.Common.Domain;
using BubbleTea.Contracts;
using MediatR;

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
