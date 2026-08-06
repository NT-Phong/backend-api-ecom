using Ecom.Application.Common.Commerce;
using Ecom.Domain.Entities;

namespace Ecom.Application.Common.Interfaces;

public interface ICartPrincipalResolver
{
    CartPrincipal ResolveOrCreateGuestPrincipal();
    CartPrincipal? ResolveExistingPrincipal();
    CartPrincipal? ResolveGuestPrincipal();
    void ClearGuestPrincipal();
}

public interface ICheckoutPricingService
{
    Task<TResult<CheckoutQuote>> CreateQuoteAsync(CartPrincipal principal, IReadOnlyCollection<Guid> cartItemIds,
        CheckoutRecipient recipient, PaymentMethod paymentMethod, CancellationToken cancellationToken);
}

public interface IInventoryReservationStore
{
    Task<TResult<IReadOnlyDictionary<Guid, LockedInventory>>> LockTrackedInventoryAsync(
        IReadOnlyCollection<InventoryLockRequest> requests, CancellationToken cancellationToken);
}

public interface IIdempotencyStore
{
    Task<IdempotencyBeginResult> BeginAsync(string operation, string ownerScope, string key, string fingerprint,
        DateTime expiresAt, CancellationToken cancellationToken);
}

public interface IOrderNumberGenerator
{
    string Create(DateTime nowUtc);
}
