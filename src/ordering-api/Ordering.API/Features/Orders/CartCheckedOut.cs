using BubbleTeaShop.Contracts;
using MediatR;
using ServiceDefaults.Domain;
using ServiceDefaults.Exceptions;
using ServiceDefaults.Messaging;

namespace Ordering.API.Features.Orders;

public static class CartCheckedOut
{
    public sealed class Consumer(ISender sender) : IntegrationEventHandler<CartCheckedOutEvent>
    {
        public override async Task Handle(
            CartCheckedOutEvent integrationEvent,
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
                    item.Currency,
                    item.Size,
                    item.SugarLevel,
                    item.IceLevel,
                    item.Temperature)).ToList());
            
            Result<Guid> result = await sender.Send(command, cancellationToken);

            if (result.IsFailure)
            {
                throw new BubbleTeaShopException(nameof(CreateOrder.Command), result.Error);
            }
        }
    }
}
