using BubbleTea.Common.Application.Slugs;
using Microsoft.EntityFrameworkCore;

namespace BubbleTea.Services.Catalog.API.Infrastructure.Database.Queries;

internal static class SlugCollisionQueries
{
    internal static async Task<(string? collidingKey, IEnumerable<string> similarKeys)> FindCollisions(
        this IQueryable<string> keys,
        Slug slugCandidate)
    {
        string? collidingSlug = await keys.FirstOrDefaultAsync(key => key == slugCandidate.Value);
        
        if (collidingSlug is null)
        {
            return (null, []);
        }

        List<string> similarKeys = await keys
            .Where(key => key.StartsWith(slugCandidate.Value) && key.Length > slugCandidate.Value.Length)
            .ToListAsync();
        
        return (collidingSlug, similarKeys);
    }
}
