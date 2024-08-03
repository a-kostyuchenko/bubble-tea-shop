using Catalog.API.Entities.Ingredients;
using Catalog.API.Infrastructure.Database;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceDefaults.Domain;
using ServiceDefaults.Endpoints;
using ServiceDefaults.Messaging;

namespace Catalog.API.Features.Ingredients;

public static class DeleteIngredient
{
    public sealed record Command(Guid IngredientId) : ICommand;
    
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.IngredientId).NotEmpty();
        }
    }

    internal sealed class CommandHandler(CatalogDbContext dbContext) : ICommandHandler<Command>
    {
        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            Ingredient? ingredient = await dbContext.Ingredients
                .FirstOrDefaultAsync(i => i.Id == request.IngredientId, cancellationToken);

            if (ingredient is null)
            {
                return Result.Failure(IngredientErrors.NotFound(request.IngredientId));
            }

            dbContext.Remove(ingredient);
            
            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapDelete("ingredients/{ingredientId:guid}", Handler)
                .WithTags(nameof(Ingredient))
                .WithName(nameof(DeleteIngredient));
        }

        private static async Task<IResult> Handler(ISender sender, Guid ingredientId)
        {
            var command = new Command(ingredientId);
            
            Result result = await sender.Send(command);

            return result.Match(Results.NoContent, ApiResults.Problem);
        }
    }
}
