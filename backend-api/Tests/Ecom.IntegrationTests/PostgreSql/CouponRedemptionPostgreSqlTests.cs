using Ecom.Domain.Entities;
using Ecom.Domain.Enums;
using Ecom.Domain.Models.Commerce;
using Ecom.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace Ecom.IntegrationTests.PostgreSql;

[Collection(PostgreSqlCollection.Name)]
public sealed class CouponRedemptionPostgreSqlTests(PostgreSqlFixture fixture)
{
    [PostgreSqlFact]
    public async Task Coupon_row_lock_serializes_redemption_count_for_the_same_code()
    {
        await fixture.ResetDatabaseAsync();
        var now = DateTime.UtcNow;
        var seeded = await SeedCouponAndOrdersAsync(now);

        await using var firstContext = fixture.CreateDbContext();
        await using var firstTransaction = await firstContext.Database.BeginTransactionAsync();
        var firstStore = new CouponRedemptionStore(firstContext);
        var firstLock = await firstStore.LockCouponAndPromotionAsync(seeded.Coupon.Code, CancellationToken.None);
        Assert.NotNull(firstLock);
        Assert.Equal(0, await firstContext.CouponRedemptions.CountAsync(x => x.CouponId == seeded.Coupon.Id));

        firstContext.CouponRedemptions.Add(CouponRedemption.Create(seeded.Coupon.Id, null, seeded.FirstOrderId, 10_000m, now));
        await firstContext.SaveChangesAsync();

        var secondAcquired = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        var secondTask = Task.Run(async () =>
        {
            await using var secondContext = fixture.CreateDbContext();
            await using var secondTransaction = await secondContext.Database.BeginTransactionAsync();
            var secondStore = new CouponRedemptionStore(secondContext);
            var secondLock = await secondStore.LockCouponAndPromotionAsync(seeded.Coupon.Code, CancellationToken.None);
            Assert.NotNull(secondLock);
            var count = await secondContext.CouponRedemptions.CountAsync(x => x.CouponId == seeded.Coupon.Id);
            secondAcquired.TrySetResult(count);
            await secondTransaction.CommitAsync();
        });

        var beforeFirstCommit = await Task.WhenAny(secondAcquired.Task, Task.Delay(TimeSpan.FromMilliseconds(250)));
        Assert.NotSame(secondAcquired.Task, beforeFirstCommit);

        await firstTransaction.CommitAsync();
        Assert.Equal(1, await secondAcquired.Task.WaitAsync(TimeSpan.FromSeconds(5)));
        await secondTask;
    }

    private async Task<SeededCoupon> SeedCouponAndOrdersAsync(DateTime now)
    {
        await using var context = fixture.CreateDbContext();
        var producerId = Guid.NewGuid();
        await context.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO ""Tbl_Producer""
                (""Id"", ""Code"", ""Name"", ""PublicStatus"", ""IsVerified"", ""CreatedAt"", ""IsDeleted"", ""ConcurrencyStamp"")
            VALUES
                ({producerId}, {"COUPON_LOCK_PRODUCER"}, {"Coupon lock producer"}, {"Draft"}, {false}, {now}, {false}, {Guid.NewGuid()});
            """);

        var product = Product.Create(producerId, "Coupon lock product", $"coupon-lock-{Guid.NewGuid():N}");
        var variant = ProductVariant.Create(product.Id, $"LOCK-{Guid.NewGuid():N}", "Default", InventoryMode.NotTracked);
        var promotion = Promotion.Create("LOCK-PROMO", "Lock promotion", PromotionType.FixedAmount, 10_000m,
            now.AddDays(-1), now.AddDays(1), null);
        var coupon = Coupon.Create("LOCK-COUPON", promotion.Id, 1, null, now.AddDays(-1), now.AddDays(1));

        var firstItems = new List<OrderItem>();
        var firstHistory = new List<OrderStatusHistory>();
        var firstOrder = Order.Create("ORD-LOCK-1", null, "guest-lock-one", null, "0900000000", "Buyer one",
            "0900000000", null, "Test address", 0m, now,
            [new OrderLineSnapshot(variant.Id, product.Name, variant.Name, variant.Sku, 100_000m, 1)], firstItems, firstHistory);
        var secondItems = new List<OrderItem>();
        var secondHistory = new List<OrderStatusHistory>();
        var secondOrder = Order.Create("ORD-LOCK-2", null, "guest-lock-two", null, "0900000001", "Buyer two",
            "0900000001", null, "Test address", 0m, now,
            [new OrderLineSnapshot(variant.Id, product.Name, variant.Name, variant.Sku, 100_000m, 1)], secondItems, secondHistory);

        context.Products.Add(product);
        context.ProductVariants.Add(variant);
        context.Promotions.Add(promotion);
        context.Coupons.Add(coupon);
        context.Orders.AddRange(firstOrder, secondOrder);
        context.OrderItems.AddRange(firstItems);
        context.OrderItems.AddRange(secondItems);
        context.OrderStatusHistories.AddRange(firstHistory);
        context.OrderStatusHistories.AddRange(secondHistory);
        await context.SaveChangesAsync();

        return new SeededCoupon(coupon, firstOrder.Id);
    }

    private sealed record SeededCoupon(Coupon Coupon, Guid FirstOrderId);
}
