using Catalog.API.Entities.Ingredients;
using Catalog.API.Entities.Parameters;
using Catalog.API.Entities.Products;
using Catalog.API.Infrastructure.Database;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceDefaults.Domain;
using ServiceDefaults.Endpoints;
using ServiceDefaults.Messaging;

namespace Catalog.API.Features.Products;

public static class RemoveParameter
{
    public sealed record Command(Guid ProductId, Guid ParameterId) : ICommand;
    
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.ProductId).NotEmpty();
            RuleFor(c => c.ParameterId).NotEmpty();
        }
    }

    internal sealed class CommandHandler(CatalogDbContext dbContext) : ICommandHandler<Command>
    {
        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            Product? product = await dbContext.Products
                .Include(b => b.Parameters)
                .FirstOrDefaultAsync(b => b.Id == request.ProductId, cancellationToken);

            if (product is null)
            {
                return Result.Failure(ProductErrors.NotFound(request.ProductId));
            }

            Parameter? parameter = await dbContext.Parameters
                .FirstOrDefaultAsync(i => i.Id == request.ParameterId, cancellationToken);

            if (parameter is null)
            {
                return Result.Failure(ParameterErrors.NotFound(request.ParameterId));
            }
            
            product.RemoveParameter(parameter);
            
            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success(product.Id);
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapDelete("products/{productId:guid}/parameters/{parameterId:guid}", Handler)
                .WithTags(nameof(Product))
                .WithName(nameof(RemoveParameter));
        }

        private static async Task<IResult> Handler(ISender sender, Guid productId, Guid parameterId)
        {
            var command = new Command(productId, parameterId);
            
            Result result = await sender.Send(command);

            return result.Match(Results.NoContent, ApiResults.Problem);
        }
    }
}
