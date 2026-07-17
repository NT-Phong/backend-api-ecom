using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class ProducerCertificationConfiguration : BaseEntityConfiguration<ProducerCertification> { public override void Configure(EntityTypeBuilder<ProducerCertification> b) { base.Configure(b); CommerceConfigurationSupport.Unique(b, nameof(ProducerCertification.ProducerId), nameof(ProducerCertification.CertificationId)); b.HasOne<Producer>().WithMany().HasForeignKey(x => x.ProducerId).OnDelete(DeleteBehavior.Cascade); b.HasOne<Certification>().WithMany().HasForeignKey(x => x.CertificationId).OnDelete(DeleteBehavior.Restrict); } }

