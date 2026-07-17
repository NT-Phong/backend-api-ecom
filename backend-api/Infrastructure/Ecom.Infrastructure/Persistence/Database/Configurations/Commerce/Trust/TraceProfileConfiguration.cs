using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class TraceProfileConfiguration : BaseEntityConfiguration<TraceProfile> { public override void Configure(EntityTypeBuilder<TraceProfile> b) { base.Configure(b); b.Property(x => x.PublicCode).HasMaxLength(100).IsRequired(); b.Property(x => x.Summary).HasColumnType("text"); b.Property(x => x.PublicStatus).HasConversion<string>().HasMaxLength(30).IsRequired(); CommerceConfigurationSupport.Unique(b, nameof(TraceProfile.ProductId)); CommerceConfigurationSupport.Unique(b, nameof(TraceProfile.PublicCode)); b.HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict); } }

