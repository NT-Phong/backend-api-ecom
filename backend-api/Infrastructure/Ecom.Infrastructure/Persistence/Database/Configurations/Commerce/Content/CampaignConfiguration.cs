using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class CampaignConfiguration : BaseEntityConfiguration<Campaign> { public override void Configure(EntityTypeBuilder<Campaign> b) { base.Configure(b); b.Property(x => x.Code).HasMaxLength(50).IsRequired(); b.Property(x => x.Name).HasMaxLength(300).IsRequired(); b.Property(x => x.Description).HasColumnType("text"); b.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired(); CommerceConfigurationSupport.Unique(b, nameof(Campaign.Code)); b.ToTable(t => t.HasCheckConstraint("CK_Campaign_TimeWindow", "\"EndsAt\" IS NULL OR \"StartsAt\" IS NULL OR \"EndsAt\" > \"StartsAt\"")); } }

