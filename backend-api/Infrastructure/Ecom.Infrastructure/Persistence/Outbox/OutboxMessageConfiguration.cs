namespace Ecom.Infrastructure.Persistence.Outbox;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> b)
    {
        b.ToTable("Tbl_OutboxMessage");
        b.HasKey(x => x.Id);
        b.Property(x => x.EventType).HasMaxLength(500).IsRequired();
        b.Property(x => x.Payload).HasColumnType("jsonb").IsRequired();
        b.Property(x => x.LastError).HasMaxLength(2000);
        b.HasIndex(x => new { x.ProcessedAt, x.DeadLetteredAt, x.NextAttemptAt, x.LeaseExpiresAt, x.OccurredOn })
            .HasDatabaseName("IX_OutboxMessage_Pending");
    }
}
