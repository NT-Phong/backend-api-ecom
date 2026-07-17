using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class InventoryLevelConfiguration : BaseEntityConfiguration<InventoryLevel>
{
    public override void Configure(EntityTypeBuilder<InventoryLevel> b) { base.Configure(b); CommerceConfigurationSupport.Quantity(b.Property(x => x.StockedQuantity)).HasDefaultValue(0m); CommerceConfigurationSupport.Quantity(b.Property(x => x.ReservedQuantity)).HasDefaultValue(0m); CommerceConfigurationSupport.Quantity(b.Property(x => x.IncomingQuantity)).HasDefaultValue(0m); CommerceConfigurationSupport.Unique(b, nameof(InventoryLevel.InventoryItemId), nameof(InventoryLevel.StockLocationId)); b.HasOne<InventoryItem>().WithMany().HasForeignKey(x => x.InventoryItemId).OnDelete(DeleteBehavior.Cascade); b.HasOne<StockLocation>().WithMany().HasForeignKey(x => x.StockLocationId).OnDelete(DeleteBehavior.Restrict); b.ToTable(t => t.HasCheckConstraint("CK_InventoryLevel_Quantities", "\"StockedQuantity\" >= 0 AND \"ReservedQuantity\" >= 0 AND \"IncomingQuantity\" >= 0 AND \"ReservedQuantity\" <= \"StockedQuantity\"")); }
}

