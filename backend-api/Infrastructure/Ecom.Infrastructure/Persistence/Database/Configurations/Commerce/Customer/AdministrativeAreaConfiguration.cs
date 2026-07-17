using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class AdministrativeAreaConfiguration : BaseEntityConfiguration<AdministrativeArea>
{
    public override void Configure(EntityTypeBuilder<AdministrativeArea> b) { base.Configure(b); b.Property(x => x.Code).HasMaxLength(30).IsRequired(); b.Property(x => x.Name).HasMaxLength(200).IsRequired(); b.Property(x => x.Level).HasConversion<string>().HasMaxLength(20).IsRequired(); b.Property(x => x.DisplayOrder).HasDefaultValue(0); b.Property(x => x.IsActive).HasDefaultValue(true); CommerceConfigurationSupport.Unique(b, nameof(AdministrativeArea.Code)); b.HasOne<AdministrativeArea>().WithMany().HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.Restrict); }
}

