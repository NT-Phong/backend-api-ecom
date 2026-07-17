using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class CustomerAddressConfiguration : BaseEntityConfiguration<CustomerAddress>
{
    public override void Configure(EntityTypeBuilder<CustomerAddress> b) { base.Configure(b); b.Property(x => x.RecipientName).HasMaxLength(200).IsRequired(); b.Property(x => x.PhoneNumber).HasMaxLength(20).IsRequired(); b.Property(x => x.AddressLine).HasMaxLength(500).IsRequired(); b.Property(x => x.PostalCode).HasMaxLength(20); b.Property(x => x.Latitude).HasPrecision(10, 7); b.Property(x => x.Longitude).HasPrecision(10, 7); b.Property(x => x.Label).HasMaxLength(50); b.Property(x => x.IsDefault).HasDefaultValue(false); CommerceConfigurationSupport.UniqueWhere(b, "\"IsDeleted\" = false AND \"IsDefault\" = true", nameof(CustomerAddress.UserId)); b.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict); b.HasOne<AdministrativeArea>().WithMany().HasForeignKey(x => x.AdministrativeAreaId).OnDelete(DeleteBehavior.SetNull); b.ToTable(t => { t.HasCheckConstraint("CK_CustomerAddress_Latitude", "\"Latitude\" BETWEEN -90 AND 90"); t.HasCheckConstraint("CK_CustomerAddress_Longitude", "\"Longitude\" BETWEEN -180 AND 180"); }); }
}
