using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class FacilityCertificationConfiguration : BaseEntityConfiguration<FacilityCertification> { public override void Configure(EntityTypeBuilder<FacilityCertification> b) { base.Configure(b); CommerceConfigurationSupport.Unique(b, nameof(FacilityCertification.ProductionFacilityId), nameof(FacilityCertification.CertificationId)); b.HasOne<ProductionFacility>().WithMany().HasForeignKey(x => x.ProductionFacilityId).OnDelete(DeleteBehavior.Cascade); b.HasOne<Certification>().WithMany().HasForeignKey(x => x.CertificationId).OnDelete(DeleteBehavior.Restrict); } }

