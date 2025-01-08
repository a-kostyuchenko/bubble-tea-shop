using System.Globalization;
using System.Text.RegularExpressions;

namespace BubbleTea.ServiceDefaults.Common;

public sealed record Slug(string Value)
{
    public static Slug AvoidCollisionsWithNumber(Slug key, string? collidingKey, IEnumerable<string> similarKeys) =>
        collidingKey is null ?
            new Slug(key) : new Slug($"{AppendNumberToSlug(key, collidingKey, similarKeys)}");

    private static string AppendNumberToSlug(Slug key, string collidingKey, IEnumerable<string> similarKeys) =>
        $"{key.Value}-{GetNonCollidingSlugSuffixNumber(collidingKey, similarKeys)}";

    private static int GetNonCollidingSlugSuffixNumber(string collidingKey, IEnumerable<string> similarKeys) => similarKeys
        .Select(similarKey => TryExtractNumber(collidingKey, similarKey) ?? 0)
        .DefaultIfEmpty(0)
        .Max() + 1;

    private static int? TryExtractNumber(string collidingKey, string similarKey) =>
        similarKey.Length > collidingKey.Length &&
        Regex.Match(similarKey[collidingKey.Length..], @"^-(?<number>\d+)$") is { Success: true } match
            ? int.Parse(match.Groups["number"].Value, CultureInfo.InvariantCulture)
            : null;
}
