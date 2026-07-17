using Ecom.Domain.Entities;
using Ecom.Domain.Enums;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class VisitorSessionConfiguration : BaseEntityConfiguration<VisitorSession> { public override void Configure(EntityTypeBuilder<VisitorSession> b) { base.Configure(b); b.Property(x => x.SessionHash).HasMaxLength(255).IsRequired(); b.Property(x => x.Source).HasMaxLength(100); b.Property(x => x.Medium).HasMaxLength(100); b.Property(x => x.Campaign).HasMaxLength(200); b.Property(x => x.ConsentStatus).HasConversion<string>().HasMaxLength(30).IsRequired(); CommerceConfigurationSupport.Unique(b, nameof(VisitorSession.SessionHash)); b.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull); b.ToTable(t => t.HasCheckConstraint("CK_VisitorSession_TimeWindow", "\"EndedAt\" IS NULL OR \"EndedAt\" >= \"StartedAt\"")); } }

