using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations;

/// <summary>
/// Configuration for UserPolicy entity (grant/revoke specific policies for a user)
/// </summary>
public class UserPolicyConfiguration : BaseEntityConfiguration<UserPolicy>
{
    public override void Configure(EntityTypeBuilder<UserPolicy> builder)
    {
        base.Configure(builder);
        

        // Unique constraint: 1 User không thể có Policy trùng
        builder.HasIndex(up => new { up.UserId, up.PolicyId })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");
        
        // IsGranted
        builder.Property(up => up.IsGranted)
            .HasDefaultValue(true);
        
        // Reason
        builder.Property(up => up.Reason)
            .HasMaxLength(500);
        
        // ExpiresAt
        builder.Property(up => up.ExpiresAt)
            .HasColumnType("timestamp with time zone");
        
        // Relationship with User
        builder.HasOne(up => up.User)
            .WithMany(u => u.UserPolicies)
            .HasForeignKey(up => up.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Relationship with Policy
        builder.HasOne(up => up.Policy)
            .WithMany(p => p.UserPolicies)
            .HasForeignKey(up => up.PolicyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

