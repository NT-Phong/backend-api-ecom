using Ecom.Domain.Entities;
using Ecom.Domain.Enums;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class SystemSettingConfiguration : BaseEntityConfiguration<SystemSetting> { public override void Configure(EntityTypeBuilder<SystemSetting> b) { base.Configure(b); b.Property(x => x.SettingKey).HasMaxLength(200).IsRequired(); b.Property(x => x.Value).HasColumnType("jsonb").IsRequired(); b.Property(x => x.IsPublic).HasDefaultValue(false); b.Property(x => x.Description).HasMaxLength(1000); CommerceConfigurationSupport.Unique(b, nameof(SystemSetting.SettingKey)); } }

