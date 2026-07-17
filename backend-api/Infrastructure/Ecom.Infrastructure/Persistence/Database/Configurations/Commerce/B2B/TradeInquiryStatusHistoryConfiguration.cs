using Ecom.Domain.Entities;
using Ecom.Domain.Enums;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class TradeInquiryStatusHistoryConfiguration : BaseEntityConfiguration<TradeInquiryStatusHistory> { public override void Configure(EntityTypeBuilder<TradeInquiryStatusHistory> b) { base.Configure(b); b.Property(x => x.FromStatus).HasConversion<string>().HasMaxLength(30); b.Property(x => x.ToStatus).HasConversion<string>().HasMaxLength(30).IsRequired(); b.Property(x => x.Reason).HasMaxLength(1000); b.HasOne<TradeInquiry>().WithMany().HasForeignKey(x => x.TradeInquiryId).OnDelete(DeleteBehavior.Cascade); } }

