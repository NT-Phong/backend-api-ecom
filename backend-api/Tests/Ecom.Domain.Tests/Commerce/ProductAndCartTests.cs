namespace Ecom.Domain.Tests.Commerce;

public class ProductAndCartTests
{
    [Fact]
    public void Product_requires_publish_prerequisites_and_cannot_restore_discontinued()
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
}
