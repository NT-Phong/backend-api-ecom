using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class PageConfiguration : BaseEntityConfiguration<Page> { public override void Configure(EntityTypeBuilder<Page> b) { base.Configure(b); b.Property(x => x.Title).HasMaxLength(300).IsRequired(); b.Property(x => x.Slug).HasMaxLength(350).IsRequired(); b.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired(); b.Property(x => x.MetaTitle).HasMaxLength(255); b.Property(x => x.MetaDescription).HasMaxLength(500); CommerceConfigurationSupport.Unique(b, nameof(Page.Slug)); } }

