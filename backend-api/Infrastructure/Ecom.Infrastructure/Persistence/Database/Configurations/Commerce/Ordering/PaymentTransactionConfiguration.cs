using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class PaymentTransactionConfiguration : BaseEntityConfiguration<PaymentTransaction>
{
    public override void Configure(EntityTypeBuilder<PaymentTransaction> b) { base.Configure(b); b.Property(x => x.Provider).HasMaxLength(50).IsRequired(); b.Property(x => x.ProviderReference).HasMaxLength(200); b.Property(x => x.TransactionType).HasConversion<string>().HasMaxLength(30).IsRequired(); b.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired(); CommerceConfigurationSupport.Money(b.Property(x => x.Amount)).IsRequired(); CommerceConfigurationSupport.Unique(b, nameof(PaymentTransaction.Provider), nameof(PaymentTransaction.ProviderReference)); b.HasOne<Payment>().WithMany().HasForeignKey(x => x.PaymentId).OnDelete(DeleteBehavior.Cascade); b.HasOne<MediaAsset>().WithMany().HasForeignKey(x => x.ProofMediaAssetId).OnDelete(DeleteBehavior.SetNull); b.ToTable(t => t.HasCheckConstraint("CK_PaymentTransaction_Amount", "\"Amount\" >= 0")); }
}

