using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations;
public sealed class VerificationChallengeConfiguration : BaseEntityConfiguration<VerificationChallenge>
{
    public override void Configure(EntityTypeBuilder<VerificationChallenge> b)
    {
        base.Configure(b); b.Property(x=>x.Purpose).HasConversion<int>(); b.Property(x=>x.Status).HasConversion<int>();
        b.Property(x=>x.DestinationHash).HasMaxLength(128).IsRequired(); b.Property(x=>x.SecretHash).HasMaxLength(256).IsRequired();
        b.Property(x=>x.CreatedByIpHash).HasMaxLength(128).IsRequired();
        b.HasIndex(x=>new{x.DestinationHash,x.Purpose,x.Status}); b.HasIndex(x=>x.ExpiresAt);
        b.HasOne<User>().WithMany().HasForeignKey(x=>x.UserId).OnDelete(DeleteBehavior.SetNull);
    }
}
