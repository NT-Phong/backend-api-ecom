using Ecom.Application.Common.Commerce;
using Ecom.Application.Common.Interfaces;
using Ecom.Infrastructure.Persistence.Database;
using Microsoft.EntityFrameworkCore;

namespace Ecom.Infrastructure.Services;

/// <summary>
/// PostgreSQL serialization point for coupon usage limits. A coupon row is locked before its
/// promotion and redemption counts are evaluated, so every redemption of one code observes a
/// committed count from the preceding transaction.
/// </summary>
public sealed class CouponRedemptionStore(ApplicationDbContext db) : ICouponRedemptionStore
{
    public async Task<LockedCouponPromotion?> LockCouponAndPromotionAsync(string normalizedCouponCode,
        CancellationToken cancellationToken)
    {
        var coupon = await db.Coupons.FromSqlInterpolated($@"
SELECT * FROM ""Tbl_Coupon""
WHERE ""Code"" = {normalizedCouponCode}
  AND ""IsDeleted"" = false
FOR UPDATE").SingleOrDefaultAsync(cancellationToken);

        if (coupon is null)
            return null;

        if (!coupon.PromotionId.HasValue)
            return new LockedCouponPromotion(coupon, null);

        var promotion = await db.Promotions.FromSqlInterpolated($@"
SELECT * FROM ""Tbl_Promotion""
WHERE ""Id"" = {coupon.PromotionId.Value}
  AND ""IsDeleted"" = false
FOR UPDATE").SingleOrDefaultAsync(cancellationToken);

        return new LockedCouponPromotion(coupon, promotion);
    }
}
