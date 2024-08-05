using Catalog.API.Entities.BubbleTeas;
using Catalog.API.Entities.Ingredients;
using Catalog.API.Infrastructure.Database;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceDefaults.Domain;
using ServiceDefaults.Endpoints;
using ServiceDefaults.Messaging;

namespace Catalog.API.Features.BubbleTeas;

public static class AddIngredient
{
    public sealed record Command(Guid BubbleTeaId, Guid IngredientId) : ICommand;
    
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.BubbleTeaId).NotEmpty();
            RuleFor(c => c.BubbleTeaId).NotEmpty();
        }
    }

    internal sealed class CommandHandler(CatalogDbContext dbContext) : ICommandHandler<Command>
    {
        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            BubbleTea? bubbleTea = await dbContext.BubbleTeas
                .Include(b => b.Ingredients)
                .FirstOrDefaultAsync(b => b.Id == request.BubbleTeaId, cancellationToken);

            if (bubbleTea is null)
            {
                return Result.Failure(BubbleTeaErrors.NotFound(request.BubbleTeaId));
            }

            Ingredient? ingredient = await dbContext.Ingredients
                .FirstOrDefaultAsync(i => i.Id == request.IngredientId, cancellationToken);

            if (ingredient is null)
            {
                return Result.Failure(IngredientErrors.NotFound(request.IngredientId));
            }
            
            bubbleTea.AddIngredient(ingredient);
            
            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success(bubbleTea.Id);
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPut("bubble-teas/{bubbleTeaId:guid}/ingredients/{ingredientId:guid}", Handler)
                .WithTags(nameof(BubbleTea))
                .WithName(nameof(AddIngredient));
        }

        private static async Task<IResult> Handler(ISender sender, Guid bubbleTeaId, Guid ingredientId)
        {
            var command = new Command(bubbleTeaId, ingredientId);
            
            Result result = await sender.Send(command);

            return result.Match(Results.NoContent, ApiResults.Problem);
        }
    }
}
