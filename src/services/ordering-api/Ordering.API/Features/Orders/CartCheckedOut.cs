using BubbleTeaShop.Contracts;
using MediatR;
using ServiceDefaults.Domain;
using ServiceDefaults.Exceptions;
using ServiceDefaults.Messaging;

namespace Ordering.API.Features.Orders;

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
            integrationEvent.Items.Select(item => new CreateOrder.ItemRequest(
                item.ProductName,
                item.Quantity,
                item.Price,
                item.Currency)).ToList());
        
        Result<Guid> result = await sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            throw new BubbleTeaShopException(nameof(CreateOrder.Command), result.Error);
        }
    }
}
