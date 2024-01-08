namespace EPR.RegulatorService.Frontend.Core.Models.Pagination;

public class PaginatedList<T>
{
    public List<T> Items { get; set; } = new();
    public int CurrentPage { get; set; }
    public int TotalItems { get; set; }
    public int PageSize { get; set; }

    public int TotalPages
    {
        get => (TotalItems + PageSize - 1) / PageSize;
    }
}
