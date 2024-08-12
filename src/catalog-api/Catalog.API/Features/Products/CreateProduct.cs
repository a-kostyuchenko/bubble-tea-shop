using Catalog.API.Entities.Products;
using Catalog.API.Infrastructure.Database;
using FluentValidation;
using MediatR;
using ServiceDefaults.Domain;
using ServiceDefaults.Endpoints;
using ServiceDefaults.Messaging;

namespace Catalog.API.Features.Products;

public static class CreateProduct
{
    public sealed record Command(string Name, decimal Price, string Currency) : ICommand<Guid>;
    
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Name).NotEmpty().MaximumLength(300);
            RuleFor(c => c.Price).GreaterThanOrEqualTo(0);
            RuleFor(c => c.Currency).NotEmpty().MaximumLength(3);
        }
    }

    internal sealed class CommandHandler(CatalogDbContext dbContext) : ICommandHandler<Command, Guid>
    {
        public async Task<Result<Guid>> Handle(Command request, CancellationToken cancellationToken)
        {
            Result<Money> moneyResult = Money.Create(request.Price, Currency.FromCode(request.Currency));
            
            if (moneyResult.IsFailure)
            {
                return Result.Failure<Guid>(moneyResult.Error);
            }
            
            Result<Product> productResult = Product.Create(
                request.Name,
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
                .WithName(nameof(CreateProduct));
        }

        private static async Task<IResult> Handler(ISender sender, Request request)
        {
            var command = new Command(request.Name, request.Price, request.Currency);
            
            Result<Guid> result = await sender.Send(command);

            return result.Match(
                productId => Results.CreatedAtRoute(nameof(GetProduct), new { productId }, productId),
                ApiResults.Problem);
        }

        private sealed record Request(string Name, decimal Price, string Currency);
    }
}
