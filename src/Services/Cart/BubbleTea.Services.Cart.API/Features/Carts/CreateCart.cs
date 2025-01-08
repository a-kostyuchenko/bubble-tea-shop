using FluentValidation;
using MediatR;
using BubbleTea.ServiceDefaults.Domain;
using BubbleTea.ServiceDefaults.Endpoints;
using BubbleTea.ServiceDefaults.Messaging;
using BubbleTea.Services.Cart.API.Entities.Carts;
using BubbleTea.Services.Cart.API.Infrastructure.Database;

namespace BubbleTea.Services.Cart.API.Features.Carts;

public static class CreateCart
{
    public sealed record ItemRequest(
        Guid ProductId,
        int Quantity,
        string ProductName,
        decimal Price,
        string Currency,
        HashSet<ParameterRequest> Parameters);
    public sealed record ParameterRequest(string Name, OptionRequest SelectedOption);
    public sealed record OptionRequest(string Name, double Value, decimal ExtraPrice, string Currency);
    public sealed record Command(string Customer,  List<ItemRequest> Items) : ICommand<Guid>;
    
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Customer).NotEmpty().MaximumLength(300);
            
            RuleForEach(c => c.Items)
                .ChildRules(item =>
                {
                    item.RuleFor(i => i.ProductId).NotEmpty();
                    item.RuleFor(i => i.Quantity).GreaterThan(0);
                    item.RuleFor(i => i.ProductName).NotEmpty().MaximumLength(300);
                    item.RuleFor(i => i.Price).GreaterThan(0);
                    item.RuleFor(i => i.Currency).NotEmpty().MaximumLength(3);
                });
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

                var inspection = Result.Inspect(
                    moneyResult,
                    quantityResult);

                if (inspection.IsFailure)
                {
                    return Result.Failure<Guid>(inspection.Error);
                }

                HashSet<Parameter> parameters = [];
            
                foreach ((string name, OptionRequest selectedOption) in item.Parameters)
                {
                    Result<Money> parameterMoneyResult = Money.Create(
                        selectedOption.ExtraPrice,
                        Currency.FromCode(selectedOption.Currency));
                
                    if (parameterMoneyResult.IsFailure)
                    {
                        return Result.Failure<Guid>(parameterMoneyResult.Error);
                    }

                    var parameter = Parameter.Create(
                        name,
                        selectedOption.Name,
                        selectedOption.Value,
                        parameterMoneyResult.Value
                    );

                    parameters.Add(parameter);
                }

                Result<CartItem> cartItemResult = CartItem.Create(
                    item.ProductId,
                    item.ProductName,
                    moneyResult.Value,
                    quantityResult.Value,
                    parameters);

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
            var command = new Command(request.Customer, request.Items);
            
            Result<Guid> result = await sender.Send(command);

            return result.Match(
                cartId => Results.CreatedAtRoute(nameof(GetCart), new { cartId }, cartId),
                ApiResults.Problem);
        }

        private sealed record Request(string Customer)
        {
            public List<ItemRequest> Items { get; init; } = [];
        }
    }
}
