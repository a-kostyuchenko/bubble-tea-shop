namespace ServiceDefaults.Common;

public sealed record PagedResponse<T>(
    int Page,
    int PageSize,
    int TotalCount,
    IReadOnlyCollection<T> Data);
