using Catalog.API.Entities.Products;
using Catalog.API.Infrastructure.Database;
using Catalog.API.Infrastructure.Storage;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ServiceDefaults.Domain;
using ServiceDefaults.Endpoints;
using ServiceDefaults.Messaging;

namespace Catalog.API.Features.Products;

public static class CreateProduct
{
    public sealed record Command(
        string Name,
        string Description,
        string Category,
        decimal Price,
        string Currency,
        Stream Stream,
        string ContentType) : ICommand<Guid>;
    
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Name).NotEmpty().MaximumLength(300);
            RuleFor(c => c.Description).NotEmpty().MaximumLength(1000);
            RuleFor(c => c.Category).NotEmpty().MaximumLength(100);
            RuleFor(c => c.Price).GreaterThanOrEqualTo(0);
            RuleFor(c => c.Currency).NotEmpty().MaximumLength(3);
            RuleFor(c => c.Stream).NotEmpty();
            RuleFor(c => c.ContentType).NotEmpty();
        }
    }

    internal sealed class CommandHandler(
        CatalogDbContext dbContext,
        IBlobService blobService) : ICommandHandler<Command, Guid>
    {
        public async Task<Result<Guid>> Handle(Command request, CancellationToken cancellationToken)
        {
            Result<Money> moneyResult = Money.Create(request.Price, Currency.FromCode(request.Currency));
            var categoryResult = Result.Create(Category.FromName(request.Category));
            
            var inspection = Result.Inspect(moneyResult, categoryResult);
            
            if (inspection.IsFailure)
            {
                return Result.Failure<Guid>(inspection.Error);
            }
            
            Guid imageId = await blobService.UploadAsync(request.Stream, request.ContentType, cancellationToken);

            // TODO: Implement slug generation
            string slug = "slug";
            
            Result<Product> productResult = Product.Create(
                request.Name,
                slug,
                request.Description,
                categoryResult.Value,
                imageId,
                moneyResult.Value);

            if (productResult.IsFailure)
            {
                return Result.Failure<Guid>(productResult.Error);
            }

            dbContext.Add(productResult.Value);

            await dbContext.SaveChangesAsync(cancellationToken);

            return productResult.Value.Id;
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("products", Handler)
                .WithTags(nameof(Product))
                .WithName(nameof(CreateProduct))
                .DisableAntiforgery();
        }

        private static async Task<IResult> Handler(ISender sender, [AsParameters] Request request)
        {
            var command = new Command(
                request.Name,
                request.Description,
                request.Category,
                request.Price,
                request.Currency,
                request.Image.OpenReadStream(),
                request.Image.ContentType);
            
            Result<Guid> result = await sender.Send(command);

            return result.Match(
                productId => Results.CreatedAtRoute(nameof(GetProduct), new { productId }, productId),
                ApiResults.Problem);
        }

        private sealed record Request(
            [FromForm] string Name,
            [FromForm] string Description,
            [FromForm] string Category,
            [FromForm] decimal Price,
            [FromForm] string Currency,
            IFormFile Image);
    }
}
