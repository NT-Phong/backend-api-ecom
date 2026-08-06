using Ecom.Infrastructure.Persistence.Outbox;

namespace Ecom.Infrastructure.Services;

public sealed class OutboxMessageDispatcher(IPublisher publisher)
{
    public async Task DispatchAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        var eventType = Type.GetType(message.EventType, throwOnError: false);
        if (eventType is null)
            throw new InvalidOperationException($"Outbox event type '{message.EventType}' could not be resolved.");

        var domainEvent = JsonSerializer.Deserialize(message.Payload, eventType) as INotification;
        if (domainEvent is null)
            throw new InvalidOperationException($"Outbox event '{message.Id}' is not a MediatR notification.");

        await publisher.Publish(domainEvent, cancellationToken);
    }
}
