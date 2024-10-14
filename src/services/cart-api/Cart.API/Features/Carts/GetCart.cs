using System.Data.Common;
using Cart.API.Entities.Carts;
using Cart.API.Infrastructure.Database;
using Dapper;
using MediatR;
using ServiceDefaults.Domain;
using ServiceDefaults.Endpoints;
using ServiceDefaults.Messaging;

namespace Cart.API.Features.Carts;

public static class GetCart
{
    public sealed record Query(Guid CartId) : IQuery<Response>;

    public sealed record Response(
        Guid Id,
        string Customer)
    {
        public List<ItemResponse> Items { get; init; } = [];
        public decimal TotalPrice => Items.Sum(i => i.Price * i.Quantity);
    }
    
    public sealed record ItemResponse(
        Guid ItemId,
        Guid ProductId,
        int Quantity,
        string ProductName,
        decimal Price,
        string Currency,
        string Size,
        string SugarLevel,
        string IceLevel,
        string Temperature);

    internal sealed class QueryHandler(IDbConnectionFactory dbConnectionFactory) : IQueryHandler<Query, Response>
    {
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();
            
            const string sql = 
                $"""
                    SELECT
                        c.id AS {nameof(Response.Id)},
                        c.customer AS {nameof(Response.Customer)},
                        i.id AS {nameof(ItemResponse.ItemId)},
                        i.product_id AS {nameof(ItemResponse.ProductId)},
                        i.quantity AS {nameof(ItemResponse.Quantity)},
                        i.product_name AS {nameof(ItemResponse.ProductName)},
                        i.amount AS {nameof(ItemResponse.Price)},
                        i.currency AS {nameof(ItemResponse.Currency)},
                        i.size AS {nameof(ItemResponse.Size)},
                        i.sugar_level AS {nameof(ItemResponse.SugarLevel)},
                        i.ice_level AS {nameof(ItemResponse.IceLevel)},
                        i.temperature AS {nameof(ItemResponse.Temperature)}
                    FROM cart.shopping_carts c
                    LEFT JOIN cart.cart_items i ON i.cart_id = c.id
                    WHERE c.id = @CartId
                 """;
            
            Dictionary<Guid, Response> cartsDictionary = [];

            await connection.QueryAsync<Response, ItemResponse?, Response>(
                sql,
                (cart, item) =>
                {
                    if (cartsDictionary.TryGetValue(cart.Id, out Response? existingCart))
                    {
                        cart = existingCart;
                    }
                    else
                    {
                        cartsDictionary.Add(cart.Id, cart);
                    }

                    if (item is not null)
                    {
                        cart.Items.Add(item);
                    }
                
                    return cart;
                },
                request,
                splitOn: nameof(ItemResponse.ItemId));
        
            if (!cartsDictionary.TryGetValue(request.CartId, out Response cartResponse))
            {
                return Result.Failure<Response>(CartErrors.NotFound(request.CartId));
            }

            return cartResponse;
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("carts/{cartId:guid}", Handler)
                .WithTags(nameof(ShoppingCart))
                .WithName(nameof(GetCart))
                .Produces<Response>();
        }

        private static async Task<IResult> Handler(ISender sender, Guid cartId)
        {
            var query = new Query(cartId);
            
            Result<Response> result = await sender.Send(query);

            return result.Match(Results.Ok, ApiResults.Problem);
        }
    }
}
