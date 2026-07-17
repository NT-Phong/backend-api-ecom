using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class ShipmentConfiguration : BaseEntityConfiguration<Shipment>
{
    public override void Configure(EntityTypeBuilder<Shipment> b) { base.Configure(b); b.Property(x => x.ShippingMethod).HasMaxLength(100).IsRequired(); b.Property(x => x.CarrierName).HasMaxLength(100); b.Property(x => x.TrackingCode).HasMaxLength(100); b.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired(); b.HasOne<Order>().WithMany().HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Restrict); }
}

