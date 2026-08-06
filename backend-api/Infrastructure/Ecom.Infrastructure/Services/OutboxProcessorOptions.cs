namespace Ecom.Infrastructure.Services;

public sealed class OutboxProcessorOptions
{
    public const string SectionName = "Outbox:Processor";

    public int PollSeconds { get; set; } = 5;
    public int BatchSize { get; set; } = 50;
    public int LeaseSeconds { get; set; } = 60;
    public int MaxRetries { get; set; } = 8;

    public TimeSpan PollInterval => TimeSpan.FromSeconds(Math.Clamp(PollSeconds, 1, 300));
    public TimeSpan LeaseDuration => TimeSpan.FromSeconds(Math.Clamp(LeaseSeconds, 5, 3600));
    public int ValidBatchSize => Math.Clamp(BatchSize, 1, 500);
    public int ValidMaxRetries => Math.Clamp(MaxRetries, 1, 100);
}
