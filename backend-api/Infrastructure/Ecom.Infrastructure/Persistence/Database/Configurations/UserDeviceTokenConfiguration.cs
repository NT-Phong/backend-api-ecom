using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecom.Infrastructure.Persistence.Database.Configurations;

public class UserDeviceTokenConfiguration : BaseEntityConfiguration<UserDeviceToken>
{
    public override void Configure(EntityTypeBuilder<UserDeviceToken> builder)
    {
        base.Configure(builder);

        builder.Property(t => t.FcmToken).HasMaxLength(4096).IsRequired();
        builder.Property(t => t.Platform).HasMaxLength(20).IsRequired();

        builder.HasOne(t => t.User)
            .WithMany(u => u.DeviceTokens)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index để lookup nhanh token theo UserId
        builder.HasIndex(t => t.UserId);
        
        // Index để check duplicate token
        builder.HasIndex(t => t.FcmToken).HasFilter("\"IsDeleted\" = false");
    }
}

