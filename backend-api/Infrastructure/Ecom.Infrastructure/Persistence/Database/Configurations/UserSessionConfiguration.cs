using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;
namespace Ecom.Infrastructure.Persistence.Database.Configurations;
public sealed class UserSessionConfiguration : BaseEntityConfiguration<UserSession>
{
 public override void Configure(EntityTypeBuilder<UserSession> b) { base.Configure(b);
  b.Property(x=>x.ClientType).HasConversion<int>(); b.Property(x=>x.AuthenticationMethod).HasConversion<int>(); b.Property(x=>x.AuthenticationStrength).HasConversion<int>();
  b.Property(x=>x.SecurityStamp).HasMaxLength(64).IsRequired(); b.Property(x=>x.DeviceId).HasMaxLength(200); b.Property(x=>x.RevocationReason).HasMaxLength(200);
  b.HasIndex(x=>new{x.UserId,x.RevokedAt}); b.HasIndex(x=>x.AbsoluteExpiresAt);
  b.HasOne<User>().WithMany().HasForeignKey(x=>x.UserId).OnDelete(DeleteBehavior.Cascade); }
}
