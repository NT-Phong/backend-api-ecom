using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations;

/// <summary>
/// Configuration for Policy entity
/// </summary>
public class PolicyConfiguration : BaseEntityConfiguration<Policy>
{
    public override void Configure(EntityTypeBuilder<Policy> builder)
    {
        base.Configure(builder);
        

        // Code - unique
        builder.Property(p => p.Code)
            .HasMaxLength(100)
            .IsRequired();
        
        builder.HasIndex(p => p.Code)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");
        
        // Name
        builder.Property(p => p.Name)
            .HasMaxLength(200)
            .IsRequired();
        
        // Description
        builder.Property(p => p.Description)
            .HasMaxLength(500);
        
        // Module
        builder.Property(p => p.Module)
            .HasMaxLength(100);
        
        builder.HasIndex(p => p.Module);
        
        // IsActive
        builder.Property(p => p.IsActive)
            .HasDefaultValue(true);
        
        // IsSystemPolicy
        builder.Property(p => p.IsSystemPolicy)
            .HasDefaultValue(false);
    }
}

