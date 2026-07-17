using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class StockLocationConfiguration : BaseEntityConfiguration<StockLocation>
{
    public override void Configure(EntityTypeBuilder<StockLocation> b) { base.Configure(b); b.Property(x => x.Code).HasMaxLength(50).IsRequired(); b.Property(x => x.Name).HasMaxLength(200).IsRequired(); b.Property(x => x.AddressLine).HasMaxLength(500); b.Property(x => x.IsActive).HasDefaultValue(true); CommerceConfigurationSupport.Unique(b, nameof(StockLocation.Code)); b.HasOne<AdministrativeArea>().WithMany().HasForeignKey(x => x.AdministrativeAreaId).OnDelete(DeleteBehavior.SetNull); }
}

