using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class CouponProductConfiguration : BaseEntityConfiguration<CouponProduct>
{
    public override void Configure(EntityTypeBuilder<CouponProduct> b) { base.Configure(b); CommerceConfigurationSupport.Unique(b, nameof(CouponProduct.CouponId), nameof(CouponProduct.ProductId)); b.HasOne<Coupon>().WithMany().HasForeignKey(x => x.CouponId).OnDelete(DeleteBehavior.Cascade); b.HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict); }
}

