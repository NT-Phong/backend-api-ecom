using Ecom.Domain.Common.Interfaces;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Base;

/// <summary>
/// Base configuration for all entities that inherit from BaseEntity.
/// Apply this configuration to all entity configurations.
/// </summary>
public abstract class BaseEntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : BaseEntity
{
    protected virtual string? TableName => null;
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.ToTable(TableName ?? "Tbl_" + typeof(TEntity).Name);
        // Primary key
        if (typeof(IEntity).IsAssignableFrom(typeof(TEntity)))
        {
            builder.HasKey(nameof(IEntity.Id));
            builder.Property(nameof(IEntity.Id))
                .ValueGeneratedNever();
        }

        // Auto-increment No - sử dụng IDENTITY cho PostgreSQL
        builder.Property(e => e.No)
            .ValueGeneratedOnAdd()
            .UseIdentityByDefaultColumn();

        // Audit fields - Creation
        builder.Property(e => e.CreatedBy);

        builder.Property(e => e.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        // Audit fields - Modification
        builder.Property(e => e.UpdatedBy);

        builder.Property(e => e.UpdatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired(false);

        // Soft delete fields
        builder.Property(e => e.DeletedBy);

        builder.Property(e => e.DeletedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired(false);

        builder.Property(e => e.IsDeleted)
            .HasDefaultValue(false)
            .IsRequired();

        // Concurrency
        builder.Property(e => e.ConcurrencyStamp)
            .IsConcurrencyToken();

        // Global query filter for soft delete
        builder.HasQueryFilter(e => !e.IsDeleted);

        // Indexes
        builder.HasIndex(e => e.No);
        builder.HasIndex(e => e.CreatedAt);
        builder.HasIndex(e => e.IsDeleted);
    }
}


