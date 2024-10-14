using Catalog.API.Entities.Products;
using MediatR;
using ServiceDefaults.Domain;
using ServiceDefaults.Endpoints;
using ServiceDefaults.Messaging;

namespace Catalog.API.Features.Categories;

public static class GetCategories
{
    public sealed record Query() : IQuery<List<Response>>;
    public sealed record Response(string Name);

    internal sealed class QueryHandler() 
        : IQueryHandler<Query, List<Response>>
    {
        public Task<Result<List<Response>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var categories = Category.GetValues()
                .Select(c => new Response(c.Name))
                .ToList();

            return Task.FromResult<Result<List<Response>>>(categories);
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("categories", Handler)
                .WithTags(nameof(Category))
                .WithName(nameof(GetCategories))
                .Produces<List<Response>>();
        }

        private static async Task<IResult> Handler(ISender sender)
        {
            var query = new Query();
            
            Result<List<Response>> result = await sender.Send(query);

            return result.Match(Results.Ok, ApiResults.Problem);
        }
    }
}
