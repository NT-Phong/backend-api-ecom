using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class PaymentGatewayAttemptConfiguration : BaseEntityConfiguration<PaymentGatewayAttempt>
{
    public override void Configure(EntityTypeBuilder<PaymentGatewayAttempt> b)
    {
        base.Configure(b);
        b.Property(x => x.Provider).HasMaxLength(50).IsRequired();
        b.Property(x => x.InvoiceNumber).HasMaxLength(200).IsRequired();
        CommerceConfigurationSupport.Money(b.Property(x => x.ExpectedAmount)).IsRequired();
        b.Property(x => x.CurrencyCode).HasMaxLength(3).IsFixedLength().IsRequired();
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
        b.Property(x => x.ExpiresAt).HasColumnType("timestamp with time zone").IsRequired();
        b.Property(x => x.CheckoutIssuedAt).HasColumnType("timestamp with time zone");
        b.Property(x => x.PaidAt).HasColumnType("timestamp with time zone");
        b.Property(x => x.LastNotificationAt).HasColumnType("timestamp with time zone");
        b.Property(x => x.ExternalOrderId).HasMaxLength(200);
        b.Property(x => x.ExternalTransactionId).HasMaxLength(200);
        b.Property(x => x.ExternalTransactionReference).HasMaxLength(200);
        b.Property(x => x.ProviderOrderStatus).HasMaxLength(50);
        b.Property(x => x.ProviderTransactionStatus).HasMaxLength(50);
        CommerceConfigurationSupport.Unique(b, nameof(PaymentGatewayAttempt.Provider), nameof(PaymentGatewayAttempt.InvoiceNumber));
        CommerceConfigurationSupport.Unique(b, nameof(PaymentGatewayAttempt.PaymentId), nameof(PaymentGatewayAttempt.Provider));
        CommerceConfigurationSupport.Unique(b, nameof(PaymentGatewayAttempt.Provider), nameof(PaymentGatewayAttempt.ExternalTransactionId));
        b.HasIndex(x => new { x.PaymentId, x.Status, x.ExpiresAt }).HasDatabaseName("IX_PaymentGatewayAttempt_Payment_Status_ExpiresAt");
        b.HasOne<Payment>().WithMany().HasForeignKey(x => x.PaymentId).OnDelete(DeleteBehavior.Restrict);
        b.ToTable(t => t.HasCheckConstraint("CK_PaymentGatewayAttempt_ExpectedAmount", "\"ExpectedAmount\" > 0"));
    }
}
