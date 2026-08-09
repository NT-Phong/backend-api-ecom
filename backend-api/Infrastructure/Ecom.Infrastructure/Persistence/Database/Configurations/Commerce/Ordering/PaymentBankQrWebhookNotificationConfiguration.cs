using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class PaymentBankQrWebhookNotificationConfiguration : BaseEntityConfiguration<PaymentBankQrWebhookNotification>
{
    public override void Configure(EntityTypeBuilder<PaymentBankQrWebhookNotification> b)
    {
        base.Configure(b);
        b.Property(x => x.Provider).HasMaxLength(50).IsRequired();
        b.Property(x => x.NotificationType).HasMaxLength(50).IsRequired();
        b.Property(x => x.Disposition).HasConversion<string>().HasMaxLength(30).IsRequired();
        b.Property(x => x.PaymentCode).HasMaxLength(100);
        CommerceConfigurationSupport.Money(b.Property(x => x.TransactionAmount));
        b.Property(x => x.CurrencyCode).HasMaxLength(3).IsFixedLength();
        b.Property(x => x.ExternalTransactionId).HasMaxLength(200);
        b.Property(x => x.ExternalTransactionReference).HasMaxLength(200);
        b.Property(x => x.FailureReasonCode).HasMaxLength(100);
        b.Property(x => x.ReceivedAt).HasColumnType("timestamp with time zone").IsRequired();
        b.Property(x => x.OccurredAt).HasColumnType("timestamp with time zone");
        CommerceConfigurationSupport.UniqueWhere(b, "\"IsDeleted\" = false AND \"ExternalTransactionId\" IS NOT NULL",
            nameof(PaymentBankQrWebhookNotification.Provider), nameof(PaymentBankQrWebhookNotification.NotificationType),
            nameof(PaymentBankQrWebhookNotification.ExternalTransactionId));
        b.HasIndex(x => new { x.Provider, x.Disposition, x.ReceivedAt }).HasDatabaseName("IX_PaymentBankQrNotification_Provider_Disposition_ReceivedAt");
        b.HasOne<PaymentBankQrAttempt>().WithMany().HasForeignKey(x => x.PaymentBankQrAttemptId).OnDelete(DeleteBehavior.Restrict);
    }
}
