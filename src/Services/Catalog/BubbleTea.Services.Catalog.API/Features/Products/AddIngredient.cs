using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using BubbleTea.ServiceDefaults.Domain;
using BubbleTea.ServiceDefaults.Endpoints;
using BubbleTea.ServiceDefaults.Messaging;
using BubbleTea.Services.Catalog.API.Entities.Ingredients;
using BubbleTea.Services.Catalog.API.Entities.Products;
using BubbleTea.Services.Catalog.API.Infrastructure.Database;

namespace BubbleTea.Services.Catalog.API.Features.Products;

public static class AddIngredient
{
    public sealed record Command(Guid ProductId, Guid IngredientId) : ICommand;
    
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.ProductId).NotEmpty();
            RuleFor(c => c.IngredientId).NotEmpty();
        }
    }

    internal sealed class CommandHandler(CatalogDbContext dbContext) : ICommandHandler<Command>
    {
        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            Product? product = await dbContext.Products
                .Include(b => b.Ingredients)
                .FirstOrDefaultAsync(b => b.Id == request.ProductId, cancellationToken);

            if (product is null)
            {
                return Result.Failure(ProductErrors.NotFound(request.ProductId));
            }

            Ingredient? ingredient = await dbContext.Ingredients
                .FirstOrDefaultAsync(i => i.Id == request.IngredientId, cancellationToken);

            if (ingredient is null)
            {
                return Result.Failure(IngredientErrors.NotFound(request.IngredientId));
            }
            
            product.AddIngredient(ingredient);
            
            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPut("products/{productId:guid}/ingredients/{ingredientId:guid}", Handler)
                .WithTags(nameof(Product))
                .WithName(nameof(AddIngredient));
        }

        private static async Task<IResult> Handler(ISender sender, Guid productId, Guid ingredientId)
        {
            var command = new Command(productId, ingredientId);
            
            Result result = await sender.Send(command);

            return result.Match(Results.NoContent, ApiResults.Problem);
        }
    }
}
