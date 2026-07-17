using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class ProductReviewMediaConfiguration : BaseEntityConfiguration<ProductReviewMedia> { public override void Configure(EntityTypeBuilder<ProductReviewMedia> b) { base.Configure(b); CommerceConfigurationSupport.Unique(b, nameof(ProductReviewMedia.ProductReviewId), nameof(ProductReviewMedia.MediaAssetId)); b.HasOne<ProductReview>().WithMany().HasForeignKey(x => x.ProductReviewId).OnDelete(DeleteBehavior.Cascade); b.HasOne<MediaAsset>().WithMany().HasForeignKey(x => x.MediaAssetId).OnDelete(DeleteBehavior.Restrict); } }

