# Checkout source map

Use this map to accelerate discovery only. Re-run the scan script and inspect current source before asserting behavior; the working tree may be ahead of this reference.

## Entry points

| Boundary | Locate under |
| --- | --- |
| Storefront routes | `Presentation/Ecom.API/Controllers/V1/{Cart,Checkout,Orders}Controller.cs` |
| Backoffice lifecycle routes | `Presentation/Ecom.API/Controllers/V1/ManagementOrdersController.cs` |
| Cart, address, checkout and order slices | `Core/Ecom.Application/Features/Commerce/` |
| Pricing and principal abstractions | `Core/Ecom.Application/Common/{Commerce,Interfaces,Services}/` |
| Aggregates | `Core/Ecom.Domain/Entities/Commerce/{Ordering,Inventory,Customer,System}/` |
| Persistence and locking | `Infrastructure/Ecom.Infrastructure/{Services,Persistence/Database}/` |
| Commerce migrations | `Infrastructure/Ecom.Infrastructure/Migrations/` |

## Current design anchors

- `CartPrincipalResolver` uses an opaque Secure/HttpOnly guest-cart cookie; storage and Order snapshots use its SHA-256 hash only.
- `CheckoutPricingService` is the price/availability/shipping authority. The shipping setting key is `checkout.shipping.standardFeeVnd`.
- `IInventoryReservationStore` owns PostgreSQL stock locking; use it only inside transactional CreateOrder.
- `IIdempotencyStore` owns key races. Replays require the same request fingerprint; a different fingerprint is a conflict.
- `Order`, `Shipment`, `Payment`, `InventoryReservation`, and `InventoryLevel` own transitions and movements. Handlers orchestrate facts, authorization, and persistence.
- `AddCommerceCheckoutIdempotency` and `AddGuestOrderOwnership` are forward-only migrations. Inspect their position against other un-applied migrations before generating another one.

## Search targets

```powershell
rg -n "ITransactionalRequest|UnitOfWorkBehavior|CreateOrder|PreviewCheckout|Idempotency" Core/Ecom.Application Infrastructure
rg -n "CheckoutSelectedItems|ConfirmHold|Reserve\(|MarkDeliveryFailed|GuestTokenHashSnapshot" Core/Ecom.Domain
rg -n "HasIndex|IdempotencyRecord|GuestTokenHashSnapshot|CartItem" Infrastructure/Ecom.Infrastructure/Persistence/Database/Configurations Infrastructure/Ecom.Infrastructure/Migrations
rg -n "Cart|Checkout|orders|shipment" Presentation/Ecom.API/Controllers/V1 Tests
```
