using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class ProductVariantConfiguration : BaseEntityConfiguration<ProductVariant>
{
    public override void Configure(EntityTypeBuilder<ProductVariant> b) { base.Configure(b); b.Property(x => x.Sku).HasMaxLength(100).IsRequired(); b.Property(x => x.Name).HasMaxLength(300).IsRequired(); b.Property(x => x.Barcode).HasMaxLength(100); b.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired(); b.Property(x => x.InventoryMode).HasConversion<string>().HasMaxLength(20).IsRequired(); b.Property(x => x.AllowBackorder).HasDefaultValue(false); b.Property(x => x.WeightGrams).HasPrecision(12, 3); b.Property(x => x.DisplayOrder).HasDefaultValue(0); CommerceConfigurationSupport.Unique(b, nameof(ProductVariant.Sku)); b.HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict); b.ToTable(t => t.HasCheckConstraint("CK_ProductVariant_WeightGrams", "\"WeightGrams\" IS NULL OR \"WeightGrams\" >= 0")); }
}

