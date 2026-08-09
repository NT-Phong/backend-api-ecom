using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class PaymentGatewayNotificationConfiguration : BaseEntityConfiguration<PaymentGatewayNotification>
{
    public override void Configure(EntityTypeBuilder<PaymentGatewayNotification> b)
    {
        base.Configure(b);
        b.Property(x => x.Provider).HasMaxLength(50).IsRequired();
        b.Property(x => x.NotificationType).HasMaxLength(50).IsRequired();
        b.Property(x => x.Disposition).HasConversion<string>().HasMaxLength(30).IsRequired();
        b.Property(x => x.InvoiceNumber).HasMaxLength(200);
        CommerceConfigurationSupport.Money(b.Property(x => x.OrderAmount));
        CommerceConfigurationSupport.Money(b.Property(x => x.TransactionAmount));
        b.Property(x => x.CurrencyCode).HasMaxLength(3).IsFixedLength();
        b.Property(x => x.ExternalOrderId).HasMaxLength(200);
        b.Property(x => x.ExternalTransactionId).HasMaxLength(200);
        b.Property(x => x.ExternalTransactionReference).HasMaxLength(200);
        b.Property(x => x.ProviderOrderStatus).HasMaxLength(50);
        b.Property(x => x.ProviderTransactionStatus).HasMaxLength(50);
        b.Property(x => x.FailureReasonCode).HasMaxLength(100);
        b.Property(x => x.ReceivedAt).HasColumnType("timestamp with time zone").IsRequired();
        b.Property(x => x.OccurredAt).HasColumnType("timestamp with time zone");
        CommerceConfigurationSupport.UniqueWhere(b,
            "\"IsDeleted\" = false AND \"ExternalTransactionId\" IS NOT NULL",
            nameof(PaymentGatewayNotification.Provider), nameof(PaymentGatewayNotification.NotificationType),
            nameof(PaymentGatewayNotification.ExternalTransactionId));
        b.HasIndex(x => new { x.Provider, x.Disposition, x.ReceivedAt })
            .HasDatabaseName("IX_PaymentGatewayNotification_Provider_Disposition_ReceivedAt");
        b.HasOne<PaymentGatewayAttempt>().WithMany().HasForeignKey(x => x.PaymentGatewayAttemptId).OnDelete(DeleteBehavior.Restrict);
    }
}
