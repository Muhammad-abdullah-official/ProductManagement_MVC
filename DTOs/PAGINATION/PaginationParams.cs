namespace ProductManagement.DTOs.PAGINATION
{
    public record PaginationParams(
        int Page = 1,
        int PageSize = 10,
        string? Search = null,
        string? SortBy = null,
        bool Ascending = true
    );
}
