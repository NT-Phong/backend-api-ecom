namespace Ecom.Domain.Models;

public class FindResult<T>
{
    public IEnumerable<T> Items { get; set; }

    public int TotalCount { get; private set; }

    public static FindResult<T> Success(IEnumerable<T> items, long totalCount)
    {
        return new FindResult<T> { Items = items, TotalCount = (int)totalCount };
    }

    public static FindResult<T> Empty()
    {
        return new FindResult<T> { Items = Enumerable.Empty<T>(), TotalCount = 0 };
    }
}
