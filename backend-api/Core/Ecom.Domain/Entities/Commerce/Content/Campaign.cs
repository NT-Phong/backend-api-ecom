namespace Ecom.Domain.Entities;
public class Campaign : BaseEntity
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTime? StartsAt { get; private set; }
    public DateTime? EndsAt { get; private set; }
    public ContentStatus Status { get; private set; }

    private Campaign()
    {
    }
}