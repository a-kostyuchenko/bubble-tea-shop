using BubbleTea.Common.Application.Messaging;
using BubbleTea.Common.Domain;
using BubbleTea.Contracts;
using FluentValidation;
using BubbleTea.Services.Orders.API.Entities.Orders;
using BubbleTea.Services.Orders.API.Entities.Orders.Events;
using BubbleTea.Services.Orders.API.Infrastructure.Database;
using BubbleTea.Services.Orders.API.Infrastructure.EventBus;

namespace BubbleTea.Services.Orders.API.Features.Orders;

public static class CreateOrder
{
    public sealed record Command(Guid Id, string Customer, string? Note, List<CartItemModel> Items) : ICommand<Guid>;
    
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
                    
                    item.RuleForEach(i => i.Parameters)
                        .ChildRules(parameter =>
                        {
                            parameter.RuleFor(p => p.Name).NotEmpty().MaximumLength(300);
                            parameter.RuleFor(p => p.SelectedOption).NotEmpty();
                            parameter.RuleFor(p => p.SelectedOption.Name).NotEmpty().MaximumLength(300);
                            parameter.RuleFor(p => p.SelectedOption.ExtraPrice).GreaterThanOrEqualTo(0);
                            parameter.RuleFor(p => p.SelectedOption.Currency).NotEmpty().MaximumLength(3);
                        });
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
            
            foreach (CartItemModel item in request.Items)
            {
                var parameters = new List<Parameter>();
                
                foreach (ParameterModel parameter in item.Parameters)
                {
                    Result<Money> extraPriceResult = Money.Create(
                        parameter.SelectedOption.ExtraPrice,
                        Currency.FromCode(parameter.SelectedOption.Currency));

                    if (extraPriceResult.IsFailure)
                    {
                        return Result.Failure<Guid>(extraPriceResult.Error);
                    }
                    
                    parameters.Add(Parameter.Create(parameter.Name, parameter.SelectedOption.Name, extraPriceResult.Value));
                }
                
                Result<Money> moneyResult = Money.Create(item.Price, Currency.FromCode(item.Currency));

                if (moneyResult.IsFailure)
                {
                    return Result.Failure<Guid>(moneyResult.Error);
                }
                
                order.AddItem(
                    item.ProductName,
                    moneyResult.Value,
                    item.Quantity,
                    parameters);
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
