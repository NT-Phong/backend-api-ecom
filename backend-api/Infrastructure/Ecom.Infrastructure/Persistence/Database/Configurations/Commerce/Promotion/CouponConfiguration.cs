using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class CouponConfiguration : BaseEntityConfiguration<Coupon>
{
    public override void Configure(EntityTypeBuilder<Coupon> b) { base.Configure(b); b.Property(x => x.Code).HasMaxLength(50).IsRequired(); b.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired(); CommerceConfigurationSupport.Unique(b, nameof(Coupon.Code)); b.HasOne<Promotion>().WithMany().HasForeignKey(x => x.PromotionId).OnDelete(DeleteBehavior.SetNull); b.ToTable(t => { t.HasCheckConstraint("CK_Coupon_UsageLimit", "\"UsageLimit\" IS NULL OR \"UsageLimit\" >= 0"); t.HasCheckConstraint("CK_Coupon_PerUserLimit", "\"PerUserLimit\" IS NULL OR \"PerUserLimit\" >= 0"); t.HasCheckConstraint("CK_Coupon_TimeWindow", "\"EndsAt\" IS NULL OR \"EndsAt\" > \"StartsAt\""); }); }
}

