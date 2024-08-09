using BubbleTeaShop.Contracts;
using Cart.API.Entities.Carts;
using Cart.API.Entities.Carts.Events;
using Cart.API.Infrastructure.Database;
using Cart.API.Infrastructure.EventBus;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceDefaults.Domain;
using ServiceDefaults.Endpoints;
using ServiceDefaults.Exceptions;
using ServiceDefaults.Messaging;

namespace Cart.API.Features.Carts;

public static class CheckOutCart
{
    public sealed record Command(Guid CartId) : ICommand;
    
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.CartId).NotEmpty();
        }
    }

    internal sealed class CommandHandler(CartDbContext dbContext) : ICommandHandler<Command>
    {
        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            ShoppingCart? cart = await dbContext.ShoppingCarts
                .FirstOrDefaultAsync(c => c.Id == request.CartId, cancellationToken);

            if (cart is null)
            {
                return Result.Failure(CartErrors.NotFound(request.CartId));
            }

            Result result = cart.CheckOut();

            if (result.IsFailure)
            {
                return result;
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPut("carts/{cartId:guid}/check-out", Handler)
                .WithTags(nameof(ShoppingCart))
                .WithName(nameof(CheckOutCart));
        }

        private static async Task<IResult> Handler(ISender sender, Guid cartId)
        {
            var command = new Command(cartId);
            
            Result result = await sender.Send(command);

            return result.Match(Results.NoContent, ApiResults.Problem);
        }
    }
    
    internal sealed class CartCheckedOutDomainEventHandler(CartDbContext dbContext, IEventBus eventBus) 
        : DomainEventHandler<CartCheckedOutDomainEvent>
    {
        public override async Task Handle(
            CartCheckedOutDomainEvent domainEvent,
            CancellationToken cancellationToken = default)
        {
            ShoppingCart? cart = await dbContext.ShoppingCarts
                .FirstOrDefaultAsync(c => c.Id == domainEvent.CartId, cancellationToken);

            if (cart is null)
            {
                throw new BubbleTeaShopException(
                    nameof(CartCheckedOutDomainEventHandler),
                    CartErrors.NotFound(domainEvent.CartId));
            }

            await eventBus.PublishAsync(new CartCheckedOutEvent(
                domainEvent.Id,
                domainEvent.OccurredOnUtc,
                cart.Id,
                cart.Customer,
                cart.Note,
                cart.Items.Select(item => new CartItemModel(
                    item.ProductId,
                    item.ProductName,
                    item.Quantity.Value,
                    item.Price.Amount,
                    item.Price.Currency.Code,
                    item.Size.Name,
                    item.SugarLevel.Name,
                    item.IceLevel.Name,
                    item.Temperature.Name))
                    .ToList()), cancellationToken);
        }
    }
}
