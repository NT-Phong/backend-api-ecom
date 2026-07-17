using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class CouponRedemptionConfiguration : BaseEntityConfiguration<CouponRedemption>
{
    public override void Configure(EntityTypeBuilder<CouponRedemption> b) { base.Configure(b); CommerceConfigurationSupport.Money(b.Property(x => x.DiscountAmount)).IsRequired(); CommerceConfigurationSupport.Unique(b, nameof(CouponRedemption.CouponId), nameof(CouponRedemption.OrderId)); b.HasOne<Coupon>().WithMany().HasForeignKey(x => x.CouponId).OnDelete(DeleteBehavior.Restrict); b.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull); b.HasOne<Order>().WithMany().HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Restrict); b.ToTable(t => t.HasCheckConstraint("CK_CouponRedemption_DiscountAmount", "\"DiscountAmount\" >= 0")); }
}

