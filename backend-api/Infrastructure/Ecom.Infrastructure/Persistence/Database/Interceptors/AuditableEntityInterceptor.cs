using Ecom.Domain.Common.Interfaces;

namespace Ecom.Infrastructure.Persistence.Database.Interceptors;

public class AuditableEntityInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUser _currentUser;
    private readonly IDateTimeService _dateTime;

    public AuditableEntityInterceptor(ICurrentUser currentUser, IDateTimeService dateTime)
    {
        _currentUser = currentUser;
        _dateTime = dateTime;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateEntities(DbContext? context)
    {
        if (context == null) return;

        var resolvedUserId = _currentUser.UserId;
        Guid? currentUserId = resolvedUserId == Guid.Empty ? null : resolvedUserId;
        var utcNow = _dateTime.UtcNow.UtcDateTime;

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is IAuditableEntity auditable)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        if (auditable.CreatedBy == null || auditable.CreatedBy == Guid.Empty)
                            auditable.CreatedBy = currentUserId;
                        if (auditable.CreatedAt == default(DateTime))
                            auditable.CreatedAt = utcNow;
                        break;

                    case EntityState.Modified:
                        auditable.UpdatedBy = currentUserId;
                        auditable.UpdatedAt = utcNow;
                        break;
                }
            }

            if (entry.State == EntityState.Deleted && entry.Entity is ISoftDelete softDelete)
            {
                entry.State = EntityState.Modified;
                softDelete.IsDeleted = true;
                softDelete.DeletedAt = utcNow;
                softDelete.DeletedBy = currentUserId;
            }

            if (entry.State == EntityState.Modified && entry.Entity is IHasConcurrencyStamp concurrencyEntity
                && !entry.Property(nameof(IHasConcurrencyStamp.ConcurrencyStamp)).IsModified)
            {
                concurrencyEntity.ConcurrencyStamp = Guid.NewGuid();
            }
        }
    }
} 

