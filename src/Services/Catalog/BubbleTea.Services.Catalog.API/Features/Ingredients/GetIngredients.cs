using System.Data.Common;
using BubbleTea.Common.Application.Messaging;
using BubbleTea.Common.Application.Paging;
using BubbleTea.Common.Domain;
using BubbleTea.Common.Presentation.Endpoints;
using Dapper;
using MediatR;
using BubbleTea.Services.Catalog.API.Entities.Ingredients;
using BubbleTea.Services.Catalog.API.Infrastructure.Database;

namespace BubbleTea.Services.Catalog.API.Features.Ingredients;

public static class GetIngredients
{
    public sealed record Query(string? SearchTerm, int Page, int PageSize) : IQuery<PagedResponse<Response>>;
    public sealed record Response(Guid Id, string Name);

    internal sealed class QueryHandler(IDbConnectionFactory dbConnectionFactory) 
        : IQueryHandler<Query, PagedResponse<Response>>
    {
        public async Task<Result<PagedResponse<Response>>> Handle(Query request, CancellationToken cancellationToken)
        {
            await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

            var parameters = new {
                SearchTerm = $"%{request.SearchTerm}%",
                Take = request.PageSize,
                Skip = (request.Page - 1) * request.PageSize
            };
            
            IReadOnlyCollection<Response> ingredients = await GetIngredientsAsync(connection, parameters);
            
            int totalCount = await CountIngredientsAsync(connection, parameters);
            
            return new PagedResponse<Response>(request.Page, request.PageSize, totalCount, ingredients);
        }
        
        private static async Task<IReadOnlyCollection<Response>> GetIngredientsAsync(
            DbConnection connection,
            object parameters)
        {
            const string sql = 
                $@"
                SELECT
                    i.id AS {nameof(Response.Id)},
                    i.name AS {nameof(Response.Name)}
                FROM catalog.ingredients i
                WHERE (@SearchTerm IS NULL OR i.name ILIKE @SearchTerm)
                ORDER BY i.id
                OFFSET @Skip
                LIMIT @Take
                ";

            List<Response> ingredients = (await connection.QueryAsync<Response>(
                    sql,
                    parameters))
                .AsList();

            return ingredients;
        }
    
        private static async Task<int> CountIngredientsAsync(DbConnection connection, object parameters)
        {
            const string sql =
                """
                SELECT COUNT(*)
                FROM catalog.ingredients i
                WHERE (@SearchTerm IS NULL OR i.name ILIKE @SearchTerm)
                """;

            int totalCount = await connection.ExecuteScalarAsync<int>(sql, parameters);

            return totalCount;
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("ingredients", Handler)
                .WithTags(nameof(Ingredient))
                .WithName(nameof(GetIngredients))
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
