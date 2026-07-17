using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class PageSectionConfiguration : BaseEntityConfiguration<PageSection> { public override void Configure(EntityTypeBuilder<PageSection> b) { base.Configure(b); b.Property(x => x.SectionType).HasMaxLength(50).IsRequired(); b.Property(x => x.Title).HasMaxLength(300); b.Property(x => x.Content).HasColumnType("jsonb"); b.Property(x => x.DisplayOrder).HasDefaultValue(0); b.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired(); b.HasOne<Page>().WithMany().HasForeignKey(x => x.PageId).OnDelete(DeleteBehavior.Cascade); } }

