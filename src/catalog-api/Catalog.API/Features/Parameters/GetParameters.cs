using System.Data.Common;
using Catalog.API.Entities.Parameters;
using Catalog.API.Infrastructure.Database;
using Dapper;
using MediatR;
using ServiceDefaults.Domain;
using ServiceDefaults.Endpoints;
using ServiceDefaults.Messaging;

namespace Catalog.API.Features.Parameters;

public static class GetParameters
{
    public sealed record Query(string? SearchTerm, int Page, int PageSize) : IQuery<PagedResponse>;

    public sealed record Response(Guid Id, string Name)
    {
        public List<OptionResponse> Options { get; init; } = [];
    }
    public sealed record PagedResponse(
        int Page,
        int PageSize,
        int TotalCount,
        IReadOnlyCollection<Response> Parameters);
    
    public sealed record OptionResponse(Guid OptionId, string Name, double Value, decimal ExtraPrice, string Currency);


    internal sealed class QueryHandler(IDbConnectionFactory dbConnectionFactory) 
        : IQueryHandler<Query, PagedResponse>
    {
        public async Task<Result<PagedResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

            var searchParameters = new {
                SearchTerm = $"%{request.SearchTerm}%",
                Take = request.PageSize,
                Skip = (request.Page - 1) * request.PageSize
            };
            
            IReadOnlyCollection<Response> parameters = await GetParametersAsync(connection, searchParameters);
            
            int totalCount = await CountParametersAsync(connection, searchParameters);
            
            return new PagedResponse(request.Page, request.PageSize, totalCount, parameters);
        }
        
        private static async Task<IReadOnlyCollection<Response>> GetParametersAsync(
            DbConnection connection,
            object parameters)
        {
            const string sql = 
                $@"
                SELECT
                   p.id AS {nameof(Response.Id)},
                   p.name AS {nameof(Response.Name)},
                   o.id AS {nameof(OptionResponse.OptionId)},
                   o.name AS {nameof(OptionResponse.Name)},
                   o.value AS {nameof(OptionResponse.Value)},
                   o.extra_price AS {nameof(OptionResponse.ExtraPrice)},
                   o.currency AS {nameof(OptionResponse.Currency)}
                FROM catalog.parameters p
                LEFT JOIN catalog.options o ON o.parameter_id = p.id
                WHERE (@SearchTerm IS NULL OR p.name ILIKE @SearchTerm)
                ORDER BY p.id
                OFFSET @Skip
                LIMIT @Take
                ";

            Dictionary<Guid, Response> parametersDictionary = [];

            await connection.QueryAsync<Response, OptionResponse?, Response>(
                sql,
                (parameter, option) =>
                {
                    if (parametersDictionary.TryGetValue(parameter.Id, out Response? existingParameter))
                    {
                        parameter = existingParameter;
                    }
                    else
                    {
                        parametersDictionary.Add(parameter.Id, parameter);
                    }

                    if (option is not null)
                    {
                        parameter.Options.Add(option);
                    }
                
                    return parameter;
                },
                parameters,
                splitOn: nameof(OptionResponse.OptionId));

            return parametersDictionary.Values;
        }
    
        private static async Task<int> CountParametersAsync(DbConnection connection, object parameters)
        {
            const string sql =
                """
                SELECT COUNT(*)
                FROM catalog.parameters p
                WHERE (@SearchTerm IS NULL OR p.name ILIKE @SearchTerm)
                """;

            int totalCount = await connection.ExecuteScalarAsync<int>(sql, parameters);

            return totalCount;
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("parameters", Handler)
                .WithTags(nameof(Parameter))
                .WithName(nameof(GetParameters))
                .Produces<PagedResponse>();
        }

        private static async Task<IResult> Handler(
            ISender sender,
            string? searchTerm,
            int page = 1,
            int pageSize = 15)
        {
            var query = new Query(searchTerm, page, pageSize);
            
            Result<PagedResponse> result = await sender.Send(query);

            return result.Match(Results.Ok, ApiResults.Problem);
        }
    }
}
