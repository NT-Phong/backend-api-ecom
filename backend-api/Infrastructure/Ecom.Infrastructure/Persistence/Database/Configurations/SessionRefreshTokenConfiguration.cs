using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;
namespace Ecom.Infrastructure.Persistence.Database.Configurations;
public sealed class SessionRefreshTokenConfiguration : BaseEntityConfiguration<SessionRefreshToken>
{
 public override void Configure(EntityTypeBuilder<SessionRefreshToken> b) { base.Configure(b);
  b.Property(x=>x.TokenHash).HasMaxLength(128).IsRequired(); b.HasIndex(x=>x.TokenHash).IsUnique(); b.HasIndex(x=>x.FamilyId);
  b.HasOne<UserSession>().WithMany().HasForeignKey(x=>x.SessionId).OnDelete(DeleteBehavior.Cascade);
  b.HasOne<SessionRefreshToken>().WithOne().HasForeignKey<SessionRefreshToken>(x=>x.ReplacedByTokenId).OnDelete(DeleteBehavior.Restrict); }
}
