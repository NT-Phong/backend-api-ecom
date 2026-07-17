using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class ArticleCategoryMapConfiguration : BaseEntityConfiguration<ArticleCategoryMap> { public override void Configure(EntityTypeBuilder<ArticleCategoryMap> b) { base.Configure(b); CommerceConfigurationSupport.Unique(b, nameof(ArticleCategoryMap.ArticleId), nameof(ArticleCategoryMap.ArticleCategoryId)); b.HasOne<Article>().WithMany().HasForeignKey(x => x.ArticleId).OnDelete(DeleteBehavior.Cascade); b.HasOne<ArticleCategory>().WithMany().HasForeignKey(x => x.ArticleCategoryId).OnDelete(DeleteBehavior.Restrict); } }

