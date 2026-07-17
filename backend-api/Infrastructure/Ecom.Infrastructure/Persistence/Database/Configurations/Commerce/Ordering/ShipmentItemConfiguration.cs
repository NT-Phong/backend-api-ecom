using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class ShipmentItemConfiguration : BaseEntityConfiguration<ShipmentItem>
{
    public override void Configure(EntityTypeBuilder<ShipmentItem> b) { base.Configure(b); CommerceConfigurationSupport.Unique(b, nameof(ShipmentItem.ShipmentId), nameof(ShipmentItem.OrderItemId)); b.HasOne<Shipment>().WithMany().HasForeignKey(x => x.ShipmentId).OnDelete(DeleteBehavior.Cascade); b.HasOne<OrderItem>().WithMany().HasForeignKey(x => x.OrderItemId).OnDelete(DeleteBehavior.Restrict); b.ToTable(t => t.HasCheckConstraint("CK_ShipmentItem_Quantity", "\"Quantity\" > 0")); }
}

