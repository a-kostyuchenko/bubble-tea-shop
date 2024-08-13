using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Ordering.API.Entities.Orders;
using Ordering.API.Infrastructure.Database;
using ServiceDefaults.Domain;
using ServiceDefaults.Endpoints;
using ServiceDefaults.Messaging;

namespace Ordering.API.Features.Orders;

public static class CompleteOrder
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

            Result result = order.Complete();

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
            app.MapPut("orders/{orderId:guid}/complete", Handler)
                .WithTags(nameof(Order))
                .WithName(nameof(CompleteOrder));
        }

        private static async Task<IResult> Handler(ISender sender, Guid orderId)
        {
            var command = new Command(orderId);
            
            Result result = await sender.Send(command);

            return result.Match(Results.NoContent, ApiResults.Problem);
        }
    }
}
