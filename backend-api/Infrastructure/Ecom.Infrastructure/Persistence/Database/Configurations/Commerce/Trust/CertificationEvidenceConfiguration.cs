using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class CertificationEvidenceConfiguration : BaseEntityConfiguration<CertificationEvidence> { public override void Configure(EntityTypeBuilder<CertificationEvidence> b) { base.Configure(b); b.Property(x => x.EvidenceType).HasConversion<string>().HasMaxLength(30).IsRequired(); CommerceConfigurationSupport.Unique(b, nameof(CertificationEvidence.CertificationId), nameof(CertificationEvidence.MediaAssetId)); b.HasOne<Certification>().WithMany().HasForeignKey(x => x.CertificationId).OnDelete(DeleteBehavior.Cascade); b.HasOne<MediaAsset>().WithMany().HasForeignKey(x => x.MediaAssetId).OnDelete(DeleteBehavior.Restrict); } }

