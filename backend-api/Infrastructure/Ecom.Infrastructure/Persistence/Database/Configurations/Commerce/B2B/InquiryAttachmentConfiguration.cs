using Ecom.Domain.Entities;
using Ecom.Domain.Enums;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class InquiryAttachmentConfiguration : BaseEntityConfiguration<InquiryAttachment> { public override void Configure(EntityTypeBuilder<InquiryAttachment> b) { base.Configure(b); b.Property(x => x.Visibility).HasConversion<string>().HasMaxLength(20).HasDefaultValue(MediaVisibility.Internal).IsRequired(); b.HasOne<TradeInquiry>().WithMany().HasForeignKey(x => x.TradeInquiryId).OnDelete(DeleteBehavior.SetNull); b.HasOne<PartnerApplication>().WithMany().HasForeignKey(x => x.PartnerApplicationId).OnDelete(DeleteBehavior.SetNull); b.HasOne<MediaAsset>().WithMany().HasForeignKey(x => x.MediaAssetId).OnDelete(DeleteBehavior.Restrict); b.ToTable(t => t.HasCheckConstraint("CK_InquiryAttachment_Parent", "(\"TradeInquiryId\" IS NULL) <> (\"PartnerApplicationId\" IS NULL)")); } }
