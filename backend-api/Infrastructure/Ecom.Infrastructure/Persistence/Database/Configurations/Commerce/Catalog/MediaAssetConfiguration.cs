using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class MediaAssetConfiguration : BaseEntityConfiguration<MediaAsset>
{
    public override void Configure(EntityTypeBuilder<MediaAsset> b) { base.Configure(b); b.Property(x => x.StorageKey).HasMaxLength(1000).IsRequired(); b.Property(x => x.OriginalFileName).HasMaxLength(500).IsRequired(); b.Property(x => x.ContentType).HasMaxLength(100).IsRequired(); b.Property(x => x.MediaType).HasConversion<string>().HasMaxLength(30).IsRequired(); b.Property(x => x.AltText).HasMaxLength(500); b.Property(x => x.Visibility).HasConversion<string>().HasMaxLength(20).IsRequired(); b.Property(x => x.ScanStatus).HasConversion<string>().HasMaxLength(30).IsRequired(); CommerceConfigurationSupport.Unique(b, nameof(MediaAsset.StorageKey)); b.ToTable(t => t.HasCheckConstraint("CK_MediaAsset_SizeBytes", "\"SizeBytes\" >= 0")); }
}

