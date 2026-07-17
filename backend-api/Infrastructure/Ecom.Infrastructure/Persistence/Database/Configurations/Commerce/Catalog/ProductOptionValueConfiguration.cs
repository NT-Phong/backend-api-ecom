using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class ProductOptionValueConfiguration : BaseEntityConfiguration<ProductOptionValue>
{
    public override void Configure(EntityTypeBuilder<ProductOptionValue> b) { base.Configure(b); b.Property(x => x.Value).HasMaxLength(150).IsRequired(); b.Property(x => x.DisplayOrder).HasDefaultValue(0); CommerceConfigurationSupport.Unique(b, nameof(ProductOptionValue.ProductOptionId), nameof(ProductOptionValue.Value)); b.HasOne<ProductOption>().WithMany().HasForeignKey(x => x.ProductOptionId).OnDelete(DeleteBehavior.Cascade); }
}

