using Ecom.Application.Common.Commerce;
using Ecom.Domain.Entities;

namespace Ecom.Application.Common.Interfaces;

public interface ICartPrincipalResolver
{
    CartPrincipal ResolveOrCreateGuestPrincipal();
    CartPrincipal RotateGuestPrincipal();
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

    Task<TResult<IReadOnlyDictionary<InventoryLevelLockRequest, InventoryLevel>>> LockInventoryLevelsAsync(
        IReadOnlyCollection<InventoryLevelLockRequest> requests, CancellationToken cancellationToken);
}

public interface IOrderLifecycleStore
{
    Task<Order?> LockOrderAsync(Guid orderId, CancellationToken cancellationToken);
    Task<Payment?> LockPaymentAsync(Guid orderId, CancellationToken cancellationToken);
    Task<Shipment?> LockShipmentAsync(Guid orderId, CancellationToken cancellationToken);
    Task<PaymentGatewayAttempt?> LockPaymentGatewayAttemptAsync(string provider, string invoiceNumber,
        CancellationToken cancellationToken);
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

public sealed record SePayCheckoutRequest(Guid OrderId, string InvoiceNumber, decimal Amount, string OrderNumber, Guid? CustomerId);
public sealed record SePayCheckoutField(string Name, string Value);
public sealed record SePayCheckoutForm(string ActionUrl, string Method, IReadOnlyList<SePayCheckoutField> Fields);

public interface ISePayCheckoutService
{
    bool IsEnabled { get; }
    SePayCheckoutForm CreateCheckoutForm(SePayCheckoutRequest request);
    bool IsValidIpnSecret(string? suppliedSecret);
}
