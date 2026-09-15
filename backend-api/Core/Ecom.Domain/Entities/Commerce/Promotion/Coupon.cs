namespace Ecom.Domain.Entities;
public class Coupon : BaseEntity
{
    public Guid? PromotionId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public int? UsageLimit { get; private set; }
    public int? PerUserLimit { get; private set; }
    public DateTime StartsAt { get; private set; }
    public DateTime? EndsAt { get; private set; }
    public CouponStatus Status { get; private set; }

    public static Coupon Create(
        string code,
        Guid? promotionId,
        int? usageLimit,
        int? perUserLimit,
        DateTime startsAt,
        DateTime? endsAt,
        CouponStatus status = CouponStatus.Active)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new CommerceDomainException("COUPON_CODE_REQUIRED", "Coupon code is required.");
        if (usageLimit.HasValue && usageLimit.Value < 0)
            throw new CommerceDomainException("COUPON_USAGE_LIMIT_INVALID", "Usage limit cannot be negative.");
        if (perUserLimit.HasValue && perUserLimit.Value < 0)
            throw new CommerceDomainException("COUPON_PER_USER_LIMIT_INVALID", "Per-user limit cannot be negative.");
        if (endsAt.HasValue && endsAt.Value <= startsAt)
            throw new CommerceDomainException("COUPON_TIME_WINDOW_INVALID", "Coupon end time must be after start time.");

        return new Coupon
        {
            Code = code.Trim().ToUpperInvariant(),
            PromotionId = promotionId,
            UsageLimit = usageLimit,
            PerUserLimit = perUserLimit,
            StartsAt = startsAt,
            EndsAt = endsAt,
            Status = status
        };
    }

    public void Update(
        Guid? promotionId,
        int? usageLimit,
        int? perUserLimit,
        DateTime startsAt,
        DateTime? endsAt)
    {
        if (usageLimit.HasValue && usageLimit.Value < 0)
            throw new CommerceDomainException("COUPON_USAGE_LIMIT_INVALID", "Usage limit cannot be negative.");
        if (perUserLimit.HasValue && perUserLimit.Value < 0)
            throw new CommerceDomainException("COUPON_PER_USER_LIMIT_INVALID", "Per-user limit cannot be negative.");
        if (endsAt.HasValue && endsAt.Value <= startsAt)
            throw new CommerceDomainException("COUPON_TIME_WINDOW_INVALID", "Coupon end time must be after start time.");

        PromotionId = promotionId;
        UsageLimit = usageLimit;
        PerUserLimit = perUserLimit;
        StartsAt = startsAt;
        EndsAt = endsAt;
    }

    public void Activate() => Status = CouponStatus.Active;
    public void Pause() => Status = CouponStatus.Paused;
    public void Expire() => Status = CouponStatus.Expired;

    public bool IsEligible(DateTime now)
    {
        if (Status != CouponStatus.Active) return false;
        if (now < StartsAt) return false;
        if (EndsAt.HasValue && now > EndsAt.Value) return false;
        return true;
    }
}