using Microsoft.EntityFrameworkCore;

namespace Core.Pagination;

public static class PaginationExtensions
{
    public static async Task<PaginatedData<T>> ToPaginatedDataAsync<T>(
        this IQueryable<T> query,
        PaginationQueryParams pagination,
        CancellationToken cancellationToken = default
    )
    {
        int totalCount = await query.CountAsync(cancellationToken);

        List<T> data = await query
            .Skip((pagination.Page - 1) * pagination.Limit)
            .Take(pagination.Limit)
            .ToListAsync(cancellationToken);

        return new()
        {
            Page = pagination.Page,
            Limit = pagination.Limit,
            TotalCount = totalCount,
            Count = data.Count,
            Data = data,
        };
    }
}