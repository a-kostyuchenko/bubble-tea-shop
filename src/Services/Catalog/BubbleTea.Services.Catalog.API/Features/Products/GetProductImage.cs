using System.Data.Common;
using Dapper;
using MediatR;
using BubbleTea.ServiceDefaults.Domain;
using BubbleTea.ServiceDefaults.Endpoints;
using BubbleTea.ServiceDefaults.Messaging;
using BubbleTea.Services.Catalog.API.Entities.Products;
using BubbleTea.Services.Catalog.API.Infrastructure.Database;
using BubbleTea.Services.Catalog.API.Infrastructure.Storage;

namespace BubbleTea.Services.Catalog.API.Features.Products;

public static class GetProductImage
{
    public sealed record Query(Guid ProductId) : IQuery<FileResponse>;

    internal sealed class QueryHandler(
        IDbConnectionFactory dbConnectionFactory,
        IBlobService blobService) : IQueryHandler<Query, FileResponse>
    {
        public async Task<Result<FileResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();
            
            const string sql = 
                $"""
                    SELECT
                        p.image_id AS ImageId
                    FROM catalog.products p
                    WHERE p.id = @ProductId
                 """;
            
            Guid? imageId = await connection.ExecuteScalarAsync<Guid?>(sql, request);
            
            if (imageId is null)
            {
                return Result.Failure<FileResponse>(ProductErrors.NotFound(request.ProductId));
            }

            Result<FileResponse> imageResult = await blobService.DownloadAsync(imageId.Value, cancellationToken);
            
            return imageResult;
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("products/{productId:guid}/image", Handler)
                .WithTags(nameof(Product))
                .WithName(nameof(GetProductImage));
        }

        private static async Task<IResult> Handler(ISender sender, Guid productId)
        {
            var query = new Query(productId);
            
            Result<FileResponse> result = await sender.Send(query);

            return result.Match(
                file => Results.File(file.Stream, file.ContentType),
                ApiResults.Problem);
        }
    }
}
