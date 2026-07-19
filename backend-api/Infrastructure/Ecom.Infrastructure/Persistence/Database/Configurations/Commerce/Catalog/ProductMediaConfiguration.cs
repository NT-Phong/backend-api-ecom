using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class ProductMediaConfiguration : BaseEntityConfiguration<ProductMedia>
{
    public override void Configure(EntityTypeBuilder<ProductMedia> b) { base.Configure(b); b.Property(x => x.DisplayOrder).HasDefaultValue(0); b.Property(x => x.IsPrimary).HasDefaultValue(false); b.Property(x => x.Caption).HasMaxLength(500); CommerceConfigurationSupport.Unique(b, nameof(ProductMedia.ProductId), nameof(ProductMedia.MediaAssetId)); CommerceConfigurationSupport.UniqueWhere(b, "\"IsDeleted\" = false AND \"IsPrimary\" = true", nameof(ProductMedia.ProductId)); b.HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade); b.HasOne<MediaAsset>().WithMany().HasForeignKey(x => x.MediaAssetId).OnDelete(DeleteBehavior.Restrict); }
}
