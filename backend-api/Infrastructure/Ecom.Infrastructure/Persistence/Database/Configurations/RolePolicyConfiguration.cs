using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations;

/// <summary>
/// Configuration for RolePolicy entity (many-to-many between Role and Policy)
/// </summary>
public class RolePolicyConfiguration : BaseEntityConfiguration<RolePolicy>
{
    public override void Configure(EntityTypeBuilder<RolePolicy> builder)
    {
        base.Configure(builder);
        

        // Unique constraint: 1 Role không thể có Policy trùng
        builder.HasIndex(rp => new { rp.RoleId, rp.PolicyId })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");
        
        // Relationship with Role
        builder.HasOne(rp => rp.Role)
            .WithMany(r => r.RolePolicies)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Relationship with Policy
        builder.HasOne(rp => rp.Policy)
            .WithMany(p => p.RolePolicies)
            .HasForeignKey(rp => rp.PolicyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

