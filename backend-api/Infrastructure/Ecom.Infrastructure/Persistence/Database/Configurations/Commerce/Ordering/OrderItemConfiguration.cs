using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class OrderItemConfiguration : BaseEntityConfiguration<OrderItem>
{
    public override void Configure(EntityTypeBuilder<OrderItem> b) { base.Configure(b); b.Property(x => x.ProductNameSnapshot).HasMaxLength(300).IsRequired(); b.Property(x => x.VariantNameSnapshot).HasMaxLength(300).IsRequired(); b.Property(x => x.SkuSnapshot).HasMaxLength(100).IsRequired(); CommerceConfigurationSupport.Money(b.Property(x => x.UnitPriceSnapshot)).IsRequired(); CommerceConfigurationSupport.Money(b.Property(x => x.DiscountAmountSnapshot)).HasDefaultValue(0m); CommerceConfigurationSupport.Money(b.Property(x => x.LineTotalAmount)).IsRequired(); b.HasOne<Order>().WithMany().HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade); b.HasOne<ProductVariant>().WithMany().HasForeignKey(x => x.ProductVariantId).OnDelete(DeleteBehavior.SetNull); b.ToTable(t => { t.HasCheckConstraint("CK_OrderItem_Quantity", "\"Quantity\" > 0"); t.HasCheckConstraint("CK_OrderItem_Amounts", "\"UnitPriceSnapshot\" >= 0 AND \"DiscountAmountSnapshot\" >= 0 AND \"LineTotalAmount\" >= 0"); }); }
}

