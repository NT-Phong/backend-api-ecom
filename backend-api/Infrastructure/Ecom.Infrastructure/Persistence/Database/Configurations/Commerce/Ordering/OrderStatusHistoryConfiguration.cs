using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class OrderStatusHistoryConfiguration : BaseEntityConfiguration<OrderStatusHistory>
{
    public override void Configure(EntityTypeBuilder<OrderStatusHistory> b) { base.Configure(b); b.Property(x => x.FromStatus).HasConversion<string>().HasMaxLength(30); b.Property(x => x.ToStatus).HasConversion<string>().HasMaxLength(30).IsRequired(); b.Property(x => x.Reason).HasMaxLength(1000); b.HasOne<Order>().WithMany().HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade); }
}

