using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class ShipmentHistoryConfiguration : BaseEntityConfiguration<ShipmentHistory>
{
    public override void Configure(EntityTypeBuilder<ShipmentHistory> b) { base.Configure(b); b.Property(x => x.FromStatus).HasConversion<string>().HasMaxLength(30); b.Property(x => x.ToStatus).HasConversion<string>().HasMaxLength(30).IsRequired(); b.Property(x => x.Reason).HasMaxLength(1000); b.HasOne<Shipment>().WithMany().HasForeignKey(x => x.ShipmentId).OnDelete(DeleteBehavior.Cascade); }
}

