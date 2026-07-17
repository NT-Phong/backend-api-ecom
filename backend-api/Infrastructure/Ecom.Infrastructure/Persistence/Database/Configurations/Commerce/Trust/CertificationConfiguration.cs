using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class CertificationConfiguration : BaseEntityConfiguration<Certification> { public override void Configure(EntityTypeBuilder<Certification> b) { base.Configure(b); b.Property(x => x.CertificationType).HasMaxLength(50).IsRequired(); b.Property(x => x.CertificateNumber).HasMaxLength(100).IsRequired(); b.Property(x => x.IssuingAuthority).HasMaxLength(300).IsRequired(); b.Property(x => x.VerificationStatus).HasConversion<string>().HasMaxLength(30).IsRequired(); CommerceConfigurationSupport.Unique(b, nameof(Certification.CertificationType), nameof(Certification.CertificateNumber)); b.ToTable(t => t.HasCheckConstraint("CK_Certification_TimeWindow", "\"EffectiveTo\" IS NULL OR \"EffectiveFrom\" IS NULL OR \"EffectiveTo\" >= \"EffectiveFrom\"")); } }

