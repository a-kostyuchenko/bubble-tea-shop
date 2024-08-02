using Catalog.API.Database;
using Catalog.API.Entities.Ingredients;
using Catalog.API.Infrastructure.Database;
using FluentValidation;
using MediatR;
using ServiceDefaults.Domain;
using ServiceDefaults.Endpoints;
using ServiceDefaults.Messaging;

namespace Catalog.API.Features.Ingredients;

public static class CreateIngredient
{
    public sealed record Command(string Name) : ICommand<Guid>;
    
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
            Result<Ingredient> ingredientResult = Ingredient.Create(request.Name);

            if (ingredientResult.IsFailure)
            {
                return Result.Failure<Guid>(ingredientResult.Error);
            }

            dbContext.Add(ingredientResult.Value);

            await dbContext.SaveChangesAsync(cancellationToken);

            return ingredientResult.Value.Id;
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("ingredients", Handler);
        }

        private static async Task<IResult> Handler(ISender sender, Request request)
        {
            var command = new Command(request.Name);
            
            Result<Guid> result = await sender.Send(command);

            return result.Match(Results.Ok, ApiResults.Problem);
        }

        private sealed record Request(string Name);
    }
}
