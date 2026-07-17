using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class InventoryMovementConfiguration : BaseEntityConfiguration<InventoryMovement>
{
    public override void Configure(EntityTypeBuilder<InventoryMovement> b) { base.Configure(b); CommerceConfigurationSupport.Quantity(b.Property(x => x.QuantityDelta)).IsRequired(); b.Property(x => x.MovementType).HasConversion<string>().HasMaxLength(30).IsRequired(); b.Property(x => x.Reason).HasMaxLength(1000); b.HasOne<InventoryItem>().WithMany().HasForeignKey(x => x.InventoryItemId).OnDelete(DeleteBehavior.Restrict); b.HasOne<StockLocation>().WithMany().HasForeignKey(x => x.StockLocationId).OnDelete(DeleteBehavior.Restrict); b.HasOne<OrderItem>().WithMany().HasForeignKey(x => x.OrderItemId).OnDelete(DeleteBehavior.SetNull); b.ToTable(t => t.HasCheckConstraint("CK_InventoryMovement_QuantityDelta", "\"QuantityDelta\" <> 0")); }
}

