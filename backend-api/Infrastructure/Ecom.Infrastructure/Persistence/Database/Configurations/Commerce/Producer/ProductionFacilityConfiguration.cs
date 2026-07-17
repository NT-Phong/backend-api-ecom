using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class ProductionFacilityConfiguration : BaseEntityConfiguration<ProductionFacility>
{
    public override void Configure(EntityTypeBuilder<ProductionFacility> b) { base.Configure(b); b.Property(x => x.Name).HasMaxLength(300).IsRequired(); b.Property(x => x.AddressLine).HasMaxLength(500); b.Property(x => x.Latitude).HasPrecision(10, 7); b.Property(x => x.Longitude).HasPrecision(10, 7); b.Property(x => x.PublicStatus).HasConversion<string>().HasMaxLength(30).IsRequired(); b.Property(x => x.Description).HasColumnType("text"); b.HasOne<Producer>().WithMany().HasForeignKey(x => x.ProducerId).OnDelete(DeleteBehavior.Restrict); b.HasOne<AdministrativeArea>().WithMany().HasForeignKey(x => x.AdministrativeAreaId).OnDelete(DeleteBehavior.SetNull); b.ToTable(t => { t.HasCheckConstraint("CK_ProductionFacility_Latitude", "\"Latitude\" BETWEEN -90 AND 90"); t.HasCheckConstraint("CK_ProductionFacility_Longitude", "\"Longitude\" BETWEEN -180 AND 180"); }); }
}

