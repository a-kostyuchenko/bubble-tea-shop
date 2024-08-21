using FluentValidation;
using Ordering.API.Entities.Orders;
using Ordering.API.Infrastructure.Database;
using ServiceDefaults.Domain;
using ServiceDefaults.Messaging;

namespace Ordering.API.Features.Orders;

public static class CreateOrder
{
    public sealed record ItemRequest(
        string ProductName,
        int Quantity,
        decimal Price,
        string Currency,
        string Size,
        string SugarLevel,
        string IceLevel,
        string Temperature);
    public sealed record Command(Guid Id, string Customer, string? Note, List<ItemRequest> Items) : ICommand<Guid>;
    
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Id).NotEmpty();
            
            RuleFor(c => c.Customer).NotEmpty().MaximumLength(300);
            
            RuleForEach(c => c.Items)
                .ChildRules(item =>
                {
                    item.RuleFor(i => i.Quantity).GreaterThan(0);
                    item.RuleFor(i => i.ProductName).NotEmpty().MaximumLength(300);
                    item.RuleFor(i => i.Price).GreaterThan(0);
                    item.RuleFor(i => i.Currency).NotEmpty().MaximumLength(3);
                    item.RuleFor(i => i.Size).NotEmpty().MaximumLength(50);
                    item.RuleFor(i => i.SugarLevel).NotEmpty().MaximumLength(50);
                    item.RuleFor(i => i.IceLevel).NotEmpty().MaximumLength(50);
                    item.RuleFor(i => i.Temperature).NotEmpty().MaximumLength(50);
                });
        }
    }

    internal sealed class CommandHandler(OrderingDbContext dbContext) : ICommandHandler<Command, Guid>
    {
        public async Task<Result<Guid>> Handle(Command request, CancellationToken cancellationToken)
        {
            Result<Order> orderResult = Order.Create(request.Id, request.Customer, request.Note);

            if (orderResult.IsFailure)
            {
                return Result.Failure<Guid>(orderResult.Error);
            }
            
            Order order = orderResult.Value;
            
            foreach (ItemRequest item in request.Items)
            {
                Result<Money> moneyResult = Money.Create(item.Price, Currency.FromCode(item.Currency));

                if (moneyResult.IsFailure)
                {
                    return Result.Failure<Guid>(moneyResult.Error);
                }
                
                order.AddItem(
                    item.ProductName,
                    moneyResult.Value,
                    item.Quantity,
                    new Parameters(item.Size, item.IceLevel, item.SugarLevel, item.Temperature));
            }
            
            dbContext.Add(order);

            await dbContext.SaveChangesAsync(cancellationToken);

            return order.Id;
        }
    }
}
