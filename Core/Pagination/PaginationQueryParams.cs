namespace Core.Pagination;

public class PaginationQueryParams
{
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 20;
}