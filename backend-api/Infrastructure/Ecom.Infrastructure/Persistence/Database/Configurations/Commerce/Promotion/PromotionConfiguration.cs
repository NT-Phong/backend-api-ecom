using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class PromotionConfiguration : BaseEntityConfiguration<Promotion>
{
    public override void Configure(EntityTypeBuilder<Promotion> b) { base.Configure(b); b.Property(x => x.Code).HasMaxLength(50).IsRequired(); b.Property(x => x.Name).HasMaxLength(300).IsRequired(); b.Property(x => x.PromotionType).HasConversion<string>().HasMaxLength(30).IsRequired(); CommerceConfigurationSupport.Money(b.Property(x => x.Value)).IsRequired(); b.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired(); CommerceConfigurationSupport.Money(b.Property(x => x.MinOrderAmount)); CommerceConfigurationSupport.Unique(b, nameof(Promotion.Code)); b.ToTable(t => { t.HasCheckConstraint("CK_Promotion_Value", "\"Value\" >= 0"); t.HasCheckConstraint("CK_Promotion_MinOrder", "\"MinOrderAmount\" IS NULL OR \"MinOrderAmount\" >= 0"); t.HasCheckConstraint("CK_Promotion_TimeWindow", "\"EndsAt\" IS NULL OR \"EndsAt\" > \"StartsAt\""); }); }
}

