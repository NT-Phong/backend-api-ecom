using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class CustomerProfileConfiguration : BaseEntityConfiguration<CustomerProfile>
{
    public override void Configure(EntityTypeBuilder<CustomerProfile> b) { base.Configure(b); b.Property(x => x.PreferredName).HasMaxLength(200); b.Property(x => x.Gender).HasMaxLength(20); CommerceConfigurationSupport.Unique(b, nameof(CustomerProfile.UserId)); b.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict); }
}

