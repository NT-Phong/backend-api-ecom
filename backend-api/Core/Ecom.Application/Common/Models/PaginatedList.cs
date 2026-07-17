namespace Ecom.Application.Common.Models;

public class PaginatedList<T>
{
    public IReadOnlyCollection<T> Items { get; }
    public int PageNumber { get; }
    public int TotalPages { get; }
    public int TotalCount { get; }
    public int PageSize { get; }

    public PaginatedList(IEnumerable<T>? items, int count, int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        TotalCount = count;
        PageSize = pageSize;
        Items = (items ?? [])
                 .ToList()
                 .AsReadOnly();
    }

    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public static PaginatedList<T> Create(
           IEnumerable<T>? source,
           int totalCount,
           int pageNumber,
           int pageSize)
    {
        return new PaginatedList<T>(source, totalCount, pageNumber, pageSize);
    }
} 

