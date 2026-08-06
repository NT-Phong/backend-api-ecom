using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class PaymentConfiguration : BaseEntityConfiguration<Payment>
{
    public override void Configure(EntityTypeBuilder<Payment> b) { base.Configure(b); b.Property(x => x.Method).HasConversion<string>().HasMaxLength(30).IsRequired(); b.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired(); CommerceConfigurationSupport.Money(b.Property(x => x.Amount)).IsRequired(); CommerceConfigurationSupport.Unique(b, nameof(Payment.OrderId)); b.HasOne<Order>().WithMany().HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Restrict); b.ToTable(t => t.HasCheckConstraint("CK_Payment_Amount", "\"Amount\" >= 0")); }
}
