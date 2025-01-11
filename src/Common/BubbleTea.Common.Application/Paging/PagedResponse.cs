namespace BubbleTea.Common.Application.Paging;

public sealed record PagedResponse<T>(
    int Page,
    int PageSize,
    int TotalCount,
    IReadOnlyCollection<T> Data);
