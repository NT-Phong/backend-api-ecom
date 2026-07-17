using Ecom.Domain.Entities;
using Ecom.Domain.Enums;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class AuditLogConfiguration : BaseEntityConfiguration<AuditLog> { public override void Configure(EntityTypeBuilder<AuditLog> b) { base.Configure(b); b.Property(x => x.Action).HasMaxLength(100).IsRequired(); b.Property(x => x.EntityName).HasMaxLength(200).IsRequired(); b.Property(x => x.BeforeData).HasColumnType("jsonb"); b.Property(x => x.AfterData).HasColumnType("jsonb"); b.Property(x => x.IpAddress).HasMaxLength(45); } }

