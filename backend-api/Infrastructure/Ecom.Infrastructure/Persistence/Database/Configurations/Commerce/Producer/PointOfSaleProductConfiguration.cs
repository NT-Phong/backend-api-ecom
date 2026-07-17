using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class PointOfSaleProductConfiguration : BaseEntityConfiguration<PointOfSaleProduct>
{
    public override void Configure(EntityTypeBuilder<PointOfSaleProduct> b) { base.Configure(b); b.Property(x => x.IsAvailable).HasDefaultValue(true); CommerceConfigurationSupport.Unique(b, nameof(PointOfSaleProduct.PointOfSaleId), nameof(PointOfSaleProduct.ProductId)); b.HasOne<PointOfSale>().WithMany().HasForeignKey(x => x.PointOfSaleId).OnDelete(DeleteBehavior.Cascade); b.HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict); }
}

