using BubbleTea.Common.Application.Exceptions;
using BubbleTea.Common.Application.Messaging;
using BubbleTea.Common.Domain;
using BubbleTea.Common.Presentation.Endpoints;
using BubbleTea.Contracts;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using BubbleTea.Services.Cart.API.Entities.Carts;
using BubbleTea.Services.Cart.API.Entities.Carts.Events;
using BubbleTea.Services.Cart.API.Infrastructure.Database;
using BubbleTea.Services.Cart.API.Infrastructure.EventBus;

namespace BubbleTea.Services.Cart.API.Features.Carts;

public static class CheckOutCart
{
    public sealed record Command(
        Guid CartId,
        string CardNumber,
        int ExpiryMonth,
        int ExpiryYear,
        string CVV,
        string CardHolderName) : ICommand;
    
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.CartId).NotEmpty();
            RuleFor(c => c.CardNumber).CreditCard();
            RuleFor(c => c.ExpiryMonth).InclusiveBetween(1, 12);
            RuleFor(c => c.ExpiryYear).InclusiveBetween(DateTime.UtcNow.Year, DateTime.UtcNow.Year + 10);
            RuleFor(c => c.CVV).NotEmpty().Length(PaymentInfo.DefaultCvvLength);
            RuleFor(c => c.CardHolderName).NotEmpty().MaximumLength(300);
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
            
            Result<PaymentInfo> paymentInfoResult = PaymentInfo.Create(
                request.CardNumber,
                request.ExpiryMonth,
                request.ExpiryYear,
                request.CVV,
                request.CardHolderName);

            if (paymentInfoResult.IsFailure)
            {
                return paymentInfoResult;
            }

            Result result = cart.CheckOut(paymentInfoResult.Value);

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

        private static async Task<IResult> Handler(ISender sender, Guid cartId, Request request)
        {
            var command = new Command(
                cartId,
                request.CardNumber,
                request.ExpiryMonth,
                request.ExpiryYear,
                request.CVV,
                request.CardHolderName);
            
            Result result = await sender.Send(command);

            return result.Match(Results.NoContent, ApiResults.Problem);
        }
        
        private sealed record Request(
            string CardNumber,
            int ExpiryMonth,
            int ExpiryYear,
            string CVV,
            string CardHolderName);
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
                domainEvent.CardNumber,
                domainEvent.ExpiryMonth,
                domainEvent.ExpiryYear,
                domainEvent.CVV,
                domainEvent.CardHolderName,
                cart.Items.Aggregate(Money.Zero(), (money, item) => money + item.TotalPrice).Amount,
                cart.Items.First().Price.Currency.Code,
                cart.Items.Select(i => new CartItemModel(
                    i.ProductId,
                    i.ProductName,
                    i.Quantity.Value,
                    i.Price.Amount,
                    i.TotalPrice.Amount,
                    i.Price.Currency.Code,
                    i.Parameters.Select(p => new ParameterModel(
                        p.Name,
                        new OptionModel(
                            p.SelectedOption.Name,
                            p.SelectedOption.ExtraPrice.Amount,
                            p.SelectedOption.ExtraPrice.Currency.Code
                        )
                    )).ToList()
                )).ToList()), cancellationToken);
        }
    }
}
