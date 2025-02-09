using BubbleTea.Common.Application.Messaging;
using BubbleTea.Common.Domain;
using BubbleTea.Common.Presentation.Endpoints;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using BubbleTea.Services.Cart.API.Entities.Carts;
using BubbleTea.Services.Cart.API.Infrastructure.Database;

namespace BubbleTea.Services.Cart.API.Features.Carts;

public static class RemoveCartItem
{
    public sealed record Command(Guid CartId, Guid CartItemId) : ICommand;
    
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.CartId).NotEmpty();
            RuleFor(c => c.CartItemId).NotEmpty();
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

            Result result = cart.RemoveItem(request.CartItemId);

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
            app.MapDelete("carts/{cartId:guid}/items/{cartItemId:guid}", Handler)
                .WithTags(nameof(ShoppingCart))
                .WithName(nameof(RemoveCartItem));
        }

        private static async Task<IResult> Handler(ISender sender, Guid cartId, Guid cartItemId)
        {
            var command = new Command(cartId, cartItemId);
            
            Result result = await sender.Send(command);

            return result.Match(Results.Created, ApiResults.Problem);
        }
    }
}
