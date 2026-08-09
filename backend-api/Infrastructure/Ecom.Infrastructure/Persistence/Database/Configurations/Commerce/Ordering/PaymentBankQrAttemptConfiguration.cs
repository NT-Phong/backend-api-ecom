using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class PaymentBankQrAttemptConfiguration : BaseEntityConfiguration<PaymentBankQrAttempt>
{
    public override void Configure(EntityTypeBuilder<PaymentBankQrAttempt> b)
    {
        base.Configure(b);
        b.Property(x => x.Provider).HasMaxLength(50).IsRequired();
        b.Property(x => x.PaymentCode).HasMaxLength(100).IsRequired();
        CommerceConfigurationSupport.Money(b.Property(x => x.ExpectedAmount)).IsRequired();
        b.Property(x => x.CurrencyCode).HasMaxLength(3).IsFixedLength().IsRequired();
        b.Property(x => x.VirtualAccountFingerprint).HasMaxLength(64).IsRequired();
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
        b.Property(x => x.ExpiresAt).HasColumnType("timestamp with time zone").IsRequired();
        b.Property(x => x.QrIssuedAt).HasColumnType("timestamp with time zone");
        b.Property(x => x.PaidAt).HasColumnType("timestamp with time zone");
        b.Property(x => x.LastNotificationAt).HasColumnType("timestamp with time zone");
        b.Property(x => x.ExternalTransactionId).HasMaxLength(200);
        b.Property(x => x.ExternalTransactionReference).HasMaxLength(200);
        CommerceConfigurationSupport.Unique(b, nameof(PaymentBankQrAttempt.Provider), nameof(PaymentBankQrAttempt.PaymentCode));
        CommerceConfigurationSupport.Unique(b, nameof(PaymentBankQrAttempt.PaymentId), nameof(PaymentBankQrAttempt.Provider));
        CommerceConfigurationSupport.Unique(b, nameof(PaymentBankQrAttempt.Provider), nameof(PaymentBankQrAttempt.ExternalTransactionId));
        b.HasIndex(x => new { x.PaymentId, x.Status, x.ExpiresAt }).HasDatabaseName("IX_PaymentBankQrAttempt_Payment_Status_ExpiresAt");
        b.HasOne<Payment>().WithMany().HasForeignKey(x => x.PaymentId).OnDelete(DeleteBehavior.Restrict);
        b.ToTable(t => t.HasCheckConstraint("CK_PaymentBankQrAttempt_ExpectedAmount", "\"ExpectedAmount\" > 0"));
    }
}
