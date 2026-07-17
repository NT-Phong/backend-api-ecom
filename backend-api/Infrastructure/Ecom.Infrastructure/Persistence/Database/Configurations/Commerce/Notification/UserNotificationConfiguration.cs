using Ecom.Domain.Entities;
using Ecom.Domain.Enums;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class UserNotificationConfiguration : BaseEntityConfiguration<UserNotification> { public override void Configure(EntityTypeBuilder<UserNotification> b) { base.Configure(b); b.Property(x => x.DeliveryStatus).HasConversion<string>().HasMaxLength(30).IsRequired(); CommerceConfigurationSupport.Unique(b, nameof(UserNotification.NotificationId), nameof(UserNotification.UserId)); b.HasOne<Notification>().WithMany().HasForeignKey(x => x.NotificationId).OnDelete(DeleteBehavior.Cascade); b.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict); } }

