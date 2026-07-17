using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class ProducerConfiguration : BaseEntityConfiguration<Producer>
{
    public override void Configure(EntityTypeBuilder<Producer> b) { base.Configure(b); b.Property(x => x.Code).HasMaxLength(50).IsRequired(); b.Property(x => x.Name).HasMaxLength(300).IsRequired(); b.Property(x => x.LegalName).HasMaxLength(300); b.Property(x => x.Description).HasColumnType("text"); b.Property(x => x.WebsiteUrl).HasMaxLength(500); b.Property(x => x.PublicStatus).HasConversion<string>().HasMaxLength(30).IsRequired(); b.Property(x => x.IsVerified).HasDefaultValue(false); CommerceConfigurationSupport.Unique(b, nameof(Producer.Code)); }
}

