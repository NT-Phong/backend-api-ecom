using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class TraceEventEvidenceConfiguration : BaseEntityConfiguration<TraceEventEvidence> { public override void Configure(EntityTypeBuilder<TraceEventEvidence> b) { base.Configure(b); CommerceConfigurationSupport.Unique(b, nameof(TraceEventEvidence.TraceEventId), nameof(TraceEventEvidence.MediaAssetId)); b.HasOne<TraceEvent>().WithMany().HasForeignKey(x => x.TraceEventId).OnDelete(DeleteBehavior.Cascade); b.HasOne<MediaAsset>().WithMany().HasForeignKey(x => x.MediaAssetId).OnDelete(DeleteBehavior.Restrict); } }

