namespace Ecom.Application.Features.Catalog.Common;

public static class ProductSearchSort
{
    public const string Relevance = "relevance";
    public const string Newest = "newest";
    public const string PriceAscending = "price-asc";
    public const string PriceDescending = "price-desc";

    public static bool IsSupported(string value) => value is Relevance or Newest or PriceAscending or PriceDescending;
}
