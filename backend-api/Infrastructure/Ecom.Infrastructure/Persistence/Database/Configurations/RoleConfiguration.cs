using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations;

/// <summary>
/// Configuration for Role entity
/// </summary>
public class RoleConfiguration : BaseEntityConfiguration<Role>
{
    public override void Configure(EntityTypeBuilder<Role> builder)
    {
        base.Configure(builder);
        

        // Code - unique
        builder.Property(r => r.Code)
            .HasMaxLength(50)
            .IsRequired();
        
        builder.HasIndex(r => r.Code)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");
        
        // Name
        builder.Property(r => r.Name)
            .HasMaxLength(100)
            .IsRequired();
        
        // Description
        builder.Property(r => r.Description)
            .HasMaxLength(500);
        
        // Priority
        builder.Property(r => r.Priority)
            .HasDefaultValue(0);
        
        // IsActive
        builder.Property(r => r.IsActive)
            .HasDefaultValue(true);
        
        // IsSystemRole
        builder.Property(r => r.IsSystemRole)
            .HasDefaultValue(false);
    }
}

