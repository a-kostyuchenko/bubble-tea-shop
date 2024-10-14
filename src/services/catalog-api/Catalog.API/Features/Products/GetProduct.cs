using System.Data.Common;
using Catalog.API.Entities.Products;
using Catalog.API.Infrastructure.Database;
using Dapper;
using MediatR;
using ServiceDefaults.Domain;
using ServiceDefaults.Endpoints;
using ServiceDefaults.Messaging;

namespace Catalog.API.Features.Products;

public static class GetProduct
{
    public sealed record Query(Guid ProductId) : IQuery<Response>;

    public sealed record Response(
        Guid Id,
        string Name,
        string Category,
        decimal Price,
        string Currency)
    {
        public List<IngredientResponse> Ingredients { get; init; } = [];
        public List<ParameterResponse> Parameters { get; init; } = [];
    }
    
    public sealed record IngredientResponse(Guid IngredientId, string Name);
    public sealed record ParameterResponse(Guid ParameterId, string Name);

    internal sealed class QueryHandler(IDbConnectionFactory dbConnectionFactory) : IQueryHandler<Query, Response>
    {
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();
            
            const string sql = 
                $"""
                    SELECT
                        p.id AS {nameof(Response.Id)},
                        p.name AS {nameof(Response.Name)},
                        p.category AS {nameof(Response.Category)},
                        p.amount AS {nameof(Response.Price)},
                        p.currency AS {nameof(Response.Currency)},
                        i.id AS {nameof(IngredientResponse.IngredientId)},
                        i.name AS {nameof(IngredientResponse.Name)},
                        pr.id AS {nameof(ParameterResponse.ParameterId)},
                        pr.name AS {nameof(ParameterResponse.Name)}
                    FROM catalog.products p
                    LEFT JOIN catalog.product_ingredients pi ON pi.product_id = p.id
                    LEFT JOIN catalog.ingredients i ON i.id = pi.ingredient_id
                    LEFT JOIN catalog.product_parameters pp ON pp.product_id = p.id
                    LEFT JOIN catalog.parameters pr ON pr.id = pp.parameter_id
                    WHERE p.id = @ProductId
                 """;
            
            Dictionary<Guid, Response> productsDictionary = [];

            await connection.QueryAsync<Response, IngredientResponse?, ParameterResponse?, Response>(
                sql,
                (product, ingredient, parameter) =>
                {
                    if (productsDictionary.TryGetValue(product.Id, out Response? existingProduct))
                    {
                        product = existingProduct;
                    }
                    else
                    {
                        productsDictionary.Add(product.Id, product);
                    }

                    if (ingredient is not null)
                    {
                        product.Ingredients.Add(ingredient);
                    }

                    if (parameter is not null)
                    {
                        product.Parameters.Add(parameter);
                    }
                
                    return product;
                },
                request,
                splitOn: $"{nameof(IngredientResponse.IngredientId)}, {nameof(ParameterResponse.ParameterId)}");
        
            if (!productsDictionary.TryGetValue(request.ProductId, out Response productResponse))
            {
                return Result.Failure<Response>(ProductErrors.NotFound(request.ProductId));
            }

            return productResponse;
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("products/{productId:guid}", Handler)
                .WithTags(nameof(Product))
                .WithName(nameof(GetProduct))
                .Produces<Response>();
        }

        private static async Task<IResult> Handler(ISender sender, Guid productId)
        {
            var query = new Query(productId);
            
            Result<Response> result = await sender.Send(query);

            return result.Match(Results.Ok, ApiResults.Problem);
        }
    }
}
