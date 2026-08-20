using Ecom.Domain.Constants;
using Ecom.Domain.Entities;
using Ecom.Domain.Enums;
using Ecom.Domain.Models.Commerce;
using Ecom.IntegrationTests.Catalog;
using Ecom.IntegrationTests.PostgreSql;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Ecom.IntegrationTests.Orders;

[Collection(PostgreSqlCollection.Name)]
public sealed class ManagementOrdersApiTests(PostgreSqlFixture fixture)
{
    [PostgreSqlFact]
    public async Task Management_order_reads_require_orders_read_and_do_not_grant_mutation_access()
    {
        await fixture.ResetDatabaseAsync();
        await using var factory = new CatalogApiFactory(fixture);
        using var client = factory.CreateClient();

        using var anonymous = await client.GetAsync("/api/v1/management/orders");
        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);

        client.DefaultRequestHeaders.Authorization = new("Bearer", CreateAccessToken(Permissions.Orders.Manage));
        using var manageOnly = await client.GetAsync("/api/v1/management/orders");
        Assert.Equal(HttpStatusCode.Forbidden, manageOnly.StatusCode);

        client.DefaultRequestHeaders.Authorization = new("Bearer", CreateAccessToken(Permissions.Orders.Read));
        using var readable = await client.GetAsync("/api/v1/management/orders");
        Assert.Equal(HttpStatusCode.OK, readable.StatusCode);
        await AssertSuccessEnvelopeAsync(readable);

