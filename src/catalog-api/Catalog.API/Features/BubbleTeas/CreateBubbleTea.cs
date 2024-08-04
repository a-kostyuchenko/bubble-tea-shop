using Catalog.API.Entities.BubbleTeas;
using Catalog.API.Entities.Ingredients;
using Catalog.API.Infrastructure.Database;
using FluentValidation;
using MediatR;
using ServiceDefaults.Domain;
using ServiceDefaults.Endpoints;
using ServiceDefaults.Messaging;

namespace Catalog.API.Features.BubbleTeas;

public static class CreateBubbleTea
{
    public sealed record Command(string Name, string TeaType, decimal Price, string Currency) : ICommand<Guid>;
    
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Name).NotEmpty().MaximumLength(300);
        }
    }

    internal sealed class CommandHandler(CatalogDbContext dbContext) : ICommandHandler<Command, Guid>
    {
        public async Task<Result<Guid>> Handle(Command request, CancellationToken cancellationToken)
        {
            // TODO: Validate TeaType
            
            Result<Money> moneyResult = Money.Create(request.Price, Currency.FromCode(request.Currency));

            if (moneyResult.IsFailure)
            {
                return Result.Failure<Guid>(moneyResult.Error);
            }
            
            Result<BubbleTea> bubbleTeaResult = BubbleTea.Create(
                request.Name,
                TeaType.FromName(request.Name)!,
                moneyResult.Value);

            if (bubbleTeaResult.IsFailure)
            {
                return Result.Failure<Guid>(bubbleTeaResult.Error);
            }

            dbContext.Add(bubbleTeaResult.Value);

            await dbContext.SaveChangesAsync(cancellationToken);

            return bubbleTeaResult.Value.Id;
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("bubble-teas", Handler)
                .WithTags(nameof(BubbleTea))
                .WithName(nameof(CreateBubbleTea));
        }

        private static async Task<IResult> Handler(ISender sender, Request request)
        {
            var command = new Command(request.Name, request.TeaType, request.Price, request.Currency);
            
            Result<Guid> result = await sender.Send(command);

            return result.Match(
                ingredientId => Results.CreatedAtRoute("", new { ingredientId }, ingredientId),
                ApiResults.Problem);
        }

        private sealed record Request(string Name, string TeaType, decimal Price, string Currency);
    }
}
