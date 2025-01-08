using System.Data.Common;
using Dapper;
using MediatR;
using BubbleTea.ServiceDefaults.Domain;
using BubbleTea.ServiceDefaults.Endpoints;
using BubbleTea.ServiceDefaults.Messaging;
using BubbleTea.Services.Orders.API.Entities.Orders;
using BubbleTea.Services.Orders.API.Infrastructure.Database;

namespace BubbleTea.Services.Orders.API.Features.Orders;

public static class GetOrder
{
    public sealed record Query(Guid OrderId) : IQuery<Response>;

    public sealed record Response(
        Guid Id,
        string Customer,
        string Status)
    {
        public List<ItemResponse> Items { get; init; } = [];
        public decimal TotalPrice => Items.Sum(i => (i.Price + i.Parameters.Sum(p => p.ExtraPrice)) * i.Quantity);
    }

    public sealed record ItemResponse(
        Guid ItemId,
        int Quantity,
        string ProductName,
        decimal Price,
        string Currency)
    {
        public List<ParameterResponse> Parameters { get; init; } = [];
    }
    public sealed record ParameterResponse(string Name, string Option, decimal ExtraPrice);

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
                     c.status AS {{nameof(Response.Status)}},
                     i.id AS {{nameof(ItemResponse.ItemId)}},
                     i.quantity AS {{nameof(ItemResponse.Quantity)}},
                     i.product_name AS {{nameof(ItemResponse.ProductName)}},
                     i.amount AS {{nameof(ItemResponse.Price)}},
                     i.currency AS {{nameof(ItemResponse.Currency)}},
                     p.name AS {{nameof(ParameterResponse.Name)}},
                     p.option AS {{nameof(ParameterResponse.Option)}},
                     p.extra_price AS {{nameof(ParameterResponse.ExtraPrice)}}
                 FROM ordering.orders c
                 LEFT JOIN ordering.order_items i ON i.order_id = c.id
                 LEFT JOIN ordering.order_item_parameters p ON p.order_item_id = i.id
                 WHERE c.id = @OrderId
                """;

            Dictionary<Guid, Response> ordersDictionary = [];

            await connection.QueryAsync<Response, ItemResponse?, ParameterResponse?, Response>(
                sql,
                (order, item, parameter) =>
                {
                    if (ordersDictionary.TryGetValue(order.Id, out Response? existingOrder))
                    {
                        order = existingOrder;
                    }
                    else
                    {
                        ordersDictionary.Add(order.Id, order);
                    }

                    if (item is null)
                    {
                        return order;
                    }

                    ItemResponse? existingItem = order.Items.Find(i => i.ItemId == item.ItemId);
                    
                    if (existingItem is null)
                    {
                        order.Items.Add(item);
                        existingItem = item;
                    }

                    if (parameter is not null)
                    {
                        existingItem.Parameters.Add(parameter);
                    }

                    return order;
                },
                request,
                splitOn: $"{nameof(ItemResponse.ItemId)}, {nameof(ParameterResponse.Name)}");
        
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
