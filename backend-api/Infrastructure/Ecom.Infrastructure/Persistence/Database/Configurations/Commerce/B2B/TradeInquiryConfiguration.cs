using Ecom.Domain.Entities;
using Ecom.Domain.Enums;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class TradeInquiryConfiguration : BaseEntityConfiguration<TradeInquiry> { public override void Configure(EntityTypeBuilder<TradeInquiry> b) { base.Configure(b); b.Property(x => x.InquiryNumber).HasMaxLength(40).IsRequired(); b.Property(x => x.ContactName).HasMaxLength(200).IsRequired(); b.Property(x => x.CompanyName).HasMaxLength(300); b.Property(x => x.Email).HasMaxLength(255); b.Property(x => x.PhoneNumber).HasMaxLength(20).IsRequired(); b.Property(x => x.InquiryType).HasConversion<string>().HasMaxLength(30).IsRequired(); b.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired(); b.Property(x => x.Message).HasColumnType("text"); CommerceConfigurationSupport.Unique(b, nameof(TradeInquiry.InquiryNumber)); b.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull); } }

