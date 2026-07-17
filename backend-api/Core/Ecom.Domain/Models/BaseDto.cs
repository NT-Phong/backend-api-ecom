
using System.Text.Json.Serialization;

namespace Ecom.Domain.Models;

public abstract class BaseQueryDto
{
    private const int MaxPageCount = 50;

    private int _page = 1;

    private int _pageCount = MaxPageCount;
    public Guid? Id { get; set; }
    public DateTime? CreatedAt { get; set; }
    public virtual DateTime? CreateAtFrom { get; set; }
    public virtual DateTime? CreateAtTo { get; set; }

    public int Page
    {
        get => _page;
        set => _page = value < 0 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageCount;
        set => _pageCount = value > MaxPageCount || value < 0 ? MaxPageCount : value;
    }

    public string OrderBy { get; set; } = "CreatedAt desc";

    public int Skip()
    {
        return PageSize * (Page - 1);
    }
}

public class BaseDto
{
    public Guid Id { get; set; }
    public ulong No { get; set; }
    public Guid? CreatorId { get; set; }
    public Guid? EditorId { get; set; }
    [JsonIgnore] public Guid CreatorBy { get; set; }
    [JsonIgnore] public Guid EditorBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime EditedAt { get; set; }
    public AccountCreator? Creator { get; set; } 
    public AccountCreator? Editor { get; set; }
}

public class AccountCreator
{
    public Guid Id { get; set; }
    public required string Fullname { get; set; }
}
