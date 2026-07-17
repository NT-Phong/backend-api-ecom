using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class SeoRedirectConfiguration : BaseEntityConfiguration<SeoRedirect> { public override void Configure(EntityTypeBuilder<SeoRedirect> b) { base.Configure(b); b.Property(x => x.SourcePath).HasMaxLength(1000).IsRequired(); b.Property(x => x.TargetPath).HasMaxLength(1000).IsRequired(); b.Property(x => x.StatusCode).HasDefaultValue(301); b.Property(x => x.IsActive).HasDefaultValue(true); CommerceConfigurationSupport.Unique(b, nameof(SeoRedirect.SourcePath)); } }

