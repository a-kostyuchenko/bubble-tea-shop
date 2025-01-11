using BubbleTea.Common.Application.Messaging;
using BubbleTea.Common.Domain;
using BubbleTea.Common.Presentation.Endpoints;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using BubbleTea.Services.Cart.API.Entities.Carts;
using BubbleTea.Services.Cart.API.Infrastructure.Database;

namespace BubbleTea.Services.Cart.API.Features.Carts;

public static class AddCartItem
{
    public sealed record Command(
        Guid CartId,
        Guid ProductId,
        int Quantity,
        string ProductName,
        decimal Price,
        string Currency,
        HashSet<ParameterRequest> Parameters) : ICommand;

    public sealed record ParameterRequest(string Name, OptionRequest SelectedOption);
    public sealed record OptionRequest(string Name, double Value, decimal ExtraPrice, string Currency);
    
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
            RuleFor(c => c.Parameters).NotNull();

            RuleForEach(c => c.Parameters).ChildRules(item =>
            {
                item.RuleFor(i => i.Name).NotEmpty();
                item.RuleFor(i => i.SelectedOption).NotNull();
                item.RuleFor(i => i.SelectedOption.Name).NotEmpty();
                item.RuleFor(i => i.SelectedOption.Value).GreaterThan(0);
                item.RuleFor(i => i.SelectedOption.ExtraPrice).GreaterThan(0);
                item.RuleFor(i => i.SelectedOption.Currency).NotEmpty().MaximumLength(3);
            });
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

            var inspection = Result.Inspect(
                moneyResult,
                quantityResult);

            if (inspection.IsFailure)
            {
                return Result.Failure(inspection.Error);
            }
            
            HashSet<Parameter> parameters = [];
            
            foreach ((string name, OptionRequest selectedOption) in request.Parameters)
            {
                Result<Money> parameterMoneyResult = Money.Create(
                    selectedOption.ExtraPrice,
                    Currency.FromCode(selectedOption.Currency));
                
                if (parameterMoneyResult.IsFailure)
                {
                    return Result.Failure(parameterMoneyResult.Error);
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
                request.ProductId,
                request.ProductName,
                moneyResult.Value,
                quantityResult.Value,
                parameters);

            if (cartItemResult.IsFailure)
            {
                return Result.Failure(cartItemResult.Error);
            }

            CartItem cartItem = cartItemResult.Value;
            
            cart.AddItem(cartItem);

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
                request.Parameters);
            
            Result result = await sender.Send(command);

            return result.Match(Results.Created, ApiResults.Problem);
        }

        private sealed record Request(
            Guid ProductId,
            int Quantity,
            string ProductName,
            decimal Price,
            string Currency)
        {
            public HashSet<ParameterRequest> Parameters { get; init; } = [];
        }
    }
}
