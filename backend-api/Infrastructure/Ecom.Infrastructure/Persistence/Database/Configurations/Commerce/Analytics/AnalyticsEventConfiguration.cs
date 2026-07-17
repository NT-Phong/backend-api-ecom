using Ecom.Domain.Entities;
using Ecom.Domain.Enums;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class AnalyticsEventConfiguration : BaseEntityConfiguration<AnalyticsEvent> { public override void Configure(EntityTypeBuilder<AnalyticsEvent> b) { base.Configure(b); b.Property(x => x.EventType).HasConversion<string>().HasMaxLength(50).IsRequired(); b.Property(x => x.Path).HasMaxLength(1000); b.Property(x => x.SearchTerm).HasMaxLength(300); b.HasOne<VisitorSession>().WithMany().HasForeignKey(x => x.VisitorSessionId).OnDelete(DeleteBehavior.SetNull); b.HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.SetNull); b.HasOne<Campaign>().WithMany().HasForeignKey(x => x.CampaignId).OnDelete(DeleteBehavior.SetNull); } }

