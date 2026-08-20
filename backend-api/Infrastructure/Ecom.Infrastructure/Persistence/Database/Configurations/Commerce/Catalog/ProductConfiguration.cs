using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class ProductConfiguration : BaseEntityConfiguration<Product>
{
    public override void Configure(EntityTypeBuilder<Product> b) { base.Configure(b); b.Property(x => x.Name).HasMaxLength(300).IsRequired(); b.Property(x => x.Slug).HasMaxLength(350).IsRequired(); b.Property(x => x.ShortDescription).HasMaxLength(1000); b.Property(x => x.Description).HasColumnType("text"); b.Property(x => x.UsageInstructions).HasColumnType("text"); b.Property(x => x.StorageInstructions).HasColumnType("text"); b.Property(x => x.WarningText).HasColumnType("text"); b.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired(); b.Property(x => x.MetaTitle).HasMaxLength(255); b.Property(x => x.MetaDescription).HasMaxLength(500); b.Property(x => x.BrandName).HasMaxLength(200); CommerceConfigurationSupport.Unique(b, nameof(Product.Slug)); b.HasOne<Producer>().WithMany().HasForeignKey(x => x.ProducerId).OnDelete(DeleteBehavior.Restrict); }
}
