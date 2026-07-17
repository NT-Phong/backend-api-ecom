using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class CategoryConfiguration : BaseEntityConfiguration<Category>
{
    public override void Configure(EntityTypeBuilder<Category> b) { base.Configure(b); b.Property(x => x.Name).HasMaxLength(200).IsRequired(); b.Property(x => x.Slug).HasMaxLength(250).IsRequired(); b.Property(x => x.Description).HasColumnType("text"); b.Property(x => x.DisplayOrder).HasDefaultValue(0); b.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired(); CommerceConfigurationSupport.Unique(b, nameof(Category.Slug)); b.HasOne<Category>().WithMany().HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.Restrict); }
}

