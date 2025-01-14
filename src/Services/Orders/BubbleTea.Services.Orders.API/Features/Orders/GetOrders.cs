using System.Data.Common;
using BubbleTea.Common.Application.Data;
using BubbleTea.Common.Application.Messaging;
using BubbleTea.Common.Application.Paging;
using BubbleTea.Common.Domain;
using BubbleTea.Common.Presentation.Endpoints;
using Dapper;
using MediatR;
using BubbleTea.Services.Orders.API.Entities.Orders;

namespace BubbleTea.Services.Orders.API.Features.Orders;

public static class GetOrders
{
    public sealed record Query(string? Status, int Page, int PageSize) : IQuery<PagedResponse<Response>>;

    public sealed record Response(
        Guid Id,
        string Customer,
        string Status,
        long TotalItems);

    internal sealed class QueryHandler(IDbConnectionFactory dbConnectionFactory) : IQueryHandler<Query, PagedResponse<Response>>
    {
        public async Task<Result<PagedResponse<Response>>> Handle(Query request, CancellationToken cancellationToken)
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
            
            return new PagedResponse<Response>(request.Page, request.PageSize, totalCount, orders);
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
                        COUNT(i.id) OVER (PARTITION BY c.id) AS {nameof(Response.TotalItems)}
                    FROM ordering.orders c
                    LEFT JOIN ordering.order_items i ON i.order_id = c.id
                    WHERE (@Status IS NULL OR c.status = @Status)
                    ORDER BY c.id
                    OFFSET @Skip
                    LIMIT @Take
                 """;
            
            List<Response> orders =  (await connection.QueryAsync<Response>(sql, parameters)).AsList();

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
                .Produces<PagedResponse<Response>>();
        }

        private static async Task<IResult> Handler(ISender sender, string? status, int page = 1, int pageSize = 15)
        {
            var query = new Query(status, page, pageSize);
            
            Result<PagedResponse<Response>> result = await sender.Send(query);

            return result.Match(Results.Ok, ApiResults.Problem);
        }
    }
}
