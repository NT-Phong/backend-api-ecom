using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class TraceEventConfiguration : BaseEntityConfiguration<TraceEvent> { public override void Configure(EntityTypeBuilder<TraceEvent> b) { base.Configure(b); b.Property(x => x.EventType).HasMaxLength(50).IsRequired(); b.Property(x => x.LocationText).HasMaxLength(500); b.Property(x => x.Description).HasColumnType("text"); b.HasOne<TraceLot>().WithMany().HasForeignKey(x => x.TraceLotId).OnDelete(DeleteBehavior.Cascade); } }