        var emptyDate = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, VietnamTimeZone)).AddDays(-60)
            .ToString("yyyy-MM-dd");
        using var emptyAnalytics = await client.GetAsync($"/api/v1/management/orders/analytics/overview?from={emptyDate}&to={emptyDate}");
        Assert.Equal(HttpStatusCode.OK, emptyAnalytics.StatusCode);
        using var emptyDocument = JsonDocument.Parse(await emptyAnalytics.Content.ReadAsStringAsync());
        var emptyData = emptyDocument.RootElement.GetProperty("data");
        Assert.Equal(0, emptyData.GetProperty("kpis").GetProperty("ordersPlaced").GetInt32());
        Assert.Empty(emptyData.GetProperty("statusBreakdown").EnumerateArray());
        Assert.Empty(emptyData.GetProperty("paymentMethodBreakdown").EnumerateArray());
        Assert.Empty(emptyData.GetProperty("topProducts").EnumerateArray());

        using var tooWideRange = await client.GetAsync("/api/v1/management/orders/analytics/overview?from=2020-01-01");
        Assert.Equal(HttpStatusCode.BadRequest, tooWideRange.StatusCode);

        using var mutation = await client.PostAsync($"/api/v1/management/orders/{Guid.NewGuid()}/confirm", null);
        Assert.Equal(HttpStatusCode.Forbidden, mutation.StatusCode);
    }

    [PostgreSqlFact]
    public async Task Analytics_uses_local_date_boundaries_completed_snapshots_and_cash_transactions()
    {
        await fixture.ResetDatabaseAsync();
        var localDate = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, VietnamTimeZone));
        var eventTime = ToUtc(localDate, new TimeOnly(12, 0));
        await SeedCompletedAndRefundedOrderAsync(eventTime);
        await SeedCancelledOrderAsync(eventTime.AddMinutes(10));
        await SeedDeliveryFailedOrderAsync(eventTime.AddMinutes(20));

        await using var factory = new CatalogApiFactory(fixture);
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", CreateAccessToken(Permissions.Orders.Read));

        var date = localDate.ToString("yyyy-MM-dd");
        using var response = await client.GetAsync($"/api/v1/management/orders/analytics/overview?from={date}&to={date}&granularity=day&topLimit=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var data = document.RootElement.GetProperty("data");
        var kpis = data.GetProperty("kpis");
        Assert.Equal(3, kpis.GetProperty("ordersPlaced").GetInt32());
        Assert.Equal(1, kpis.GetProperty("completedOrderCount").GetInt32());
        Assert.Equal(210_000m, kpis.GetProperty("collectedGross").GetDecimal());
        Assert.Equal(210_000m, kpis.GetProperty("refundAmount").GetDecimal());
        Assert.Equal(0m, kpis.GetProperty("netCollected").GetDecimal());
        Assert.Equal(210_000m, kpis.GetProperty("completedSales").GetDecimal());

        var topProduct = Assert.Single(data.GetProperty("topProducts").EnumerateArray());
        Assert.Equal("Historical tea", topProduct.GetProperty("productName").GetString());
        Assert.Equal("TEA-ARCHIVE", topProduct.GetProperty("sku").GetString());
        Assert.Equal(2, topProduct.GetProperty("quantitySold").GetInt32());
        Assert.Equal(200_000m, topProduct.GetProperty("salesAmount").GetDecimal());

        var paymentMethod = Assert.Single(data.GetProperty("paymentMethodBreakdown").EnumerateArray());
        Assert.Equal("SePay", paymentMethod.GetProperty("paymentMethod").GetString());
        Assert.Equal(210_000m, paymentMethod.GetProperty("collectedGross").GetDecimal());
        Assert.Equal(210_000m, paymentMethod.GetProperty("refundAmount").GetDecimal());
        Assert.Equal(0m, paymentMethod.GetProperty("netCollected").GetDecimal());

        using var listResponse = await client.GetAsync(
            "/api/v1/management/orders?status=Completed&paymentStatus=Refunded&paymentMethod=SePay&shipmentStatus=Delivered&sort=GrandTotalDescending&page=1&pageSize=1");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        using var listDocument = JsonDocument.Parse(await listResponse.Content.ReadAsStringAsync());
        var list = listDocument.RootElement.GetProperty("data");
        Assert.Equal(1, list.GetProperty("totalCount").GetInt32());
        var listedOrder = Assert.Single(list.GetProperty("items").EnumerateArray());
        Assert.Equal(1, listedOrder.GetProperty("itemCount").GetInt32());
        Assert.Equal(200_000m, listedOrder.GetProperty("subtotalAmount").GetDecimal());
        Assert.Equal(10_000m, listedOrder.GetProperty("shippingAmount").GetDecimal());
        Assert.Equal(210_000m, listedOrder.GetProperty("grandTotalAmount").GetDecimal());
        Assert.False(listDocument.RootElement.GetRawText().Contains("customerEmail", StringComparison.OrdinalIgnoreCase));
        Assert.False(listDocument.RootElement.GetRawText().Contains("providerReference", StringComparison.OrdinalIgnoreCase));

        var payload = document.RootElement.GetRawText();
        Assert.DoesNotContain("providerReference", payload, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("guestTokenHash", payload, StringComparison.OrdinalIgnoreCase);
    }

    [PostgreSqlFact]
    public async Task Analytics_collects_completed_cod_and_bank_transfer_payments()
    {
        await fixture.ResetDatabaseAsync();
        var localDate = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, VietnamTimeZone));
        var eventTime = ToUtc(localDate, new TimeOnly(12, 0));
        await SeedCompletedPaidOrderAsync(eventTime, PaymentMethod.COD, "COD-SKU", 70_000m);
        await SeedCompletedPaidOrderAsync(eventTime.AddMinutes(10), PaymentMethod.BankTransfer, "BANK-SKU", 150_000m);

        await using var factory = new CatalogApiFactory(fixture);
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", CreateAccessToken(Permissions.Orders.Read));

        var date = localDate.ToString("yyyy-MM-dd");
        using var response = await client.GetAsync($"/api/v1/management/orders/analytics/overview?from={date}&to={date}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var kpis = document.RootElement.GetProperty("data").GetProperty("kpis");
        Assert.Equal(220_000m, kpis.GetProperty("collectedGross").GetDecimal());
        Assert.Equal(0m, kpis.GetProperty("refundAmount").GetDecimal());
        Assert.Equal(220_000m, kpis.GetProperty("netCollected").GetDecimal());
        Assert.Equal(220_000m, kpis.GetProperty("completedSales").GetDecimal());
        var methods = document.RootElement.GetProperty("data").GetProperty("paymentMethodBreakdown").EnumerateArray().ToList();
        Assert.Contains(methods, x => x.GetProperty("paymentMethod").GetString() == "COD");
        Assert.Contains(methods, x => x.GetProperty("paymentMethod").GetString() == "BankTransfer");
    }

    private async Task SeedCompletedAndRefundedOrderAsync(DateTime eventTime)
    {
        var items = new List<OrderItem>();
        var history = new List<OrderStatusHistory>();
        var order = Order.Create($"ANALYTICS-{Guid.NewGuid():N}", null, "analytics-guest-token", null, "0900000000",
            "Analytics buyer", "0900000000", null, "Analytics address", 10_000m, eventTime,
            [new OrderLineSnapshot(Guid.NewGuid(), "Historical tea", "Box", "TEA-ARCHIVE", 100_000m, 2)], items, history);
        order.Confirm(null, eventTime.AddMinutes(1), history);
        order.StartPreparing(null, eventTime.AddMinutes(2), history);
        order.StartShipping(null, eventTime.AddMinutes(3), history);
        order.Complete(null, eventTime.AddMinutes(4), history);
        var payment = Payment.Create(order.Id, PaymentMethod.SePay, order.GrandTotalAmount, eventTime);
        var paid = payment.MarkPaid(order.GrandTotalAmount, "sepay", "provider-payment-reference", eventTime.AddMinutes(5));
        var refund = payment.Refund(order.GrandTotalAmount, "sepay", "provider-refund-reference", eventTime.AddMinutes(6));
        var shipmentHistory = new List<ShipmentHistory>();
        var shipment = Shipment.Create(order.Id, "standard", eventTime, shipmentHistory);
        shipment.MarkReady(null, eventTime.AddMinutes(1), shipmentHistory);
        shipment.StartShipping("carrier", "tracking-code", null, eventTime.AddMinutes(3), shipmentHistory);
        shipment.MarkDelivered(null, eventTime.AddMinutes(4), shipmentHistory);

        await using var context = fixture.CreateDbContext();
        context.Orders.Add(order);
        context.OrderItems.AddRange(items);
        context.OrderStatusHistories.AddRange(history);
        context.Payments.Add(payment);
        context.PaymentTransactions.AddRange(paid, refund);
        context.Shipments.Add(shipment);
        context.ShipmentHistories.AddRange(shipmentHistory);
        await context.SaveChangesAsync();
    }

    private async Task SeedCancelledOrderAsync(DateTime eventTime)
    {
        var items = new List<OrderItem>();
        var history = new List<OrderStatusHistory>();
        var order = Order.Create($"CANCELLED-{Guid.NewGuid():N}", null, "cancelled-guest-token", null, "0900000001",
            "Cancelled buyer", "0900000001", null, "Cancelled address", 0m, eventTime,
            [new OrderLineSnapshot(Guid.NewGuid(), "Cancelled product", "Default", "CANCELLED-SKU", 50_000m, 1)], items, history);
        order.Cancel("customer cancellation", null, eventTime.AddMinutes(1), history);

        await using var context = fixture.CreateDbContext();
        context.Orders.Add(order);
        context.OrderItems.AddRange(items);
        context.OrderStatusHistories.AddRange(history);
        await context.SaveChangesAsync();
    }

    private async Task SeedDeliveryFailedOrderAsync(DateTime eventTime)
    {
        var items = new List<OrderItem>();
        var history = new List<OrderStatusHistory>();
        var order = Order.Create($"DELIVERY-FAILED-{Guid.NewGuid():N}", null, "delivery-failed-guest-token", null, "0900000002",
            "Delivery failed buyer", "0900000002", null, "Delivery failed address", 0m, eventTime,
            [new OrderLineSnapshot(Guid.NewGuid(), "Delivery failed product", "Default", "FAILED-SKU", 50_000m, 1)], items, history);
        order.Confirm(null, eventTime.AddMinutes(1), history);
        order.StartPreparing(null, eventTime.AddMinutes(2), history);
        order.StartShipping(null, eventTime.AddMinutes(3), history);
        order.MarkDeliveryFailed("carrier could not deliver", null, eventTime.AddMinutes(4), history);

        await using var context = fixture.CreateDbContext();
        context.Orders.Add(order);
        context.OrderItems.AddRange(items);
        context.OrderStatusHistories.AddRange(history);
        await context.SaveChangesAsync();
    }

    private async Task SeedCompletedPaidOrderAsync(DateTime eventTime, PaymentMethod paymentMethod, string sku, decimal amount)
    {
        var items = new List<OrderItem>();
        var history = new List<OrderStatusHistory>();
        var order = Order.Create($"PAID-{Guid.NewGuid():N}", null, $"paid-{Guid.NewGuid():N}", null, "0900000003",
            "Paid buyer", "0900000003", null, "Paid address", 0m, eventTime,
            [new OrderLineSnapshot(Guid.NewGuid(), $"{paymentMethod} snapshot", "Default", sku, amount, 1)], items, history);
        order.Confirm(null, eventTime.AddMinutes(1), history);
        order.StartPreparing(null, eventTime.AddMinutes(2), history);
        order.StartShipping(null, eventTime.AddMinutes(3), history);
        order.Complete(null, eventTime.AddMinutes(4), history);
        var payment = Payment.Create(order.Id, paymentMethod, order.GrandTotalAmount, eventTime);
        var paid = payment.MarkPaid(order.GrandTotalAmount, paymentMethod.ToString(), "payment-reference", eventTime.AddMinutes(5));

        await using var context = fixture.CreateDbContext();
        context.Orders.Add(order);
        context.OrderItems.AddRange(items);
        context.OrderStatusHistories.AddRange(history);
        context.Payments.Add(payment);
        context.PaymentTransactions.Add(paid);
        await context.SaveChangesAsync();
    }

    private static string CreateAccessToken(params string[] permissions)
    {
        var claims = permissions.Select(permission => new Claim("policy", permission))
            .Append(new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()));
        var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(CatalogApiFactory.JwtSecret)),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(CatalogApiFactory.JwtIssuer, CatalogApiFactory.JwtAudience, claims,
            notBefore: DateTime.UtcNow.AddMinutes(-1), expires: DateTime.UtcNow.AddMinutes(5), signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static async Task AssertSuccessEnvelopeAsync(HttpResponseMessage response)
    {
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(document.RootElement.GetProperty("success").GetBoolean());
        Assert.True(document.RootElement.TryGetProperty("data", out _));
    }

    private static readonly TimeZoneInfo VietnamTimeZone = ResolveVietnamTimeZone();

    private static DateTime ToUtc(DateOnly date, TimeOnly time) => TimeZoneInfo.ConvertTimeToUtc(
        DateTime.SpecifyKind(date.ToDateTime(time), DateTimeKind.Unspecified), VietnamTimeZone);

    private static TimeZoneInfo ResolveVietnamTimeZone()
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh"); }
        catch (TimeZoneNotFoundException) { return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); }
    }
}
