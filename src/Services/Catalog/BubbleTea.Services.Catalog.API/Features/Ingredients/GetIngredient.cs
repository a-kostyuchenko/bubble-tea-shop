using System.Data.Common;
using BubbleTea.Common.Application.Messaging;
using BubbleTea.Common.Domain;
using BubbleTea.Common.Presentation.Endpoints;
using Dapper;
using MediatR;
using BubbleTea.Services.Catalog.API.Entities.Ingredients;
using BubbleTea.Services.Catalog.API.Infrastructure.Database;

namespace BubbleTea.Services.Catalog.API.Features.Ingredients;

public static class GetIngredient
{
    public sealed record Query(Guid IngredientId) : IQuery<Response>;
    public sealed record Response(Guid Id, string Name);

    internal sealed class QueryHandler(IDbConnectionFactory dbConnectionFactory) : IQueryHandler<Query, Response>
    {
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();
            
            const string sql = 
                $@"
                SELECT
                    i.id AS {nameof(Response.Id)},
                    i.name AS {nameof(Response.Name)}
                FROM catalog.ingredients i
                WHERE i.id = @IngredientId
                ";
            
            Response? ingredient = await connection.QueryFirstOrDefaultAsync<Response>(
                sql,
                new { request.IngredientId });

            if (ingredient is null)
            {
                return Result.Failure<Response>(IngredientErrors.NotFound(request.IngredientId));
            }

            return ingredient;
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("ingredients/{ingredientId:guid}", Handler)
                .WithTags(nameof(Ingredient))
                .WithName(nameof(GetIngredient))
                .Produces<Response>();
        }

        private static async Task<IResult> Handler(ISender sender, Guid ingredientId)
        {
            var query = new Query(ingredientId);
            
            Result<Response> result = await sender.Send(query);

            return result.Match(Results.Ok, ApiResults.Problem);
        }
    }
}
