using System.Data.Common;
using Catalog.API.Entities.BubbleTeas;
using Catalog.API.Infrastructure.Database;
using Dapper;
using MediatR;
using ServiceDefaults.Domain;
using ServiceDefaults.Endpoints;
using ServiceDefaults.Messaging;

namespace Catalog.API.Features.BubbleTeas;

public static class GetBubbleTea
{
    public sealed record Query(Guid BubbleTeaId) : IQuery<Response>;

    public sealed record Response(
        Guid Id,
        string Name,
        decimal Price,
        string Currency)
    {
        public List<IngredientResponse> Ingredients { get; init; } = [];
    }
    
    public sealed record IngredientResponse(Guid IngredientId, string Name);

    internal sealed class QueryHandler(IDbConnectionFactory dbConnectionFactory) : IQueryHandler<Query, Response>
    {
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();
            
            const string sql = 
                $"""
                    SELECT
                        bt.id AS {nameof(Response.Id)},
                        bt.name AS {nameof(Response.Name)},
                        bt.amount AS {nameof(Response.Price)},
                        bt.currency AS {nameof(Response.Currency)},
                        i.id AS {nameof(IngredientResponse.IngredientId)},
                        i.name AS {nameof(IngredientResponse.Name)}
                    FROM catalog.bubble_teas bt
                    LEFT JOIN catalog.bubble_tea_ingredients bti ON bti.bubble_tea_id = bt.id
                    LEFT JOIN catalog.ingredients i ON i.id = bti.ingredient_id
                    WHERE bt.id = @BubbleTeaId
                 """;
            
            Dictionary<Guid, Response> bubbleTeasDictionary = [];

            await connection.QueryAsync<Response, IngredientResponse?, Response>(
                sql,
                (bubbleTea, ingredient) =>
                {
                    if (bubbleTeasDictionary.TryGetValue(bubbleTea.Id, out Response? existingBubbleTea))
                    {
                        bubbleTea = existingBubbleTea;
                    }
                    else
                    {
                        bubbleTeasDictionary.Add(bubbleTea.Id, bubbleTea);
                    }

                    if (ingredient is not null)
                    {
                        bubbleTea.Ingredients.Add(ingredient);
                    }
                
                    return bubbleTea;
                },
                request,
                splitOn: nameof(IngredientResponse.IngredientId));
        
            if (!bubbleTeasDictionary.TryGetValue(request.BubbleTeaId, out Response bubbleTeaResponse))
            {
                return Result.Failure<Response>(BubbleTeaErrors.NotFound(request.BubbleTeaId));
            }

            return bubbleTeaResponse;
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("bubble-teas/{bubbleTeaId:guid}", Handler)
                .WithTags(nameof(BubbleTea))
                .WithName(nameof(GetBubbleTea))
                .Produces<Response>();
        }

        private static async Task<IResult> Handler(ISender sender, Guid bubbleTeaId)
        {
            var query = new Query(bubbleTeaId);
            
            Result<Response> result = await sender.Send(query);

            return result.Match(Results.Ok, ApiResults.Problem);
        }
    }
}
