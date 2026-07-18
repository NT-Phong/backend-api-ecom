using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;
namespace Ecom.Infrastructure.Persistence.Database.Configurations;
public sealed class SecurityEventConfiguration : BaseEntityConfiguration<SecurityEvent>
{
 public override void Configure(EntityTypeBuilder<SecurityEvent> b) { base.Configure(b);
  b.Property(x=>x.EventType).HasMaxLength(100).IsRequired(); b.Property(x=>x.RiskLevel).HasConversion<int>();
  b.Property(x=>x.IpFingerprint).HasMaxLength(128).IsRequired(); b.Property(x=>x.UserAgentSummary).HasMaxLength(300).IsRequired(); b.Property(x=>x.Metadata).HasColumnType("jsonb");
  b.HasIndex(x=>new{x.UserId,x.OccurredAt}); b.HasIndex(x=>x.EventType); b.HasOne<User>().WithMany().HasForeignKey(x=>x.UserId).OnDelete(DeleteBehavior.SetNull);
  b.HasOne<UserSession>().WithMany().HasForeignKey(x=>x.SessionId).OnDelete(DeleteBehavior.SetNull); }
}
