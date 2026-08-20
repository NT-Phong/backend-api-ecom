namespace Ecom.Domain.Tests.Commerce;

public class ProductAndCartTests
{
    [Fact]
    public void Product_requires_publish_prerequisites_and_rejects_content_changes_while_discontinued()
    {
        var product = Product.Create(Guid.NewGuid(), "Local product", "local-product");
        product.SubmitForReview();

        var missing = Assert.Throws<CommerceDomainException>(() =>
            product.Publish(DateTime.UtcNow, true, true, true, false));
        Assert.Equal("PRODUCT_PUBLISH_REQUIREMENTS_MISSING", missing.Code);

        product.Publish(DateTime.UtcNow, true, true, true, true);
        Assert.Equal(ProductStatus.Published, product.Status);
        product.Discontinue(DateTime.UtcNow);
        Assert.Throws<CommerceDomainException>(() => product.SubmitForReview());
    }

    [Fact]
    public void Discontinued_product_restores_to_paused_before_reentering_review()
    {
        var product = Product.Create(Guid.NewGuid(), "Local product", "local-product");
        product.Discontinue(DateTime.UtcNow.AddMinutes(-1));

        var restoredAt = DateTime.UtcNow;
        product.RestoreFromDiscontinued(restoredAt);

        Assert.Equal(ProductStatus.Paused, product.Status);
        Assert.Equal(restoredAt, product.UnpublishedAt);
        product.SubmitForReview();
        Assert.Equal(ProductStatus.Review, product.Status);
    }

    [Fact]
    public void Product_trims_brand_name_when_details_are_updated()
    {
        var product = Product.Create(Guid.NewGuid(), "Local product", "local-product");

        product.UpdateDetails("Local product", "local-product", null, null, null, null, null, null, null, "  Mebieco  ");

        Assert.Equal("Mebieco", product.BrandName);
    }

    [Fact]
    public void Product_requires_one_primary_category_when_replacing_categories()
    {
        var product = Product.Create(Guid.NewGuid(), "Local product", "local-product");
        var categories = new List<ProductCategory>();

        Assert.Throws<CommerceDomainException>(() => product.ReplaceCategories(categories,
            [new ProductCategoryAssignment(Guid.NewGuid(), false)]));

        var replacements = product.ReplaceCategories(categories,
            [new ProductCategoryAssignment(Guid.NewGuid(), true), new ProductCategoryAssignment(Guid.NewGuid(), false)]);

        Assert.Equal(2, replacements.Count);
        Assert.Single(categories, x => x.IsPrimary);
    }

    [Fact]
    public void Discontinued_product_rejects_content_mutations_and_renews_its_version()
    {
        var product = Product.Create(Guid.NewGuid(), "Local product", "local-product");
        var originalStamp = product.ConcurrencyStamp;

        product.Discontinue(DateTime.UtcNow);

        var error = Assert.Throws<CommerceDomainException>(() => product.EnsureContentCanBeChanged());
        Assert.Equal("PRODUCT_DISCONTINUED", error.Code);
        Assert.NotEqual(originalStamp, product.RenewConcurrencyStamp());
    }

    [Fact]
    public void Published_product_returns_to_review_when_public_content_changes()
    {
        var product = Product.Create(Guid.NewGuid(), "Local product", "local-product");
        var publishedAt = DateTime.UtcNow.AddMinutes(-1);
        product.SubmitForReview();
        product.Publish(publishedAt, true, true, true, true);

        var changedAt = DateTime.UtcNow;
        product.ReturnToReviewIfPublished(changedAt);

        Assert.Equal(ProductStatus.Review, product.Status);
        Assert.Equal(changedAt, product.UnpublishedAt);
        Assert.Equal(publishedAt, product.PublishedAt);
    }

    [Fact]
    public void Variant_price_requires_a_valid_effective_window()
    {
        var variantId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        Assert.Throws<CommerceDomainException>(() =>
            VariantPrice.Create(variantId, 100m, PriceType.Public, now, now));

        var price = VariantPrice.Create(variantId, 100m, PriceType.Sale, now, now.AddDays(1));
        Assert.Equal(PriceType.Sale, price.PriceType);
        Assert.Equal("VND", price.CurrencyCode);
    }

    [Fact]
    public void Discontinued_variant_cannot_accept_new_prices()
    {
        var variant = ProductVariant.Create(Guid.NewGuid(), "SKU-01", "Default", InventoryMode.NotTracked);
        variant.Discontinue();

        var error = Assert.Throws<CommerceDomainException>(() => variant.EnsurePricingCanBeChanged());

        Assert.Equal("VARIANT_DISCONTINUED", error.Code);
    }

    [Fact]
    public void Cart_merges_duplicate_variant_and_blocks_changes_after_conversion()
    {
        var cart = Cart.CreateForUser(Guid.NewGuid());
        var items = new List<CartItem>();
        var variantId = Guid.NewGuid();

        cart.AddItem(items, variantId, 2);
        cart.AddItem(items, variantId, 3);

        Assert.Single(items);
        Assert.Equal(5, items[0].Quantity);
        cart.MarkConverted();
        Assert.Throws<CommerceDomainException>(() => cart.AddItem(items, Guid.NewGuid(), 1));
    }

    [Fact]
    public void Cart_owner_must_be_user_or_guest()
    {
        Assert.Throws<CommerceDomainException>(() => Cart.CreateForUser(Guid.Empty));
        Assert.Throws<CommerceDomainException>(() => Cart.CreateForGuest(" ", DateTime.UtcNow.AddHours(1)));
    }

    [Fact]
    public void Guest_cart_expires_only_after_its_expiry_time()
    {
        var now = DateTime.UtcNow;
        var cart = Cart.CreateForGuest("guest-token-hash", now.AddMinutes(1));

        Assert.False(cart.ExpireIfDue(now));
        Assert.True(cart.ExpireIfDue(now.AddMinutes(1)));
        Assert.Equal(CartStatus.Expired, cart.Status);
        Assert.False(cart.ExpireIfDue(now.AddMinutes(2)));
    }

    [Fact]
    public void Cart_checkout_selected_items_keeps_unselected_lines_active()
    {
        var cart = Cart.CreateForUser(Guid.NewGuid());
        var items = new List<CartItem>();
        cart.AddItem(items, Guid.NewGuid(), 1);
        cart.AddItem(items, Guid.NewGuid(), 2);

        cart.CheckoutSelectedItems(items, [items[0].Id]);

        Assert.Equal(CartStatus.Active, cart.Status);
        Assert.True(items[0].IsDeleted);
        Assert.False(items[1].IsDeleted);
    }
}
