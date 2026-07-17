using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class ProductCertificationConfiguration : BaseEntityConfiguration<ProductCertification> { public override void Configure(EntityTypeBuilder<ProductCertification> b) { base.Configure(b); CommerceConfigurationSupport.Unique(b, nameof(ProductCertification.ProductId), nameof(ProductCertification.CertificationId)); b.HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade); b.HasOne<Certification>().WithMany().HasForeignKey(x => x.CertificationId).OnDelete(DeleteBehavior.Restrict); } }

