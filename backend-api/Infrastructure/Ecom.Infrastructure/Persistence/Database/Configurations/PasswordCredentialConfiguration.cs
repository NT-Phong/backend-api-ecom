using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;
namespace Ecom.Infrastructure.Persistence.Database.Configurations;
public sealed class PasswordCredentialConfiguration : BaseEntityConfiguration<PasswordCredential>
{
 public override void Configure(EntityTypeBuilder<PasswordCredential> b) { base.Configure(b);
  b.Property(x=>x.PasswordHash).HasMaxLength(256).IsRequired(); b.Property(x=>x.Algorithm).HasMaxLength(32).IsRequired();
  b.Property(x=>x.AlgorithmVersion).HasMaxLength(32).HasDefaultValue("bcrypt-v1").IsRequired();
  b.HasIndex(x=>x.UserId).IsUnique(); b.HasOne<User>().WithOne().HasForeignKey<PasswordCredential>(x=>x.UserId).OnDelete(DeleteBehavior.Cascade); }
}
