namespace Ecom.Domain.Entities;
public class PriceList : BaseEntity
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public PriceListStatus Status { get; private set; }
    public DateTime? StartsAt { get; private set; }
    public DateTime? EndsAt { get; private set; }
    public string? Description { get; private set; }

    private PriceList()
    {
    }
}