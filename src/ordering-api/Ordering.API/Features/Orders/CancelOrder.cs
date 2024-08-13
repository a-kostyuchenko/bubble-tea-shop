using BubbleTeaShop.Contracts;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Ordering.API.Entities.Orders;
using Ordering.API.Entities.Orders.Events;
using Ordering.API.Infrastructure.Database;
using Ordering.API.Infrastructure.EventBus;
using ServiceDefaults.Domain;
using ServiceDefaults.Endpoints;
using ServiceDefaults.Messaging;

namespace Ordering.API.Features.Orders;

public static class CancelOrder
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

            Result result = order.Cancel();

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
            app.MapDelete("orders/{orderId:guid}/cancel", Handler)
                .WithTags(nameof(Order))
                .WithName(nameof(CancelOrder));
        }

        private static async Task<IResult> Handler(ISender sender, Guid orderId)
        {
            var command = new Command(orderId);
            
            Result result = await sender.Send(command);

            return result.Match(Results.NoContent, ApiResults.Problem);
        }
    }
    
    internal sealed class OrderCancelledDomainEventHandler(IEventBus eventBus) 
        : DomainEventHandler<OrderCancelledDomainEvent>
    {
        public override async Task Handle(
            OrderCancelledDomainEvent domainEvent,
            CancellationToken cancellationToken = default)
        {
            await eventBus.PublishAsync(
                new OrderCancelledEvent(
                domainEvent.Id,
                domainEvent.OccurredOnUtc,
                domainEvent.OrderId),
                cancellationToken);
        }
    }
}
