using BubbleTea.Common.Application.Messaging;
using BubbleTea.Common.Domain;
using BubbleTea.Contracts;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using BubbleTea.Services.Orders.API.Entities.Orders;
using BubbleTea.Services.Orders.API.Entities.Orders.Events;
using BubbleTea.Services.Orders.API.Infrastructure.Database;
using BubbleTea.Services.Orders.API.Infrastructure.EventBus;

namespace BubbleTea.Services.Orders.API.Features.Orders;

public static class PayOrder
{
    public sealed record Command(Guid OrderId) : ICommand;
    
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.OrderId).NotEmpty();
        }
    }

    internal sealed class CommandHandler(OrderingDbContext dbContext) : ICommandHandler<Command>
    {
        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            Order? order = await dbContext.Orders
                .FirstOrDefaultAsync(c => c.Id == request.OrderId, cancellationToken);

            if (order is null)
            {
                return Result.Failure(OrderErrors.NotFound(request.OrderId));
            }

            Result result = order.Pay();

            if (result.IsFailure)
            {
                return result;
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
    
    internal sealed class OrderPaidDomainEventHandler(IEventBus eventBus) 
        : DomainEventHandler<OrderPaidDomainEvent>
    {
        public override async Task Handle(
            OrderPaidDomainEvent domainEvent,
            CancellationToken cancellationToken = default)
        {
            await eventBus.PublishAsync(new OrderPaidEvent(
                    domainEvent.Id,
                    domainEvent.OccurredOnUtc,
                    domainEvent.OrderId),
                cancellationToken);
        }
    }
}
