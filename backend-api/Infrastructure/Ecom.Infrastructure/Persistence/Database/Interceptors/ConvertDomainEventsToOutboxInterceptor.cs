using System.Text.Json;
using Ecom.Domain.Common;
using Ecom.Infrastructure.Persistence.Outbox;

namespace Ecom.Infrastructure.Persistence.Database.Interceptors;

public sealed class ConvertDomainEventsToOutboxInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Convert(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
        InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Convert(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void Convert(DbContext? context)
    {
        if (context is null) return;
        var entities = context.ChangeTracker.Entries<BaseEntity>()
            .Where(x => x.Entity.DomainEvents.Count > 0)
            .Select(x => x.Entity)
            .ToList();
        var events = entities.SelectMany(x => x.DomainEvents).ToList();
        if (events.Count == 0) return;

        foreach (var domainEvent in events)
        {
            var eventType = domainEvent.GetType();
            var eventId = domainEvent is BaseEvent baseEvent ? baseEvent.EventId : Guid.NewGuid();
            var occurredOn = domainEvent is BaseEvent occurred ? occurred.OccurredOn : DateTimeOffset.UtcNow;
            var payload = JsonSerializer.Serialize(domainEvent, eventType);
            context.Set<OutboxMessage>().Add(OutboxMessage.Create(eventId, occurredOn,
                eventType.AssemblyQualifiedName ?? eventType.FullName ?? eventType.Name, payload));
        }
        entities.ForEach(x => x.ClearDomainEvents());
    }
}
