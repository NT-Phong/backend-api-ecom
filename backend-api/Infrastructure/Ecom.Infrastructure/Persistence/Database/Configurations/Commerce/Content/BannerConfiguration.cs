using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class BannerConfiguration : BaseEntityConfiguration<Banner> { public override void Configure(EntityTypeBuilder<Banner> b) { base.Configure(b); b.Property(x => x.Title).HasMaxLength(300); b.Property(x => x.AltText).HasMaxLength(500).IsRequired(); b.Property(x => x.TargetUrl).HasMaxLength(1000); b.Property(x => x.DisplayOrder).HasDefaultValue(0); b.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired(); b.HasOne<Campaign>().WithMany().HasForeignKey(x => x.CampaignId).OnDelete(DeleteBehavior.SetNull); b.HasOne<MediaAsset>().WithMany().HasForeignKey(x => x.MediaAssetId).OnDelete(DeleteBehavior.Restrict); b.ToTable(t => t.HasCheckConstraint("CK_Banner_TimeWindow", "\"EndsAt\" IS NULL OR \"StartsAt\" IS NULL OR \"EndsAt\" > \"StartsAt\"")); } }

