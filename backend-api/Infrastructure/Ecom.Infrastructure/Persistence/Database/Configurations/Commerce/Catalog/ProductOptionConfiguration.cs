using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class ProductOptionConfiguration : BaseEntityConfiguration<ProductOption>
{
    public override void Configure(EntityTypeBuilder<ProductOption> b) { base.Configure(b); b.Property(x => x.Code).HasMaxLength(50).IsRequired(); b.Property(x => x.Name).HasMaxLength(100).IsRequired(); b.Property(x => x.DisplayOrder).HasDefaultValue(0); CommerceConfigurationSupport.Unique(b, nameof(ProductOption.ProductId), nameof(ProductOption.Code)); b.HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade); }
}

