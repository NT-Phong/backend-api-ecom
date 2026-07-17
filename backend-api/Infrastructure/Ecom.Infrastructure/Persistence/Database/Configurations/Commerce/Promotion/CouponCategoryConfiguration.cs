using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class CouponCategoryConfiguration : BaseEntityConfiguration<CouponCategory>
{
    public override void Configure(EntityTypeBuilder<CouponCategory> b) { base.Configure(b); CommerceConfigurationSupport.Unique(b, nameof(CouponCategory.CouponId), nameof(CouponCategory.CategoryId)); b.HasOne<Coupon>().WithMany().HasForeignKey(x => x.CouponId).OnDelete(DeleteBehavior.Cascade); b.HasOne<Category>().WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict); }
}

