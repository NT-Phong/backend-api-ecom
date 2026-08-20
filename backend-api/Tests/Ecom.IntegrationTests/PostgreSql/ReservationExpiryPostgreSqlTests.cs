using Ecom.Application.Features.Commerce.Inventory.Commands.ExpireReservations;
using Ecom.Domain.Entities;
using Ecom.Domain.Enums;
using Ecom.Domain.Models.Commerce;
using Ecom.Infrastructure.Persistence.Database.UnitOfWork;
using Ecom.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ecom.IntegrationTests.PostgreSql;

[Collection(PostgreSqlCollection.Name)]
public sealed class ReservationExpiryPostgreSqlTests(PostgreSqlFixture fixture)
{
    [PostgreSqlFact]
    public async Task Expiring_one_order_releases_every_active_reservation_exactly_once()
    {
        await fixture.ResetDatabaseAsync();
        var now = DateTime.UtcNow;

        await using (var seedContext = fixture.CreateDbContext())
        {
            var inventory = await SeedTrackedInventoryAsync(seedContext, now);
            var orderItems = new List<OrderItem>();
            var orderHistory = new List<OrderStatusHistory>();
            var order = Order.Create(
                "ORD-EXPIRY-001",
                null,
                "guest-hash",
                null,
                "0900000000",
                "Guest buyer",
                "0900000000",
                null,
                "Test address",
                30_000m,
                now.AddMinutes(-31),
                [
                    new OrderLineSnapshot(inventory[0].InventoryItem.ProductVariantId, "Product", "Variant A", "SKU-A", 100_000m, 2),
                    new OrderLineSnapshot(inventory[1].InventoryItem.ProductVariantId, "Product", "Variant B", "SKU-B", 100_000m, 3)
                ],
                orderItems,
                orderHistory);
            var payment = Payment.Create(order.Id, PaymentMethod.COD, order.GrandTotalAmount, now.AddMinutes(-1));

            foreach (var item in inventory)
            {
                item.Level.Reserve(item.Quantity);
                seedContext.InventoryReservations.Add(InventoryReservation.Create(
                    item.InventoryItem.Id,
                    item.Level.StockLocationId,
                    orderItems.Single(x => x.ProductVariantId == item.InventoryItem.ProductVariantId).Id,
                    item.Quantity,
                    now.AddMinutes(-1)));
                seedContext.InventoryLevels.Update(item.Level);
            }

            seedContext.Orders.Add(order);
            seedContext.OrderItems.AddRange(orderItems);
            seedContext.OrderStatusHistories.AddRange(orderHistory);
            seedContext.Payments.Add(payment);
            await seedContext.SaveChangesAsync();
        }

        await using (var commandContext = fixture.CreateDbContext())
        using (var unitOfWork = new UnitOfWork(commandContext, NullLogger<UnitOfWork>.Instance))
        {
            var handler = new ExpireReservationsCommandHandler(unitOfWork, new InventoryReservationStore(commandContext),
                new OrderLifecycleStore(commandContext));
            Assert.True(await unitOfWork.BeginTransactionAsync());

            var result = await handler.Handle(new ExpireReservationsCommand(), CancellationToken.None);

            Assert.True(result.IsSuccess);
            await unitOfWork.CommitTransactionAsync();
        }

        await using var verificationContext = fixture.CreateDbContext();
        Assert.All(await verificationContext.InventoryLevels.ToListAsync(), level =>
        {
            Assert.Equal(0m, level.ReservedQuantity);
            Assert.Equal(10m, level.StockedQuantity);
        });
        Assert.All(await verificationContext.InventoryReservations.ToListAsync(), reservation =>
            Assert.Equal(InventoryReservationStatus.Expired, reservation.Status));
        Assert.Equal(OrderStatus.Cancelled, (await verificationContext.Orders.SingleAsync()).Status);
        Assert.Equal(PaymentStatus.Cancelled, (await verificationContext.Payments.SingleAsync()).Status);
        Assert.Equal(2, await verificationContext.InventoryMovements.CountAsync());
        Assert.Single(await verificationContext.PaymentTransactions.ToListAsync());
    }

    private static async Task<IReadOnlyList<TrackedInventory>> SeedTrackedInventoryAsync(
        Ecom.Infrastructure.Persistence.Database.ApplicationDbContext context,
        DateTime now)
    {
        var producerId = Guid.NewGuid();
        var stockLocationId = Guid.NewGuid();
        await context.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO ""Tbl_Producer""
                (""Id"", ""Code"", ""Name"", ""PublicStatus"", ""IsVerified"", ""CreatedAt"", ""IsDeleted"", ""ConcurrencyStamp"")
            VALUES
                ({producerId}, {"EXPIRY_PRODUCER"}, {"Expiry producer"}, {"Draft"}, {false}, {now}, {false}, {Guid.NewGuid()});

            INSERT INTO ""Tbl_StockLocation""
                (""Id"", ""Code"", ""Name"", ""IsActive"", ""CreatedAt"", ""IsDeleted"", ""ConcurrencyStamp"")
            VALUES
                ({stockLocationId}, {"MAIN"}, {"Main warehouse"}, {true}, {now}, {false}, {Guid.NewGuid()});
            """);

        var product = Product.Create(producerId, "Expiry product", $"expiry-product-{Guid.NewGuid():N}");
        var firstVariant = ProductVariant.Create(product.Id, $"EXPIRY-A-{Guid.NewGuid():N}", "Variant A", InventoryMode.Tracked);
        var secondVariant = ProductVariant.Create(product.Id, $"EXPIRY-B-{Guid.NewGuid():N}", "Variant B", InventoryMode.Tracked);
        var firstItem = InventoryItem.Create(firstVariant.Id);
        var secondItem = InventoryItem.Create(secondVariant.Id);
        var firstLevel = InventoryLevel.Create(firstItem.Id, stockLocationId);
        var secondLevel = InventoryLevel.Create(secondItem.Id, stockLocationId);
        firstLevel.Receive(10m, now);
        secondLevel.Receive(10m, now);

        context.Products.Add(product);
        context.ProductVariants.AddRange(firstVariant, secondVariant);
        context.InventoryItems.AddRange(firstItem, secondItem);
        context.InventoryLevels.AddRange(firstLevel, secondLevel);
        await context.SaveChangesAsync();

        return
        [
            new TrackedInventory(firstItem, firstLevel, 2m),
            new TrackedInventory(secondItem, secondLevel, 3m)
        ];
    }

    private sealed record TrackedInventory(InventoryItem InventoryItem, InventoryLevel Level, decimal Quantity);
}
