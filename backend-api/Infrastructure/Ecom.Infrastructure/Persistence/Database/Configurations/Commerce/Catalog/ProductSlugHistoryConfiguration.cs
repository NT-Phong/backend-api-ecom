using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class ProductSlugHistoryConfiguration : BaseEntityConfiguration<ProductSlugHistory>
{
    public override void Configure(EntityTypeBuilder<ProductSlugHistory> b) { base.Configure(b); b.Property(x => x.Slug).HasMaxLength(350).IsRequired(); b.Property(x => x.RedirectStatusCode).HasDefaultValue(301); CommerceConfigurationSupport.Unique(b, nameof(ProductSlugHistory.Slug)); b.HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade); }
}

