using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class IdempotencyRecordConfiguration : BaseEntityConfiguration<IdempotencyRecord>
{
    public override void Configure(EntityTypeBuilder<IdempotencyRecord> b)
    {
        base.Configure(b);
        b.Property(x => x.Operation).HasMaxLength(100).IsRequired();
        b.Property(x => x.OwnerScope).HasMaxLength(255).IsRequired();
        b.Property(x => x.KeyHash).HasMaxLength(64).IsRequired();
        b.Property(x => x.RequestFingerprint).HasMaxLength(64).IsRequired();
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
        b.HasIndex(x => x.ExpiresAt);
        CommerceConfigurationSupport.Unique(b, nameof(IdempotencyRecord.Operation), nameof(IdempotencyRecord.OwnerScope), nameof(IdempotencyRecord.KeyHash));
    }
}
