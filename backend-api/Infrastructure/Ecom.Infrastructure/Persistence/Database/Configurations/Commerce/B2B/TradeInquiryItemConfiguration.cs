using Ecom.Domain.Entities;
using Ecom.Domain.Enums;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class TradeInquiryItemConfiguration : BaseEntityConfiguration<TradeInquiryItem> { public override void Configure(EntityTypeBuilder<TradeInquiryItem> b) { base.Configure(b); CommerceConfigurationSupport.Quantity(b.Property(x => x.RequestedQuantity)); b.Property(x => x.RequirementText).HasMaxLength(1000); b.HasOne<TradeInquiry>().WithMany().HasForeignKey(x => x.TradeInquiryId).OnDelete(DeleteBehavior.Cascade); b.HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.SetNull); b.HasOne<ProductVariant>().WithMany().HasForeignKey(x => x.ProductVariantId).OnDelete(DeleteBehavior.SetNull); b.ToTable(t => t.HasCheckConstraint("CK_TradeInquiryItem_Quantity", "\"RequestedQuantity\" IS NULL OR \"RequestedQuantity\" > 0")); } }

