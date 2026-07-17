using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations;

/// <summary>
/// Configuration for JwtRefreshToken entity
/// </summary>
public class JwtRefreshTokenConfiguration : BaseEntityConfiguration<JwtRefreshToken>
{
    public override void Configure(EntityTypeBuilder<JwtRefreshToken> builder)
    {
        base.Configure(builder);
        
        // Token - unique
        builder.Property(t => t.Token)
            .HasMaxLength(500)
            .IsRequired();
        
        builder.HasIndex(t => t.Token)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");
        
        // ExpiresAt
        builder.Property(t => t.ExpiresAt)
            .IsRequired();
        
        // Status - enum stored as integer
        builder.Property(t => t.Status)
            .HasConversion<int>()
            .IsRequired();
        
        // IP and User Agent
        builder.Property(t => t.CreatedByIp)
            .HasMaxLength(45);
        
        builder.Property(t => t.CreatedByUserAgent)
            .HasMaxLength(500);
        
        builder.Property(t => t.RevokedByIp)
            .HasMaxLength(45);
        
        builder.Property(t => t.RevokedReason)
            .HasMaxLength(500);
        
        // Relationship with User
        builder.HasOne(t => t.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Indexes
        builder.HasIndex(t => t.UserId);
        builder.HasIndex(t => t.ExpiresAt);
        builder.HasIndex(t => t.Status);
    }
}

