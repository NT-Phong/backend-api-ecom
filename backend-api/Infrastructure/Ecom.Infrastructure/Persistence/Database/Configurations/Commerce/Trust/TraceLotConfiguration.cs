using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class TraceLotConfiguration : BaseEntityConfiguration<TraceLot> { public override void Configure(EntityTypeBuilder<TraceLot> b) { base.Configure(b); b.Property(x => x.LotCode).HasMaxLength(100).IsRequired(); b.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired(); CommerceConfigurationSupport.Unique(b, nameof(TraceLot.LotCode)); b.HasOne<TraceProfile>().WithMany().HasForeignKey(x => x.TraceProfileId).OnDelete(DeleteBehavior.Cascade); b.ToTable(t => t.HasCheckConstraint("CK_TraceLot_TimeWindow", "\"ExpiresAt\" IS NULL OR \"ProducedAt\" IS NULL OR \"ExpiresAt\" >= \"ProducedAt\"")); } }

