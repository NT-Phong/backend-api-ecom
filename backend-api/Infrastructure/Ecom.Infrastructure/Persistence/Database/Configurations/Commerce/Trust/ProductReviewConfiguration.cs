using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class ProductReviewConfiguration : BaseEntityConfiguration<ProductReview> { public override void Configure(EntityTypeBuilder<ProductReview> b) { base.Configure(b); b.Property(x => x.Title).HasMaxLength(300); b.Property(x => x.Content).HasColumnType("text"); b.Property(x => x.ModerationStatus).HasConversion<string>().HasMaxLength(30).IsRequired(); b.HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict); b.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict); b.HasOne<OrderItem>().WithMany().HasForeignKey(x => x.OrderItemId).OnDelete(DeleteBehavior.SetNull); b.ToTable(t => t.HasCheckConstraint("CK_ProductReview_Rating", "\"Rating\" BETWEEN 1 AND 5")); } }

