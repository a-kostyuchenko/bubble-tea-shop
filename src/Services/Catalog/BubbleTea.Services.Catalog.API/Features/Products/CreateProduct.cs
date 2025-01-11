using BubbleTea.Common.Application.Messaging;
using BubbleTea.Common.Application.Slugs;
using BubbleTea.Common.Domain;
using BubbleTea.Common.Presentation.Endpoints;
using BubbleTea.Services.Catalog.API.Infrastructure.Database.Queries;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using BubbleTea.Services.Catalog.API.Entities.Products;
using BubbleTea.Services.Catalog.API.Infrastructure.Database;
using BubbleTea.Services.Catalog.API.Infrastructure.Storage;

namespace BubbleTea.Services.Catalog.API.Features.Products;

public static class CreateProduct
{
    public sealed record Command(
        string Name,
        string Description,
        string Category,
        decimal Price,
        string Currency,
        Stream Stream,
        string ContentType) : ICommand<string>;
    
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
        IBlobService blobService,
        ProductNameToSlug productNameToSlug) : ICommandHandler<Command, string>
    {
        public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            Result<Money> moneyResult = Money.Create(request.Price, Currency.FromCode(request.Currency));
            var categoryResult = Result.Create(Category.FromName(request.Category));
            
            var inspection = Result.Inspect(moneyResult, categoryResult);
            
            if (inspection.IsFailure)
            {
                return Result.Failure<string>(inspection.Error);
            }
            
            Guid imageId = await blobService.UploadAsync(request.Stream, request.ContentType, cancellationToken);

            Slug slugCandidate = productNameToSlug(request.Name);

            (string? collision, IEnumerable<string> similar) = await dbContext.Products
                .Select(p => p.Slug)
                .FindCollisions(slugCandidate);

            var slug = Slug.AvoidCollisionsWithNumber(slugCandidate, collision, similar);
            
            Result<Product> productResult = Product.Create(
                request.Name,
                slug.Value,
                request.Description,
                categoryResult.Value,
                imageId,
                moneyResult.Value);

            if (productResult.IsFailure)
            {
                return Result.Failure<string>(productResult.Error);
            }

            dbContext.Add(productResult.Value);

            await dbContext.SaveChangesAsync(cancellationToken);

            return productResult.Value.Slug;
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
            
            Result<string> result = await sender.Send(command);

            return result.Match(
                slug => Results.CreatedAtRoute(nameof(GetProduct), new { slug }, slug),
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
