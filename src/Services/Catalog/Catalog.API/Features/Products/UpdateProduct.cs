using Catalog.API.Entities.Products;
using Catalog.API.Infrastructure.Database;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceDefaults.Domain;
using ServiceDefaults.Endpoints;
using ServiceDefaults.Messaging;

namespace Catalog.API.Features.Products;

public static class UpdateProduct
{
    public sealed record Command(
        Guid ProductId,
        string Name,
        string Description,
        string Category,
        decimal Price,
        string Currency) : ICommand;
    
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Name).NotEmpty().MaximumLength(300);
            RuleFor(c => c.Description).NotEmpty().MaximumLength(1000);
            RuleFor(c => c.Category).NotEmpty().MaximumLength(100);
            RuleFor(c => c.Price).GreaterThanOrEqualTo(0);
            RuleFor(c => c.Currency).NotEmpty().MaximumLength(3);
        }
    }

    internal sealed class CommandHandler(CatalogDbContext dbContext) : ICommandHandler<Command>
    {
        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            Product? product = await dbContext.Products
                .FirstOrDefaultAsync(b => b.Id == request.ProductId, cancellationToken);

            if (product is null)
            {
                return Result.Failure(ProductErrors.NotFound(request.ProductId));
            }

            Result<Money> moneyResult = Money.Create(request.Price, Currency.FromCode(request.Currency));
            var categoryResult = Result.Create(Category.FromName(request.Category));
            
            var inspection = Result.Inspect(moneyResult, categoryResult);
            
            if (inspection.IsFailure)
            {
                return inspection;
            }
            
            product.Update(request.Description, categoryResult.Value, moneyResult.Value);

            if (product.Name != request.Name)
            {
                string slug = "slug"; // TODO: Implement slug generation
                
                product.UpdateName(request.Name, slug);
            }
            
            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPut("products/{productId:guid}", Handler)
                .WithTags(nameof(Product))
                .WithName(nameof(UpdateProduct));
        }

        private static async Task<IResult> Handler(ISender sender, Guid productId, Request request)
        {
            var command = new Command(
                productId,
                request.Name,
                request.Description,
                request.Category,
                request.Price,
                request.Currency);
            
            Result result = await sender.Send(command);

            return result.Match(Results.NoContent, ApiResults.Problem);
        }

        private sealed record Request(string Name, string Description, string Category, decimal Price, string Currency);
    }
}
