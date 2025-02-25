namespace EPR.RegulatorService.Frontend.Core.Models.Pagination;

public class PaginatedList<T>
{
    public List<T> items { get; set; } = new();
    public int currentPage { get; set; }
    public int totalItems { get; set; }
    public int pageSize { get; set; }

    public int TotalPages
    {
        get => pageSize == 0 ? 0 : (totalItems + pageSize - 1) / pageSize;
    }
}
