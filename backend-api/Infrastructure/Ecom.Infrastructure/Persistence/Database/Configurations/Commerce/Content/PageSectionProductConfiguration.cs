using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class PageSectionProductConfiguration : BaseEntityConfiguration<PageSectionProduct> { public override void Configure(EntityTypeBuilder<PageSectionProduct> b) { base.Configure(b); b.Property(x => x.DisplayOrder).HasDefaultValue(0); CommerceConfigurationSupport.Unique(b, nameof(PageSectionProduct.PageSectionId), nameof(PageSectionProduct.ProductId)); b.HasOne<PageSection>().WithMany().HasForeignKey(x => x.PageSectionId).OnDelete(DeleteBehavior.Cascade); b.HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict); } }

