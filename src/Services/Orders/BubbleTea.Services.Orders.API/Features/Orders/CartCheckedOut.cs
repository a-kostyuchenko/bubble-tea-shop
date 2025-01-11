using BubbleTea.Common.Application.EventBus;
using BubbleTea.Common.Application.Exceptions;
using BubbleTea.Common.Domain;
using BubbleTea.Contracts;
using MediatR;

namespace BubbleTea.Services.Orders.API.Features.Orders;

internal sealed class CartCheckedOutIntegrationEventHandler(ISender sender) 
    : IntegrationEventHandler<CheckOutCartStartedEvent>
{
    public override async Task Handle(
        CheckOutCartStartedEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateOrder.Command(
            integrationEvent.CartId,
            integrationEvent.Customer,
            integrationEvent.Note,
            integrationEvent.Items);
        
        Result<Guid> result = await sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            throw new BubbleTeaShopException(nameof(CreateOrder.Command), result.Error);
        }
    }
}
