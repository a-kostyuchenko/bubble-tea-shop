using System.Data.Common;
using BubbleTea.Common.Application.Data;
using BubbleTea.Common.Application.Messaging;
using BubbleTea.Common.Application.Paging;
using BubbleTea.Common.Domain;
using BubbleTea.Common.Presentation.Endpoints;
using Dapper;
using MediatR;
using BubbleTea.Services.Catalog.API.Entities.Products;

namespace BubbleTea.Services.Catalog.API.Features.Products;

public static class GetProducts
{
    public sealed record Query(string? SearchTerm, int Page, int PageSize) : IQuery<PagedResponse<Response>>;

    public sealed record Response(
        Guid Id,
        string Name,
        string Description,
        string Slug,
        string Category,
        decimal Price,
        string Currency,
        float Rank)
    {
        public List<IngredientResponse> Ingredients { get; init; } = [];
    }
    
    public sealed record IngredientResponse(Guid IngredientId, string Name);

    internal sealed class QueryHandler(IDbConnectionFactory dbConnectionFactory) 
        : IQueryHandler<Query, PagedResponse<Response>>
    {
        public async Task<Result<PagedResponse<Response>>> Handle(Query request, CancellationToken cancellationToken)
        {
            await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();
            
            var parameters = new {
                request.SearchTerm,
                Take = request.PageSize,
                Skip = (request.Page - 1) * request.PageSize
            };
            
            IReadOnlyCollection<Response> products = await GetProductsAsync(connection, parameters);
            
            int totalCount = await CountProductsAsync(connection, parameters);
            
            return new PagedResponse<Response>(request.Page, request.PageSize, totalCount, products);
        }
        
        private static async Task<IReadOnlyCollection<Response>> GetProductsAsync(
            DbConnection connection,
            object parameters)
        {
            const string sql = 
                $@"
                SELECT
                   p.id AS {nameof(Response.Id)},
                   p.name AS {nameof(Response.Name)},
                   p.description AS {nameof(Response.Description)},
                   p.slug AS {nameof(Response.Slug)},
                   p.category AS {nameof(Response.Category)},
                   p.amount AS {nameof(Response.Price)},
                   p.currency AS {nameof(Response.Currency)},
                   ts_rank(to_tsvector('english', p.name || ' ' || p.description), phraseto_tsquery('english', @SearchTerm)) AS {nameof(Response.Rank)},
                   i.id AS {nameof(IngredientResponse.IngredientId)},
                   i.name AS {nameof(IngredientResponse.Name)}
                FROM catalog.products p
                LEFT JOIN catalog.product_ingredients pi ON pi.product_id = p.id
                LEFT JOIN catalog.ingredients i ON i.id = pi.ingredient_id
                WHERE to_tsvector('english', p.name || ' ' || p.description) @@ phraseto_tsquery('english', @SearchTerm)
                ORDER BY ts_rank(to_tsvector('english', p.name || ' ' || p.description), phraseto_tsquery('english', @SearchTerm)) DESC
                OFFSET @Skip
                LIMIT @Take
                ";
            
            Dictionary<string, Response> productsDictionary = [];

            await connection.QueryAsync<Response, IngredientResponse?, Response>(
                sql,
                (product, ingredient) =>
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

                    return product;
                },
                parameters,
                splitOn: nameof(IngredientResponse.IngredientId));

            return productsDictionary.Values;
        }
        
        private static async Task<int> CountProductsAsync(DbConnection connection, object parameters)
        {
            const string sql =
                $"""
                SELECT COUNT(*)
                FROM catalog.products p
                WHERE to_tsvector('english', p.name || ' ' || p.description) @@ phraseto_tsquery('english', @SearchTerm)
                """;

            int totalCount = await connection.ExecuteScalarAsync<int>(sql, parameters);

            return totalCount;
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("products", Handler)
                .WithTags(nameof(Product))
                .WithName(nameof(GetProducts))
                .Produces<PagedResponse<Response>>();
        }

        private static async Task<IResult> Handler(
            ISender sender,
            string? searchTerm,
            int page = 1,
            int pageSize = 15)
        {
            var query = new Query(searchTerm, page, pageSize);
            
            Result<PagedResponse<Response>> result = await sender.Send(query);

            return result.Match(Results.Ok, ApiResults.Problem);
        }
    }
}
