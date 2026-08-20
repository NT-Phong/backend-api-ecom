using Ecom.Application.Common.Commerce;
using Ecom.Application.Common.Interfaces;
using Ecom.Domain.Entities;
using Ecom.Domain.Enums;
using Ecom.Infrastructure.Persistence.Database;

namespace Ecom.Infrastructure.Services;

/// <summary>
/// Owns the PostgreSQL locks used while an active cart is converted to an order.
/// </summary>
public sealed class CheckoutCartStore(ApplicationDbContext db) : ICheckoutCartStore
{
    public async Task<LockedCheckoutCart?> LockActiveCartAsync(CartPrincipal principal, DateTime now,
        CancellationToken cancellationToken)
    {
        var cart = principal.UserId.HasValue
            ? await db.Carts.FromSqlInterpolated($@"
SELECT * FROM ""Tbl_Cart""
WHERE ""UserId"" = {principal.UserId.Value}
  AND ""Status"" = {CartStatus.Active.ToString()}
  AND (""ExpiresAt"" IS NULL OR ""ExpiresAt"" > {now})
  AND ""IsDeleted"" = false
FOR UPDATE").SingleOrDefaultAsync(cancellationToken)
            : await db.Carts.FromSqlInterpolated($@"
SELECT * FROM ""Tbl_Cart""
WHERE ""GuestTokenHash"" = {principal.GuestTokenHash}
  AND ""Status"" = {CartStatus.Active.ToString()}
  AND (""ExpiresAt"" IS NULL OR ""ExpiresAt"" > {now})
  AND ""IsDeleted"" = false
FOR UPDATE").SingleOrDefaultAsync(cancellationToken);

        if (cart is null)
            return null;

        var items = await db.CartItems.FromSqlInterpolated($@"
SELECT * FROM ""Tbl_CartItem""
WHERE ""CartId"" = {cart.Id}
  AND ""IsDeleted"" = false
FOR UPDATE").ToListAsync(cancellationToken);

        return new LockedCheckoutCart(cart, items);
    }
}
