namespace Ecom.Application.Features.Catalog.Common;

public static class ProductSort
{
    public const string Newest = "newest";
    public const string NameAscending = "name-asc";
    public const string PriceAscending = "price-asc";
    public const string PriceDescending = "price-desc";

    public static bool IsSupported(string value) => value is Newest or NameAscending or PriceAscending or PriceDescending;
}
