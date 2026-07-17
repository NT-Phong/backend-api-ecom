using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class WishlistConfiguration : BaseEntityConfiguration<Wishlist> { public override void Configure(EntityTypeBuilder<Wishlist> b) { base.Configure(b); b.Property(x => x.Name).HasMaxLength(100).HasDefaultValue("Default").IsRequired(); CommerceConfigurationSupport.Unique(b, nameof(Wishlist.UserId), nameof(Wishlist.Name)); b.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict); } }

