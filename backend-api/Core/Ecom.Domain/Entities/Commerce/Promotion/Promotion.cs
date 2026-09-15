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

    public static Promotion Create(
        string code,
        string name,
        PromotionType promotionType,
        decimal value,
        DateTime startsAt,
        DateTime? endsAt,
        decimal? minOrderAmount,
        PromotionStatus status = PromotionStatus.Active)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new CommerceDomainException("PROMOTION_CODE_REQUIRED", "Promotion code is required.");
        if (string.IsNullOrWhiteSpace(name))
            throw new CommerceDomainException("PROMOTION_NAME_REQUIRED", "Promotion name is required.");
        if (value < 0)
            throw new CommerceDomainException("PROMOTION_VALUE_INVALID", "Promotion value cannot be negative.");
        if (promotionType == PromotionType.Percentage && value > 100)
            throw new CommerceDomainException("PROMOTION_PERCENTAGE_INVALID", "Percentage discount cannot exceed 100%.");
        if (endsAt.HasValue && endsAt.Value <= startsAt)
            throw new CommerceDomainException("PROMOTION_TIME_WINDOW_INVALID", "Promotion end time must be after start time.");
        if (minOrderAmount.HasValue && minOrderAmount.Value < 0)
            throw new CommerceDomainException("PROMOTION_MIN_ORDER_INVALID", "Minimum order amount cannot be negative.");

        return new Promotion
        {
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            PromotionType = promotionType,
            Value = value,
            StartsAt = startsAt,
            EndsAt = endsAt,
            MinOrderAmount = minOrderAmount,
            Status = status
        };
    }

    public void Update(
        string name,
        PromotionType promotionType,
        decimal value,
        DateTime startsAt,
        DateTime? endsAt,
        decimal? minOrderAmount)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new CommerceDomainException("PROMOTION_NAME_REQUIRED", "Promotion name is required.");
        if (value < 0)
            throw new CommerceDomainException("PROMOTION_VALUE_INVALID", "Promotion value cannot be negative.");
        if (promotionType == PromotionType.Percentage && value > 100)
            throw new CommerceDomainException("PROMOTION_PERCENTAGE_INVALID", "Percentage discount cannot exceed 100%.");
        if (endsAt.HasValue && endsAt.Value <= startsAt)
            throw new CommerceDomainException("PROMOTION_TIME_WINDOW_INVALID", "Promotion end time must be after start time.");
        if (minOrderAmount.HasValue && minOrderAmount.Value < 0)
            throw new CommerceDomainException("PROMOTION_MIN_ORDER_INVALID", "Minimum order amount cannot be negative.");

        Name = name.Trim();
        PromotionType = promotionType;
        Value = value;
        StartsAt = startsAt;
        EndsAt = endsAt;
        MinOrderAmount = minOrderAmount;
    }

    public void Activate() => Status = PromotionStatus.Active;
    public void Pause() => Status = PromotionStatus.Paused;
    public void Expire() => Status = PromotionStatus.Expired;

    public bool IsActiveAt(DateTime now)
    {
        if (Status != PromotionStatus.Active) return false;
        if (now < StartsAt) return false;
        if (EndsAt.HasValue && now > EndsAt.Value) return false;
        return true;
    }

    public decimal CalculateDiscount(decimal subtotal, decimal shippingFee)
    {
        if (MinOrderAmount.HasValue && subtotal < MinOrderAmount.Value)
            return 0;

        return PromotionType switch
        {
            PromotionType.Percentage => Math.Round(subtotal * (Value / 100m), 0, MidpointRounding.AwayFromZero),
            PromotionType.FixedAmount => Math.Min(Value, subtotal),
            PromotionType.FreeShipping => Math.Min(Value > 0 ? Value : shippingFee, shippingFee),
            _ => 0
        };
    }
}