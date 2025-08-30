namespace Core.Pagination;

public class PaginatedData<T>
{
    public int Page { get; set; }
    public int Limit { get; set; }
    public int Count { get; set; }
    public int TotalCount { get; set; }
    public List<T> Data { get; set; } = [];
    public int MaxPages => (int)Math.Ceiling((double)TotalCount / Limit);
    public bool HasMore => Page < MaxPages;

    public static PaginatedData<T> CreateFrom<TSource>(PaginatedData<TSource> paginatedData, List<T> data)
    {
        return new()
        {
            Page = paginatedData.Page,
            Limit = paginatedData.Limit,
            Count = paginatedData.Count,
            TotalCount = paginatedData.TotalCount,
            Data = data
        };
    }
}