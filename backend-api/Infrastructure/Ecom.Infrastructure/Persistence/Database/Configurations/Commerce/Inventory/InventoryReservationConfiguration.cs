using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class InventoryReservationConfiguration : BaseEntityConfiguration<InventoryReservation>
{
    public override void Configure(EntityTypeBuilder<InventoryReservation> b) { base.Configure(b); CommerceConfigurationSupport.Quantity(b.Property(x => x.Quantity)).IsRequired(); b.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired(); b.HasIndex(x => new { x.InventoryItemId, x.StockLocationId, x.Status, x.ExpiresAt }); b.HasOne<InventoryItem>().WithMany().HasForeignKey(x => x.InventoryItemId).OnDelete(DeleteBehavior.Restrict); b.HasOne<StockLocation>().WithMany().HasForeignKey(x => x.StockLocationId).OnDelete(DeleteBehavior.Restrict); b.HasOne<OrderItem>().WithMany().HasForeignKey(x => x.OrderItemId).OnDelete(DeleteBehavior.Restrict); b.ToTable(t => t.HasCheckConstraint("CK_InventoryReservation_Quantity", "\"Quantity\" > 0")); }
}
