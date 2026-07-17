using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class ArticleCategoryConfiguration : BaseEntityConfiguration<ArticleCategory> { public override void Configure(EntityTypeBuilder<ArticleCategory> b) { base.Configure(b); b.Property(x => x.Name).HasMaxLength(200).IsRequired(); b.Property(x => x.Slug).HasMaxLength(250).IsRequired(); b.Property(x => x.Description).HasMaxLength(1000); CommerceConfigurationSupport.Unique(b, nameof(ArticleCategory.Slug)); } }

