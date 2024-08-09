using Cart.API.Entities.Carts;
using Cart.API.Infrastructure.Database;
using FluentValidation;
using MediatR;
using ServiceDefaults.Domain;
using ServiceDefaults.Endpoints;
using ServiceDefaults.Messaging;

namespace Cart.API.Features.Carts;

public static class CreateCart
{
    public sealed record ItemRequest(
        Guid ProductId,
        int Quantity,
        string ProductName,
        decimal Price,
        string Currency,
        string Size,
        string SugarLevel,
        string IceLevel,
        string Temperature);
    public sealed record Command(string Customer, List<ItemRequest> Items) : ICommand<Guid>;
    
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Customer).NotEmpty().MaximumLength(300);
        }
    }

    internal sealed class CommandHandler(CartDbContext dbContext) : ICommandHandler<Command, Guid>
    {
        public async Task<Result<Guid>> Handle(Command request, CancellationToken cancellationToken)
        {
            Result<ShoppingCart> cartResult = ShoppingCart.Create(request.Customer);

            if (cartResult.IsFailure)
            {
                return Result.Failure<Guid>(cartResult.Error);
            }
            
            ShoppingCart cart = cartResult.Value;
            
            foreach (ItemRequest item in request.Items)
            {
                Result<Money> moneyResult = Money.Create(item.Price, Currency.FromCode(item.Currency));
                Result<Quantity> quantityResult = Quantity.Create(item.Quantity);
                var sizeResult = Result.Success(Size.FromName(item.Size));
                var sugarLevelResult = Result.Success(SugarLevel.FromName(item.SugarLevel));
                var iceLevelResult = Result.Success(IceLevel.FromName(item.IceLevel));
                var temperatureResult = Result.Success(Temperature.FromName(item.Temperature));

                var inspection = Result.Inspect(
                    moneyResult,
                    quantityResult,
                    sizeResult,
                    sugarLevelResult,
                    iceLevelResult,
                    temperatureResult);

                if (inspection.IsFailure)
                {
                    return Result.Failure<Guid>(inspection.Error);
                }

                Result<CartItem> cartItemResult = CartItem.Create(
                    item.ProductId,
                    item.ProductName,
                    moneyResult.Value,
                    quantityResult.Value,
                    sizeResult.Value,
                    sugarLevelResult.Value,
                    iceLevelResult.Value,
                    temperatureResult.Value);

                if (cartItemResult.IsFailure)
                {
                    return Result.Failure<Guid>(cartItemResult.Error);
                }
                
                cart.AddItem(cartItemResult.Value);
            }
            
            
            dbContext.Add(cart);

            await dbContext.SaveChangesAsync(cancellationToken);

            return cart.Id;
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("carts", Handler)
                .WithTags(nameof(ShoppingCart))
                .WithName(nameof(CreateCart));
        }

        private static async Task<IResult> Handler(ISender sender, Request request)
        {
            var command = new Command(request.Name, request.Items);
            
            Result<Guid> result = await sender.Send(command);

            return result.Match(
                cartId => Results.CreatedAtRoute("", new { cartId }, cartId),
                ApiResults.Problem);
        }

        private sealed record Request(string Name)
        {
            public List<ItemRequest> Items { get; init; } = [];
        }
    }
}
