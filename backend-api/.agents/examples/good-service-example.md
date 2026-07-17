# Good Service Example

Application services should answer focused use-case questions through project abstractions.

Project-style shape based on realtime access services:

```csharp
public sealed class ScaleRealtimeAccessService : IScaleRealtimeAccessService
{
    private readonly ICurrentUser _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public ScaleRealtimeAccessService(ICurrentUser currentUser, IUnitOfWork unitOfWork)
    {
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> CanAccessScaleAsync(Guid scaleId, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated)
            return false;

        var scale = await _unitOfWork.Repository<Scale>()
            .FindByIdAsync(scaleId);

        return scale is not null && _currentUser.HasPolicy(Permissions.Scale.Read);
    }
}
```

## Why This Is Good
- Dependencies are project abstractions.
- No HTTP details leak into application logic.
- Permission logic is explicit.
- The method answers one access question.
