# Catalog Product Domain Map

## Relationships

```text
Producer (ID) -> Product aggregate
Product -> ProductCategory -> Category
Product -> ProductMedia -> MediaAsset
Product -> ProductVariant -> VariantPrice -> PriceList (optional)
ProductVariant -> ProductVariantOptionValue -> ProductOptionValue -> ProductOption
```

Product has no authoritative SKU, price, stock, or media storage key.

## Product

States: `Draft -> Review -> Published -> Paused`; `Discontinued` is terminal.

- Create requires producer ID, name, slug.
- Details/category/media/variant/price mutations reject discontinued Product.
- Category replacement requires non-empty unique IDs and exactly one primary mapping.
- Primary media must be Clean + Public; published Product cannot remove it.
- Publish from Review requires Published+verified Producer, Published primary Category, Clean+Public primary media, active Variant, effective eligible price.
- Product `ConcurrencyStamp` versions every child mutation.

## Variant and price

Variant states: `Active <-> Paused -> Discontinued`.

- SKU is immutable after create.
- Discontinued Variant cannot change or accept price.
- VariantPrice is append-only through API; amount, currency, minimum quantity, and time window have invariants.
- Public effective price: VND, minimum quantity 1, valid window/PriceList; `Sale` before `Public`; exclude `B2B`.
- PostgreSQL exclusion constraint prevents overlapping active periods in one variant/price-list/type scope.

## Public eligibility

Require Product Published, Producer Published+verified, Published primary Category, active priced Variant, and Public+Clean MediaAsset before returning storefront data.
