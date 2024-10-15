using BubbleTeaShop.Contracts;
using FluentValidation;
using Ordering.API.Entities.Orders;
using Ordering.API.Entities.Orders.Events;
using Ordering.API.Infrastructure.Database;
using Ordering.API.Infrastructure.EventBus;
using ServiceDefaults.Domain;
using ServiceDefaults.Messaging;

namespace Ordering.API.Features.Orders;

public static class CreateOrder
{
    public sealed record ItemRequest(
        string ProductName,
        int Quantity,
        decimal Price,
        string Currency);
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
                    item.Quantity);
            }
            
            dbContext.Add(order);

            await dbContext.SaveChangesAsync(cancellationToken);

            return order.Id;
        }
    }
    
    internal sealed class OrderCreatedDomainEventHandler(IEventBus eventBus) 
        : DomainEventHandler<OrderCreatedDomainEvent>
    {
        public override async Task Handle(
            OrderCreatedDomainEvent domainEvent,
            CancellationToken cancellationToken = default)
        {
            await eventBus.PublishAsync(new OrderCreatedEvent(
                domainEvent.Id,
                domainEvent.OccurredOnUtc,
                domainEvent.OrderId), cancellationToken);
        }
    }
}
