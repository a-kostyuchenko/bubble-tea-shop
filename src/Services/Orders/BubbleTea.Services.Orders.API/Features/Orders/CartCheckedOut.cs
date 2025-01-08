using BubbleTea.Contracts;
using MediatR;
using BubbleTea.ServiceDefaults.Domain;
using BubbleTea.ServiceDefaults.Exceptions;
using BubbleTea.ServiceDefaults.Messaging;

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
