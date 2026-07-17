using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class ProductQuestionConfiguration : BaseEntityConfiguration<ProductQuestion> { public override void Configure(EntityTypeBuilder<ProductQuestion> b) { base.Configure(b); b.Property(x => x.GuestName).HasMaxLength(200); b.Property(x => x.GuestEmail).HasMaxLength(255); b.Property(x => x.Content).HasColumnType("text").IsRequired(); b.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired(); b.HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict); b.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull); } }

