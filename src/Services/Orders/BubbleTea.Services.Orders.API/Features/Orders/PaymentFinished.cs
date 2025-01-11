using BubbleTea.Common.Application.EventBus;
using BubbleTea.Common.Application.Exceptions;
using BubbleTea.Common.Domain;
using BubbleTea.Contracts;
using MediatR;

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
