using BubbleTea.Common.Application.Messaging;
using BubbleTea.Common.Domain;
using BubbleTea.Common.Presentation.Endpoints;
using FluentValidation;
using MediatR;
using BubbleTea.Services.Catalog.API.Entities.Ingredients;
using BubbleTea.Services.Catalog.API.Infrastructure.Database;

namespace BubbleTea.Services.Catalog.API.Features.Ingredients;

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
            app.MapPost("ingredients", Handler)
                .WithTags(nameof(Ingredient))
                .WithName(nameof(CreateIngredient));
        }

        private static async Task<IResult> Handler(ISender sender, Request request)
        {
            var command = new Command(request.Name);
            
            Result<Guid> result = await sender.Send(command);

            return result.Match(
                ingredientId => Results.CreatedAtRoute(nameof(GetIngredient), new { ingredientId }, ingredientId),
                ApiResults.Problem);
        }

        private sealed record Request(string Name);
    }
}
