namespace Ecom.Domain.Entities;
public class Promotion : BaseEntity
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public PromotionType PromotionType { get; private set; }
    public decimal Value { get; private set; }
    public DateTime StartsAt { get; private set; }
    public DateTime? EndsAt { get; private set; }
    public PromotionStatus Status { get; private set; }
    public decimal? MinOrderAmount { get; private set; }

    private Promotion()
    {
    }
}