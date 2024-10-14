using Cart.API.Entities.Carts;
using Cart.API.Infrastructure.Database;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceDefaults.Domain;
using ServiceDefaults.Endpoints;
using ServiceDefaults.Messaging;

namespace Cart.API.Features.Carts;

public static class CancelCart
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

            Result result = cart.Cancel();

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
            app.MapDelete("carts/{cartId:guid}/cancel", Handler)
                .WithTags(nameof(ShoppingCart))
                .WithName(nameof(CancelCart));
        }

        private static async Task<IResult> Handler(ISender sender, Guid cartId)
        {
            var command = new Command(cartId);
            
            Result result = await sender.Send(command);

            return result.Match(Results.NoContent, ApiResults.Problem);
        }
    }
}
