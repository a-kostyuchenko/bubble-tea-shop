using System.Data.Common;
using Dapper;
using MediatR;
using BubbleTea.ServiceDefaults.Domain;
using BubbleTea.ServiceDefaults.Endpoints;
using BubbleTea.ServiceDefaults.Messaging;
using BubbleTea.Services.Catalog.API.Entities.Products;
using BubbleTea.Services.Catalog.API.Infrastructure.Database;

namespace BubbleTea.Services.Catalog.API.Features.Products;

public static class GetProduct
{
    public sealed record Query(string Slug) : IQuery<Response>;

    public sealed record Response(
        Guid Id,
        string Name,
        string Description,
        string Slug,
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
                        p.description AS {nameof(Response.Description)},
                        p.slug AS {nameof(Response.Slug)},
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
                    WHERE p.slug = @Slug
                 """;
            
            Dictionary<string, Response> productsDictionary = [];

            await connection.QueryAsync<Response, IngredientResponse?, ParameterResponse?, Response>(
                sql,
                (product, ingredient, parameter) =>
                {
                    if (productsDictionary.TryGetValue(product.Slug, out Response? existingProduct))
                    {
                        product = existingProduct;
                    }
                    else
                    {
                        productsDictionary.Add(product.Slug, product);
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
        
            if (!productsDictionary.TryGetValue(request.Slug, out Response productResponse))
            {
                return Result.Failure<Response>(ProductErrors.NotFound(request.Slug));
            }

            return productResponse;
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("products/{slug:maxlength(300)}", Handler)
                .WithTags(nameof(Product))
                .WithName(nameof(GetProduct))
                .Produces<Response>();
        }

        private static async Task<IResult> Handler(ISender sender, string slug)
        {
            var query = new Query(slug);
            
            Result<Response> result = await sender.Send(query);

            return result.Match(Results.Ok, ApiResults.Problem);
        }
    }
}
