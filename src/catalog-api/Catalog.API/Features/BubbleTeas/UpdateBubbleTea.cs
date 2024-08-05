using Catalog.API.Entities.BubbleTeas;
using Catalog.API.Infrastructure.Database;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceDefaults.Domain;
using ServiceDefaults.Endpoints;
using ServiceDefaults.Messaging;

namespace Catalog.API.Features.BubbleTeas;

public static class UpdateBubbleTea
{
    public sealed record Command(
        Guid BubbleTeaId,
        string Name,
        string TeaType,
        decimal Price,
        string Currency) : ICommand;
    
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Name).NotEmpty().MaximumLength(300);
            RuleFor(c => c.TeaType).NotEmpty().MaximumLength(100);
            RuleFor(c => c.Price).GreaterThanOrEqualTo(0);
            RuleFor(c => c.Currency).NotEmpty().MaximumLength(3);
        }
    }

    internal sealed class CommandHandler(CatalogDbContext dbContext) : ICommandHandler<Command>
    {
        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            BubbleTea? bubbleTea = await dbContext.BubbleTeas
                .FirstOrDefaultAsync(b => b.Id == request.BubbleTeaId, cancellationToken);

            if (bubbleTea is null)
            {
                return Result.Failure(BubbleTeaErrors.NotFound(request.BubbleTeaId));
            }

            var teaTypeResult = Result.Create(TeaType.FromName(request.Name));
            Result<Money> moneyResult = Money.Create(request.Price, Currency.FromCode(request.Currency));
            
            var inspectResult = Result.Inspect(teaTypeResult, moneyResult);
            
            if (inspectResult.IsFailure)
            {
                return Result.Failure(inspectResult.Error);
            }
            
            bubbleTea.Update(request.Name, teaTypeResult.Value, moneyResult.Value);
            
            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success(bubbleTea.Id);
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPut("bubble-teas", Handler)
                .WithTags(nameof(BubbleTea))
                .WithName(nameof(UpdateBubbleTea));
        }

        private static async Task<IResult> Handler(ISender sender, Guid bubbleTeaId, Request request)
        {
            var command = new Command(
                bubbleTeaId,
                request.Name,
                request.TeaType,
                request.Price,
                request.Currency);
            
            Result result = await sender.Send(command);

            return result.Match(Results.NoContent, ApiResults.Problem);
        }

        private sealed record Request(string Name, string TeaType, decimal Price, string Currency);
    }
}
