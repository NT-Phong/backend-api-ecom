using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class ProducerContactConfiguration : BaseEntityConfiguration<ProducerContact>
{
    public override void Configure(EntityTypeBuilder<ProducerContact> b) { base.Configure(b); b.Property(x => x.ContactType).HasConversion<string>().HasMaxLength(30).IsRequired(); b.Property(x => x.ContactValue).HasMaxLength(500).IsRequired(); b.Property(x => x.ContactName).HasMaxLength(200); b.Property(x => x.IsPublic).HasDefaultValue(false); b.Property(x => x.DisplayOrder).HasDefaultValue(0); b.HasOne<Producer>().WithMany().HasForeignKey(x => x.ProducerId).OnDelete(DeleteBehavior.Cascade); }
}

