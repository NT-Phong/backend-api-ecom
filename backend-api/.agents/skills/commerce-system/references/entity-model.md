# Commerce Entity and Aggregate Model

## Rules

- One entity per file; private setters and private EF constructor.
- Factory for valid initial state; domain methods for mutation.
- Stable failures through `CommerceDomainException.Code`.
- Cross-aggregate references use IDs.
- Transaction snapshots/history are immutable sources of historical truth.

## Aggregate Map

```text
Product -> Variant/Option/Media/Category
Variant -> VariantPrice -> InventoryItem
Cart -> CartItem -> ProductVariantId
Order -> OrderItem/Discount/StatusHistory/Note
InventoryItem -> Level/Reservation/Movement
Payment -> PaymentTransaction
Shipment -> ShipmentItem/History
TradeInquiry -> Item/StatusHistory/Attachment
```

## Product and Variant

- Product: `Draft -> Review -> Published -> Paused`; `Discontinued` is terminal in MVP.
- Publish requires producer, primary category/media, active variant, and effective Public/Sale price.
- Product owns no authoritative SKU, price, or stock.
- Variant is sellable; `Active <-> Paused -> Discontinued`.

## Cart

- Exactly one owner: UserId XOR GuestTokenHash.
- `Active -> Converted | Expired`; terminal carts cannot change.
- Duplicate variant adds quantity.
- Guest API uses opaque Secure HttpOnly SameSite=Lax cookie and persists only hash.

## Order

```text
Pending -> Confirmed | Cancelled
Confirmed -> Preparing | Cancelled
Preparing -> Shipping | Cancelled
Shipping -> Completed | DeliveryFailed
DeliveryFailed -> Shipping | Cancelled
```

- Completed/Cancelled terminal; cancel/failure requires reason.
- Server calculates totals and creates immutable line snapshots.
- Every transition creates OrderStatusHistory.

## Inventory

- InventoryItem root; InventoryLevel balance per StockLocation.
- Available = Stocked - Reserved.
- Mutations: receive, reserve, release, consume, adjust.
- Reservation: `Active -> Consumed | Released | Expired`.
- Movement is append-only; MVP allocates only location code MAIN.

## Payment

- COD starts Pending; BankTransfer starts AwaitingConfirmation; Gateway unsupported.
- `Pending -> AwaitingConfirmation | Paid | Failed | Cancelled`.
- `AwaitingConfirmation -> Paid | Failed | Cancelled`; `Paid -> Refunded`.
- Failed/Refunded/Cancelled terminal; amount must match; each operation creates a transaction.

## Shipment

```text
Pending -> Ready -> Shipping -> Delivered
Shipping -> DeliveryFailed
DeliveryFailed -> Shipping | Cancelled
```

Application coordinates Shipment and Order; roots remain independent.

## TradeInquiry

```text
New -> Assigned -> InProgress -> Quoted -> Won | Lost | Closed
Assigned -> New only via reasoned unassignment
```

Won/Lost/Closed terminal. It is a B2B lead, not seller/store/order ownership.

## Events

- `CommerceStateChangedEvent`: aggregate type/id and from/to state.
- `InventoryChangedEvent`: item/location/movement/delta.
- Inspect `DispatchDomainEventsInterceptor` before handlers; side effects must follow commit or future outbox.
