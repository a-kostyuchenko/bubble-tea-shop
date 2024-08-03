using Catalog.API.Entities.Ingredients;
using Catalog.API.Infrastructure.Database;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceDefaults.Domain;
using ServiceDefaults.Endpoints;
using ServiceDefaults.Messaging;

namespace Catalog.API.Features.Ingredients;

public static class UpdateIngredient
{
    public sealed record Command(Guid IngredientId, string Name) : ICommand;
    
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.IngredientId).NotEmpty();
            RuleFor(c => c.Name).NotEmpty().MaximumLength(300);
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
            
            ingredient.Update(request.Name);

            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPut("ingredients/{id:guid}", Handler)
                .WithName(nameof(UpdateIngredient));
        }

        private static async Task<IResult> Handler(ISender sender, Guid id, Request request)
        {
            var command = new Command(id, request.Name);
            
            Result result = await sender.Send(command);

            return result.Match(Results.NoContent, ApiResults.Problem);
        }

        private sealed record Request(string Name);
    }
}
