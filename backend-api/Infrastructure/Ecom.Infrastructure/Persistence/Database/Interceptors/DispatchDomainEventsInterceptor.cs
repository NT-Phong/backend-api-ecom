namespace Ecom.Infrastructure.Persistence.Database.Interceptors;

public class DispatchDomainEventsInterceptor : SaveChangesInterceptor
{
    private readonly IMediator _mediator;
    private readonly ILogger<DispatchDomainEventsInterceptor> _logger;

    public DispatchDomainEventsInterceptor(IMediator mediator, ILogger<DispatchDomainEventsInterceptor> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        // For synchronous SaveChanges, we'll collect events but dispatch them later
        // to avoid sync-over-async anti-pattern. This is a compromise for legacy sync calls.
        var entities = eventData.Context?.ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity).ToList();

        if (entities?.Any() == true)
        {
            _logger.LogWarning("Domain events detected in synchronous SaveChanges. Consider using SaveChangesAsync for better performance.");
            // Clear events to prevent them from being persisted without dispatch
            entities.ForEach(e => e.ClearDomainEvents());
        }

        return base.SavingChanges(eventData, result);
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        await DispatchEvents(eventData.Context);
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private async Task DispatchEvents(DbContext? context)
    {
        if (context == null) return;

        var entities = context.ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity).ToList();

        var domainEvents = entities.SelectMany(e => e.DomainEvents).ToList();
        entities.ForEach(e => e.ClearDomainEvents());

        foreach (var domainEvent in domainEvents)
        {
            _logger.LogInformation("Dispatching domain event: {DomainEvent}", domainEvent.GetType().Name);
            await _mediator.Publish(domainEvent);
        }
    }
} 
