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
        string Customer,
        decimal TotalPrice)
    {
        public List<ItemResponse> Items { get; init; } = [];
    }

    public sealed record ItemResponse(
        Guid ItemId,
        Guid ProductId,
        int Quantity,
        string ProductName,
        decimal Price,
        string Currency)
    {
        public List<ParameterResponse> Parameters { get; init; } = [];
    }

    public sealed record ParameterResponse(
        Guid ParameterId,
        string Name)
    {
        public OptionResponse SelectedOption { get; init; }
    }

    public sealed record OptionResponse(
        Guid OptionId,
        string Name,
        double Value,
        decimal ExtraPrice,
        string Currency);

    internal sealed class QueryHandler(IDbConnectionFactory dbConnectionFactory) : IQueryHandler<Query, Response>
    {
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();
            
            const string sql = 
                $$"""
                     SELECT
                         c.id AS {{nameof(Response.Id)}},
                         c.customer AS {{nameof(Response.Customer)}},
                         SUM(i.amount * i.quantity + COALESCE(o.extra_price, 0)) OVER (PARTITION BY c.id) AS {{nameof(Response.TotalPrice)}},
                         i.id AS {{nameof(ItemResponse.ItemId)}},
                         i.product_id AS {{nameof(ItemResponse.ProductId)}},
                         i.quantity AS {{nameof(ItemResponse.Quantity)}},
                         i.product_name AS {{nameof(ItemResponse.ProductName)}},
                         i.amount AS {{nameof(ItemResponse.Price)}},
                         i.currency AS {{nameof(ItemResponse.Currency)}},
                         p.id AS {{nameof(ParameterResponse.ParameterId)}},
                         p.name AS {{nameof(ParameterResponse.Name)}},
                         o.id AS {{nameof(OptionResponse.OptionId)}},
                         o.name AS {{nameof(OptionResponse.Name)}},
                         o.value AS {{nameof(OptionResponse.Value)}},
                         o.extra_price AS {{nameof(OptionResponse.ExtraPrice)}},
                         o.currency AS {{nameof(OptionResponse.Currency)}}
                     FROM cart.shopping_carts c
                     LEFT JOIN cart.cart_items i ON i.cart_id = c.id
                     LEFT JOIN cart.cart_item_parameters p ON p.cart_item_id = i.id
                     LEFT JOIN cart.cart_item_options o ON o.parameter_id = p.id
                     WHERE c.id = @CartId
                  """;
            
            Dictionary<Guid, Response> cartsDictionary = [];

            await connection.QueryAsync<Response, ItemResponse?, ParameterResponse?, OptionResponse?, Response>(
                sql,
                (cart, item, parameter, option) =>
                {
                    if (cartsDictionary.TryGetValue(cart.Id, out Response? existingCart))
                    {
                        cart = existingCart;
                    }
                    else
                    {
                        cartsDictionary.Add(cart.Id, cart);
                    }

                    if (item is null)
                    {
                        return cart;
                    }

                    if (parameter is not null && option is not null)
                    {
                        item.Parameters.Add(parameter with { SelectedOption = option });
                    }
                        
                    cart.Items.Add(item);

                    return cart;
                },
                request,
                splitOn: $"{nameof(ItemResponse.ItemId)},{nameof(ParameterResponse.ParameterId)},{nameof(OptionResponse.OptionId)}");
        
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
