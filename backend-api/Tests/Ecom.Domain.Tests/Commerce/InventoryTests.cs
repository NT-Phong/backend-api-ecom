namespace Ecom.Domain.Tests.Commerce;

public class InventoryTests
{
    [Fact]
    public void Inventory_prevents_over_reserve_and_tracks_consume()
    {
        var level = InventoryLevel.Create(Guid.NewGuid(), Guid.NewGuid());
        level.Receive(10, DateTime.UtcNow);
        level.Reserve(6);

        Assert.Equal(4, level.AvailableQuantity);
        Assert.Throws<CommerceDomainException>(() => level.Reserve(5));

        var movement = level.Consume(4, DateTime.UtcNow, Guid.NewGuid());
        Assert.Equal(-4, movement.QuantityDelta);
        Assert.Equal(6, level.StockedQuantity);
        Assert.Equal(2, level.ReservedQuantity);
    }

    [Fact]
    public void Reservation_terminal_state_cannot_change_again()
    {
        var reservation = InventoryReservation.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 2, DateTime.UtcNow.AddMinutes(30));
        reservation.Release(DateTime.UtcNow);

        Assert.Equal(InventoryReservationStatus.Released, reservation.Status);
        Assert.Throws<CommerceDomainException>(() => reservation.Consume());
    }

    [Fact]
    public void Reservation_cannot_expire_before_deadline()
    {
        var expiry = DateTime.UtcNow.AddMinutes(30);
        var reservation = InventoryReservation.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 2, expiry);
        Assert.Throws<CommerceDomainException>(() => reservation.Expire(expiry.AddSeconds(-1)));
    }

    [Fact]
    public void Confirmed_reservation_hold_no_longer_has_an_expiry()
    {
        var reservation = InventoryReservation.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 2, DateTime.UtcNow.AddMinutes(30));
        reservation.ConfirmHold();
        Assert.Null(reservation.ExpiresAt);
    }
}
