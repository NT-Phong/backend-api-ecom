using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class NewsletterSubscriptionConfiguration : BaseEntityConfiguration<NewsletterSubscription> { public override void Configure(EntityTypeBuilder<NewsletterSubscription> b) { base.Configure(b); b.Property(x => x.Email).HasMaxLength(255).IsRequired(); b.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired(); b.Property(x => x.Source).HasMaxLength(100); b.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull); } }

