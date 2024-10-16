using System.Data.Common;
using Dapper;
using MediatR;
using Ordering.API.Entities.Orders;
using Ordering.API.Infrastructure.Database;
using ServiceDefaults.Domain;
using ServiceDefaults.Endpoints;
using ServiceDefaults.Messaging;

namespace Ordering.API.Features.Orders;

public static class GetOrder
{
    public sealed record Query(Guid OrderId) : IQuery<Response>;

    public sealed record Response(
        Guid Id,
        string Customer,
        string Status)
    {
        public List<ItemResponse> Items { get; init; } = [];
        public decimal TotalPrice => Items.Sum(i => i.Price * i.Quantity);
    }
    
    public sealed record ItemResponse(
        Guid ItemId,
        int Quantity,
        string ProductName,
        decimal Price,
        string Currency);

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
                        c.status AS {nameof(Response.Status)},
                        i.id AS {nameof(ItemResponse.ItemId)},
                        i.quantity AS {nameof(ItemResponse.Quantity)},
                        i.product_name AS {nameof(ItemResponse.ProductName)},
                        i.amount AS {nameof(ItemResponse.Price)},
                        i.currency AS {nameof(ItemResponse.Currency)}
                    FROM ordering.orders c
                    LEFT JOIN ordering.order_items i ON i.order_id = c.id
                    WHERE c.id = @OrderId
                 """;
            
            Dictionary<Guid, Response> ordersDictionary = [];

            await connection.QueryAsync<Response, ItemResponse?, Response>(
                sql,
                (order, item) =>
                {
                    if (ordersDictionary.TryGetValue(order.Id, out Response? existingCart))
                    {
                        order = existingCart;
                    }
                    else
                    {
                        ordersDictionary.Add(order.Id, order);
                    }

                    if (item is not null)
                    {
                        order.Items.Add(item);
                    }
                
                    return order;
                },
                request,
                splitOn: nameof(ItemResponse.ItemId));
        
            if (!ordersDictionary.TryGetValue(request.OrderId, out Response orderResponse))
            {
                return Result.Failure<Response>(OrderErrors.NotFound(request.OrderId));
            }

            return orderResponse;
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("orders/{orderId:guid}", Handler)
                .WithTags(nameof(Order))
                .WithName(nameof(GetOrder))
                .Produces<Response>();
        }

        private static async Task<IResult> Handler(ISender sender, Guid orderId)
        {
            var query = new Query(orderId);
            
            Result<Response> result = await sender.Send(query);

            return result.Match(Results.Ok, ApiResults.Problem);
        }
    }
}
