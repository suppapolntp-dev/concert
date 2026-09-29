namespace TicketWorld.Domain.UnitTests;

using TicketWorld.Domain.Entities;
using TicketWorld.Domain.Enums;
using TicketWorld.Domain.Exceptions;

public class SeatTests
{
    [Fact]
    public void Hold_WhenAvailable_SetsStatusToHeld()
    {
        var seat = Seat.Create(Guid.NewGuid(), Guid.NewGuid(), "A", "1", "10");
        var userId = Guid.NewGuid();

        seat.Hold(userId, TimeSpan.FromMinutes(10));

        Assert.Equal(SeatStatus.Held, seat.Status);
        Assert.Equal(userId, seat.HeldByUserId);
        Assert.NotNull(seat.HeldUntil);
    }

    [Fact]
    public void Release_WhenBooked_ThrowsDomainException()
    {
        var seat = Seat.Create(Guid.NewGuid(), Guid.NewGuid(), "A", "1", "10");
        var userId = Guid.NewGuid();
        seat.Hold(userId, TimeSpan.FromMinutes(10));
        seat.ConfirmBooking();

        var ex = Assert.Throws<DomainException>(() => seat.Release());
        Assert.Contains("Cannot release an already booked seat", ex.Message);
    }

    [Fact]
    public void Release_WhenHeld_SetsStatusToAvailable()
    {
        var seat = Seat.Create(Guid.NewGuid(), Guid.NewGuid(), "A", "1", "10");
        var userId = Guid.NewGuid();
        seat.Hold(userId, TimeSpan.FromMinutes(10));

        seat.Release();

        Assert.Equal(SeatStatus.Available, seat.Status);
        Assert.Null(seat.HeldByUserId);
        Assert.Null(seat.HeldUntil);
    }

    [Fact]
    public void ReclaimAndConfirmFromGracePeriod_WhenAvailable_ConfirmsSeat()
    {
        var seat = Seat.Create(Guid.NewGuid(), Guid.NewGuid(), "A", "1", "10");
        var userId = Guid.NewGuid();

        seat.ReclaimAndConfirmFromGracePeriod(userId, "pi_test_123");

        Assert.Equal(SeatStatus.Booked, seat.Status);
        Assert.Equal(userId, seat.HeldByUserId);
        Assert.Equal("pi_test_123", seat.PaymentIntentId);
    }
}
