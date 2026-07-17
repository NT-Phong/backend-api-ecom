using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class PriceListConfiguration : BaseEntityConfiguration<PriceList>
{
    public override void Configure(EntityTypeBuilder<PriceList> b) { base.Configure(b); b.Property(x => x.Code).HasMaxLength(50).IsRequired(); b.Property(x => x.Name).HasMaxLength(200).IsRequired(); b.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired(); b.Property(x => x.Description).HasMaxLength(1000); CommerceConfigurationSupport.Unique(b, nameof(PriceList.Code)); b.ToTable(t => t.HasCheckConstraint("CK_PriceList_TimeWindow", "\"EndsAt\" IS NULL OR \"StartsAt\" IS NULL OR \"EndsAt\" > \"StartsAt\"")); }
}

