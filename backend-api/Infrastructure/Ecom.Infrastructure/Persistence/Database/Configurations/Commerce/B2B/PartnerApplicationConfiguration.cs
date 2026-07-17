using Ecom.Domain.Entities;
using Ecom.Domain.Enums;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class PartnerApplicationConfiguration : BaseEntityConfiguration<PartnerApplication> { public override void Configure(EntityTypeBuilder<PartnerApplication> b) { base.Configure(b); b.Property(x => x.ApplicantName).HasMaxLength(300).IsRequired(); b.Property(x => x.OrganizationName).HasMaxLength(300); b.Property(x => x.Email).HasMaxLength(255); b.Property(x => x.PhoneNumber).HasMaxLength(20).IsRequired(); b.Property(x => x.ApplicationType).HasConversion<string>().HasMaxLength(30).IsRequired(); b.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired(); b.Property(x => x.Message).HasColumnType("text"); b.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull); } }

