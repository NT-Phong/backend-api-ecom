using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations;

/// <summary>
/// Configuration for OtpToken entity
/// </summary>
public class OtpTokenConfiguration : BaseEntityConfiguration<OtpToken>
{
    public override void Configure(EntityTypeBuilder<OtpToken> builder)
    {
        base.Configure(builder);
        

        // Code (OTP)
        builder.Property(o => o.Code)
            .HasMaxLength(10)
            .IsRequired();
        
        // OtpTokenType - enum stored as integer
        builder.Property(o => o.OtpTokenType)
            .HasConversion<int>()
            .IsRequired();
        
        // ExpiredAt
        builder.Property(o => o.ExpiredAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();
        
        // UsedAt
        builder.Property(o => o.UsedAt)
            .HasColumnType("timestamp with time zone");
        
        // PhoneNumber
        builder.Property(o => o.PhoneNumber)
            .HasMaxLength(20);
        
        // Email
        builder.Property(o => o.Email)
            .HasMaxLength(255);
        
        // IP addresses
        builder.Property(o => o.CreatedByIp)
            .HasMaxLength(45);
        
        builder.Property(o => o.VerifiedByIp)
            .HasMaxLength(45);
        
        // Relationship with User
        builder.HasOne(o => o.User)
            .WithMany(u => u.OtpTokens)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Indexes
        builder.HasIndex(o => o.UserId);
        builder.HasIndex(o => o.OtpTokenType);
        builder.HasIndex(o => o.ExpiredAt);
        builder.HasIndex(o => new { o.UserId, o.OtpTokenType, o.IsUsed });
    }
}

