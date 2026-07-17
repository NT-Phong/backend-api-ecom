using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class InventoryItemConfiguration : BaseEntityConfiguration<InventoryItem>
{
    public override void Configure(EntityTypeBuilder<InventoryItem> b) { base.Configure(b); b.Property(x => x.RequiresShipping).HasDefaultValue(true); CommerceConfigurationSupport.Unique(b, nameof(InventoryItem.ProductVariantId)); b.HasOne<ProductVariant>().WithMany().HasForeignKey(x => x.ProductVariantId).OnDelete(DeleteBehavior.Restrict); }
}

