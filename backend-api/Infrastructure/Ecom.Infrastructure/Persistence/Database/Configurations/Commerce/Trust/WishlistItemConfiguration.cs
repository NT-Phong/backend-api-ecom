using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class WishlistItemConfiguration : BaseEntityConfiguration<WishlistItem> { public override void Configure(EntityTypeBuilder<WishlistItem> b) { base.Configure(b); CommerceConfigurationSupport.Unique(b, nameof(WishlistItem.WishlistId), nameof(WishlistItem.ProductId)); b.HasOne<Wishlist>().WithMany().HasForeignKey(x => x.WishlistId).OnDelete(DeleteBehavior.Cascade); b.HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict); } }

