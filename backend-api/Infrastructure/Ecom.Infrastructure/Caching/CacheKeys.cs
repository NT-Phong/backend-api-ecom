namespace Ecom.Infrastructure.Caching;

public static class CacheKeys
{
    public static string Order(Guid orderId) => $"order_{orderId}";
    public static string OrdersPage(int pageNumber, int pageSize) => $"orders_page_{pageNumber}_{pageSize}";
    public static string Buyer(Guid buyerId) => $"buyer_{buyerId}";
    public static string Product(Guid productId) => $"product_{productId}";
}
