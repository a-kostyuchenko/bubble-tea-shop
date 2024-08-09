using Cart.API.Entities.Carts;
using Cart.API.Infrastructure.Database;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceDefaults.Domain;
using ServiceDefaults.Endpoints;
using ServiceDefaults.Messaging;

namespace Cart.API.Features.Carts;

public static class AddCartItem
{
    public sealed record Command(
        Guid CartId,
        Guid ProductId,
        int Quantity,
        string ProductName,
        decimal Price,
        string Currency,
        string Size,
        string SugarLevel,
        string IceLevel,
        string Temperature) : ICommand;
    
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.CartId).NotEmpty();
            RuleFor(c => c.ProductId).NotEmpty();
            RuleFor(c => c.Quantity).GreaterThan(0);
            RuleFor(c => c.ProductName).NotEmpty().MaximumLength(300);
            RuleFor(c => c.Price).GreaterThan(0);
            RuleFor(c => c.Currency).NotEmpty().MaximumLength(3);
            RuleFor(c => c.Size).NotEmpty().MaximumLength(50);
            RuleFor(c => c.SugarLevel).NotEmpty().MaximumLength(50);
            RuleFor(c => c.IceLevel).NotEmpty().MaximumLength(50);
            RuleFor(c => c.Temperature).NotEmpty().MaximumLength(50);
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

            Result<Money> moneyResult = Money.Create(request.Price, Currency.FromCode(request.Currency));
            Result<Quantity> quantityResult = Quantity.Create(request.Quantity);
            var sizeResult = Result.Success(Size.FromName(request.Size));
            var sugarLevelResult = Result.Success(SugarLevel.FromName(request.SugarLevel));
            var iceLevelResult = Result.Success(IceLevel.FromName(request.IceLevel));
            var temperatureResult = Result.Success(Temperature.FromName(request.Temperature));

            var inspection = Result.Inspect(
                moneyResult,
                quantityResult,
                sizeResult,
                sugarLevelResult,
                iceLevelResult,
                temperatureResult);

            if (inspection.IsFailure)
            {
                return Result.Failure(inspection.Error);
            }

            Result<CartItem> cartItemResult = CartItem.Create(
                request.ProductId,
                request.ProductName,
                moneyResult.Value,
                quantityResult.Value,
                sizeResult.Value,
                sugarLevelResult.Value,
                iceLevelResult.Value,
                temperatureResult.Value);

            if (cartItemResult.IsFailure)
            {
                return Result.Failure(cartItemResult.Error);
            }
                
            cart.AddItem(cartItemResult.Value);

            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("carts/{cartId:guid}/items", Handler)
                .WithTags(nameof(ShoppingCart))
                .WithName(nameof(AddCartItem));
        }

        private static async Task<IResult> Handler(ISender sender, Guid cartId, Request request)
        {
            var command = new Command(
                cartId,
                request.ProductId,
                request.Quantity,
                request.ProductName,
                request.Price,
                request.Currency,
                request.Size,
                request.SugarLevel,
                request.IceLevel,
                request.Temperature);
            
            Result result = await sender.Send(command);

            return result.Match(Results.Created, ApiResults.Problem);
        }
        
        private sealed record Request(
            Guid ProductId,
            int Quantity,
            string ProductName,
            decimal Price,
            string Currency,
            string Size,
            string SugarLevel,
            string IceLevel,
            string Temperature);
    }
}
