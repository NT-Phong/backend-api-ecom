using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class ProductVariantOptionValueConfiguration : BaseEntityConfiguration<ProductVariantOptionValue>
{
    public override void Configure(EntityTypeBuilder<ProductVariantOptionValue> b) { base.Configure(b); CommerceConfigurationSupport.Unique(b, nameof(ProductVariantOptionValue.ProductVariantId), nameof(ProductVariantOptionValue.ProductOptionValueId)); b.HasOne<ProductVariant>().WithMany().HasForeignKey(x => x.ProductVariantId).OnDelete(DeleteBehavior.Cascade); b.HasOne<ProductOptionValue>().WithMany().HasForeignKey(x => x.ProductOptionValueId).OnDelete(DeleteBehavior.Restrict); }
}

