using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class ProductAnswerConfiguration : BaseEntityConfiguration<ProductAnswer> { public override void Configure(EntityTypeBuilder<ProductAnswer> b) { base.Configure(b); b.Property(x => x.Content).HasColumnType("text").IsRequired(); b.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired(); b.HasOne<ProductQuestion>().WithMany().HasForeignKey(x => x.ProductQuestionId).OnDelete(DeleteBehavior.Cascade); b.HasOne<User>().WithMany().HasForeignKey(x => x.AnsweredByUserId).OnDelete(DeleteBehavior.SetNull); } }

