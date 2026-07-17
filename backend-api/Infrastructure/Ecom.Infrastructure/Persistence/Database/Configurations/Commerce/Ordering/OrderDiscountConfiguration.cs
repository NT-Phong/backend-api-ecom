using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class OrderDiscountConfiguration : BaseEntityConfiguration<OrderDiscount>
{
    public override void Configure(EntityTypeBuilder<OrderDiscount> b) { base.Configure(b); b.Property(x => x.Description).HasMaxLength(500).IsRequired(); CommerceConfigurationSupport.Money(b.Property(x => x.DiscountAmount)).IsRequired(); b.HasOne<Order>().WithMany().HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade); b.HasOne<OrderItem>().WithMany().HasForeignKey(x => x.OrderItemId).OnDelete(DeleteBehavior.SetNull); b.HasOne<Promotion>().WithMany().HasForeignKey(x => x.PromotionId).OnDelete(DeleteBehavior.SetNull); b.HasOne<Coupon>().WithMany().HasForeignKey(x => x.CouponId).OnDelete(DeleteBehavior.SetNull); b.ToTable(t => t.HasCheckConstraint("CK_OrderDiscount_Amount", "\"DiscountAmount\" >= 0")); }
}

