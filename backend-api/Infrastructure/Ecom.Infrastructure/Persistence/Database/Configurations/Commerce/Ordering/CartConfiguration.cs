using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class CartConfiguration : BaseEntityConfiguration<Cart>
{
    public override void Configure(EntityTypeBuilder<Cart> b) { base.Configure(b); b.Property(x => x.GuestTokenHash).HasMaxLength(255); b.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired(); b.Property(x => x.CurrencyCode).HasMaxLength(CommerceConstants.CurrencyCodeLength).IsFixedLength().HasDefaultValue(CommerceConstants.DefaultCurrency).IsRequired(); CommerceConfigurationSupport.UniqueWhere(b, "\"IsDeleted\" = false AND \"Status\" = 'Active' AND \"UserId\" IS NOT NULL", nameof(Cart.UserId)); CommerceConfigurationSupport.UniqueWhere(b, "\"IsDeleted\" = false AND \"Status\" = 'Active' AND \"GuestTokenHash\" IS NOT NULL", nameof(Cart.GuestTokenHash)); b.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull); b.ToTable(t => t.HasCheckConstraint("CK_Cart_Owner", "(\"UserId\" IS NULL) <> (\"GuestTokenHash\" IS NULL)")); }
}
