using Ecom.Domain.Entities;
using Ecom.Domain.Enums;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class NotificationConfiguration : BaseEntityConfiguration<Notification> { public override void Configure(EntityTypeBuilder<Notification> b) { base.Configure(b); b.Property(x => x.NotificationType).HasMaxLength(50).IsRequired(); b.Property(x => x.Title).HasMaxLength(300).IsRequired(); b.Property(x => x.Body).HasColumnType("text").IsRequired(); b.Property(x => x.Data).HasColumnType("jsonb"); b.Property(x => x.CreatedBySystem).HasDefaultValue(false); } }

