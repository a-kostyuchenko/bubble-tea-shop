using System.Data.Common;
using Dapper;
using MediatR;
using Ordering.API.Entities.Orders;
using Ordering.API.Infrastructure.Database;
using ServiceDefaults.Domain;
using ServiceDefaults.Endpoints;
using ServiceDefaults.Messaging;

namespace Ordering.API.Features.Orders;

public static class GetOrders
{
    public sealed record Query(string? Status, int Page, int PageSize) : IQuery<PagedResponse>;

    public sealed record Response(
        Guid Id,
        string Customer,
        string Status)
    {
        public List<ItemResponse> Items { get; init; } = [];
    }
    
    public sealed record ItemResponse(
        Guid ItemId,
        int Quantity,
        string ProductName,
        decimal Price,
        string Currency);
    
    public sealed record PagedResponse(
        int Page,
        int PageSize,
        int TotalCount,
        IReadOnlyCollection<Response> Orders);

    internal sealed class QueryHandler(IDbConnectionFactory dbConnectionFactory) : IQueryHandler<Query, PagedResponse>
    {
        public async Task<Result<PagedResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();
            
            var parameters = new
            {
                request.Status,
                Take = request.PageSize,
                Skip = (request.Page - 1) * request.PageSize
            };
            
            IReadOnlyCollection<Response> orders = await GetOrdersAsync(connection, parameters);
            
            int totalCount = await CountOrdersAsync(connection, parameters);
            
            return new PagedResponse(request.Page, request.PageSize, totalCount, orders);
        }
        
        private static async Task<IReadOnlyCollection<Response>> GetOrdersAsync(
            DbConnection connection,
            object parameters)
        {
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
                    WHERE (@Status IS NULL OR c.status = @Status)
                    ORDER BY c.id
                    OFFSET @Skip
                    LIMIT @Take
                 """;
            
            Dictionary<Guid, Response> ordersDictionary = [];

            List<Response> orders =  (await connection.QueryAsync<Response, ItemResponse?, Response>(
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
                parameters,
                splitOn: nameof(ItemResponse.ItemId))).AsList();

            return orders;
        }
        
        private static async Task<int> CountOrdersAsync(DbConnection connection, object parameters)
        {
            const string sql =
                """
                SELECT COUNT(*)
                FROM ordering.orders c
                WHERE (@Status IS NULL OR c.status = @Status)
                """;

            int totalCount = await connection.ExecuteScalarAsync<int>(sql, parameters);

            return totalCount;
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("orders", Handler)
                .WithTags(nameof(Order))
                .WithName(nameof(GetOrders))
                .Produces<PagedResponse>();
        }

        private static async Task<IResult> Handler(ISender sender, string? status, int page = 1, int pageSize = 15)
        {
            var query = new Query(status, page, pageSize);
            
            Result<PagedResponse> result = await sender.Send(query);

            return result.Match(Results.Ok, ApiResults.Problem);
        }
    }
}
