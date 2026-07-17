using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class ProductCategoryConfiguration : BaseEntityConfiguration<ProductCategory>
{
    public override void Configure(EntityTypeBuilder<ProductCategory> b) { base.Configure(b); b.Property(x => x.IsPrimary).HasDefaultValue(false); CommerceConfigurationSupport.Unique(b, nameof(ProductCategory.ProductId), nameof(ProductCategory.CategoryId)); CommerceConfigurationSupport.UniqueWhere(b, "\"IsDeleted\" = false AND \"IsPrimary\" = true", nameof(ProductCategory.ProductId)); b.HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade); b.HasOne<Category>().WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict); }
}
