using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class VariantPriceConfiguration : BaseEntityConfiguration<VariantPrice>
{
    public override void Configure(EntityTypeBuilder<VariantPrice> b) { base.Configure(b); b.Property(x => x.CurrencyCode).HasMaxLength(CommerceConstants.CurrencyCodeLength).IsFixedLength().HasDefaultValue(CommerceConstants.DefaultCurrency).IsRequired(); CommerceConfigurationSupport.Money(b.Property(x => x.Amount)).IsRequired(); b.Property(x => x.MinQuantity).HasDefaultValue(1); b.Property(x => x.PriceType).HasConversion<string>().HasMaxLength(30).IsRequired(); b.HasIndex(x => new { x.ProductVariantId, x.EffectiveFrom, x.EffectiveTo }); b.HasOne<ProductVariant>().WithMany().HasForeignKey(x => x.ProductVariantId).OnDelete(DeleteBehavior.Restrict); b.HasOne<PriceList>().WithMany().HasForeignKey(x => x.PriceListId).OnDelete(DeleteBehavior.SetNull); b.ToTable(t => { t.HasCheckConstraint("CK_VariantPrice_Amount", "\"Amount\" >= 0"); t.HasCheckConstraint("CK_VariantPrice_MinQuantity", "\"MinQuantity\" > 0"); t.HasCheckConstraint("CK_VariantPrice_TimeWindow", "\"EffectiveTo\" IS NULL OR \"EffectiveTo\" > \"EffectiveFrom\""); }); }
}
