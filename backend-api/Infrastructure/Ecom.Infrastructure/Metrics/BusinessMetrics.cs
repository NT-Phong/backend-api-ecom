using System.Diagnostics.Metrics;

namespace Ecom.Infrastructure.Metrics;

public class BusinessMetrics : IDisposable
{
    private readonly Meter _meter;
    
    // Counters
    private readonly Counter<long> _ordersCreatedCounter;
    private readonly Counter<long> _buyersCreatedCounter;
    private readonly Counter<long> _ordersCancelledCounter;
    private readonly Counter<long> _ordersShippedCounter;
    
    // Histograms for durations
    private readonly Histogram<double> _orderCreationDuration;
    private readonly Histogram<double> _databaseQueryDuration;
    
    // Gauges (using UpDownCounter for simplicity)
    private readonly UpDownCounter<long> _activeOrdersGauge;
    private readonly UpDownCounter<long> _totalRevenueGauge;
    
    public BusinessMetrics()
    {
        _meter = new Meter("Ecom.Business", "1.0.0");
        
        // Initialize counters
        _ordersCreatedCounter = _meter.CreateCounter<long>(
            "orders_created_total",
            "count",
            "Total number of orders created");
            
        _buyersCreatedCounter = _meter.CreateCounter<long>(
            "buyers_created_total",
            "count",
            "Total number of buyers created");
            
        _ordersCancelledCounter = _meter.CreateCounter<long>(
            "orders_cancelled_total",
            "count",
            "Total number of orders cancelled");
            
        _ordersShippedCounter = _meter.CreateCounter<long>(
            "orders_shipped_total",
            "count",
            "Total number of orders shipped");
        
        // Initialize histograms
        _orderCreationDuration = _meter.CreateHistogram<double>(
            "order_creation_duration_seconds",
            "seconds",
            "Duration of order creation operations");
            
        _databaseQueryDuration = _meter.CreateHistogram<double>(
            "database_query_duration_seconds",
            "seconds",
            "Duration of database queries");
        
        // Initialize gauges
        _activeOrdersGauge = _meter.CreateUpDownCounter<long>(
            "active_orders_current",
            "count",
            "Current number of active orders");
            
        _totalRevenueGauge = _meter.CreateUpDownCounter<long>(
            "total_revenue_cents",
            "cents",
            "Total revenue in cents");
    }
    
    // Counter methods
    public void IncrementOrdersCreated(string status = "success", string buyerType = "regular") =>
        _ordersCreatedCounter.Add(1, new KeyValuePair<string, object?>("status", status), 
                                      new KeyValuePair<string, object?>("buyer_type", buyerType));
    
    public void IncrementBuyersCreated(string type = "regular") =>
        _buyersCreatedCounter.Add(1, new KeyValuePair<string, object?>("type", type));
    
    public void IncrementOrdersCancelled(string reason = "unknown") =>
        _ordersCancelledCounter.Add(1, new KeyValuePair<string, object?>("reason", reason));
    
    public void IncrementOrdersShipped(string method = "standard") =>
        _ordersShippedCounter.Add(1, new KeyValuePair<string, object?>("shipping_method", method));
    
    // Histogram methods
    public void RecordOrderCreationDuration(double durationSeconds, string orderType = "standard") =>
        _orderCreationDuration.Record(durationSeconds, 
            new KeyValuePair<string, object?>("order_type", orderType));
    
    public void RecordDatabaseQueryDuration(double durationSeconds, string operation = "select", string table = "unknown") =>
        _databaseQueryDuration.Record(durationSeconds,
            new KeyValuePair<string, object?>("operation", operation),
            new KeyValuePair<string, object?>("table", table));
    
    // Gauge methods
    public void SetActiveOrders(long count) =>
        _activeOrdersGauge.Add(count);
    
    public void ChangeActiveOrders(long delta) =>
        _activeOrdersGauge.Add(delta);
    
    public void SetTotalRevenue(long revenueCents) =>
        _totalRevenueGauge.Add(revenueCents);
    
    public void AddRevenue(long revenueCents) =>
        _totalRevenueGauge.Add(revenueCents);
    
    public void Dispose()
    {
        _meter?.Dispose();
    }
}
