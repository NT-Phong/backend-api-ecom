using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class NavigationItemConfiguration : BaseEntityConfiguration<NavigationItem> { public override void Configure(EntityTypeBuilder<NavigationItem> b) { base.Configure(b); b.Property(x => x.Label).HasMaxLength(200).IsRequired(); b.Property(x => x.TargetUrl).HasMaxLength(1000); b.Property(x => x.DisplayOrder).HasDefaultValue(0); b.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired(); b.HasOne<NavigationItem>().WithMany().HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.Restrict); b.HasOne<Page>().WithMany().HasForeignKey(x => x.PageId).OnDelete(DeleteBehavior.SetNull); } }

