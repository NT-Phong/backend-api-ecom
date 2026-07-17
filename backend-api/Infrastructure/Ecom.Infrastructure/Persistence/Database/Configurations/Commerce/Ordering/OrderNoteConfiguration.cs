using Ecom.Domain.Entities;
using Ecom.Infrastructure.Persistence.Database.Configurations.Base;

namespace Ecom.Infrastructure.Persistence.Database.Configurations.Commerce;

public sealed class OrderNoteConfiguration : BaseEntityConfiguration<OrderNote>
{
    public override void Configure(EntityTypeBuilder<OrderNote> b) { base.Configure(b); b.Property(x => x.NoteType).HasConversion<string>().HasMaxLength(30).IsRequired(); b.Property(x => x.Content).HasColumnType("text").IsRequired(); b.Property(x => x.IsVisibleToCustomer).HasDefaultValue(false); b.HasOne<Order>().WithMany().HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade); }
}

