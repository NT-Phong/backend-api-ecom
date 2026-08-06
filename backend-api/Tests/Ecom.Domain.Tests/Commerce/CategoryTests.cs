namespace Ecom.Domain.Tests.Commerce;

public sealed class CategoryTests
{
    [Fact]
    public void Category_requires_details_and_cannot_change_after_hide()
    {
        Assert.Throws<CommerceDomainException>(() => Category.Create(null, " ", "slug", null, 0));

        var category = Category.Create(null, "Local food", "local-food", null, 0);
        category.Hide();

        Assert.Throws<CommerceDomainException>(() => category.UpdateDetails(null, "Changed", "changed", null, 0));
        Assert.Throws<CommerceDomainException>(() => category.Publish());
    }

    [Fact]
    public void Category_renews_concurrency_stamp_after_mutation()
    {
        var category = Category.Create(null, "Local food", "local-food", null, 0);
        var stamp = category.ConcurrencyStamp;

        var renewed = category.RenewConcurrencyStamp();

        Assert.NotEqual(stamp, renewed);
    }

    [Fact]
    public void Published_category_can_be_paused_and_republished_without_becoming_draft()
    {
        var category = Category.Create(null, "Local food", "local-food", null, 0);
        category.Publish();

        category.Pause();

        Assert.Equal(CatalogStatus.Paused, category.Status);
        category.Publish();
        Assert.Equal(CatalogStatus.Published, category.Status);
    }

    [Fact]
    public void Draft_category_cannot_be_paused()
    {
        var category = Category.Create(null, "Local food", "local-food", null, 0);

        var exception = Assert.Throws<CommerceDomainException>(() => category.Pause());

        Assert.Equal("CATEGORY_NOT_PUBLISHED", exception.Code);
    }
}
