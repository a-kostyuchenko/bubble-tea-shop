using System.Data.Common;
using Dapper;
using MediatR;
using BubbleTea.ServiceDefaults.Domain;
using BubbleTea.ServiceDefaults.Endpoints;
using BubbleTea.ServiceDefaults.Messaging;
using BubbleTea.Services.Catalog.API.Entities.Parameters;
using BubbleTea.Services.Catalog.API.Infrastructure.Database;

namespace BubbleTea.Services.Catalog.API.Features.Parameters;

public static class GetParameter
{
    public sealed record Query(Guid ParameterId) : IQuery<Response>;

    public sealed record Response(
        Guid Id,
        string Name)
    {
        public List<OptionResponse> Options { get; init; } = [];
    }
    
    public sealed record OptionResponse(Guid OptionId, string Name, double Value, decimal ExtraPrice, string Currency);

    internal sealed class QueryHandler(IDbConnectionFactory dbConnectionFactory) : IQueryHandler<Query, Response>
    {
        public async Task<Result<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();
            
            const string sql = 
                $"""
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
                    WHERE p.id = @ParameterId
                 """;
            
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
                request,
                splitOn: nameof(OptionResponse.OptionId));
        
            if (!parametersDictionary.TryGetValue(request.ParameterId, out Response parameterResponse))
            {
                return Result.Failure<Response>(ParameterErrors.NotFound(request.ParameterId));
            }

            return parameterResponse;
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("parameters/{parameterId:guid}", Handler)
                .WithTags(nameof(Parameter))
                .WithName(nameof(GetParameter))
                .Produces<Response>();
        }

        private static async Task<IResult> Handler(ISender sender, Guid parameterId)
        {
            var query = new Query(parameterId);
            
            Result<Response> result = await sender.Send(query);

            return result.Match(Results.Ok, ApiResults.Problem);
        }
    }
}
