namespace TicketWorld.Application.UnitTests;

using TicketWorld.Application.Features.Bookings.Commands;
using TicketWorld.Application.Features.Seats.Commands;

public class CommandValidatorTests
{
    [Fact]
    public void HoldSeatsCommandValidator_WhenSeatIdsEmpty_ShouldFailValidation()
    {
        var validator = new HoldSeatsCommandValidator();
        var command = new HoldSeatsCommand(Guid.NewGuid(), new List<Guid>());

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "SeatIds");
    }

    [Fact]
    public void HoldSeatsCommandValidator_WhenSeatIdsExceeds6_ShouldFailValidation()
    {
        var validator = new HoldSeatsCommandValidator();
        var seatIds = Enumerable.Range(0, 7).Select(_ => Guid.NewGuid()).ToList();
        var command = new HoldSeatsCommand(Guid.NewGuid(), seatIds);

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("Cannot hold more than 6 seats"));
    }

    [Fact]
    public void HoldSeatsCommandValidator_WhenValid_ShouldPassValidation()
    {
        var validator = new HoldSeatsCommandValidator();
        var seatIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var command = new HoldSeatsCommand(Guid.NewGuid(), seatIds);

        var result = validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void CancelBookingCommandValidator_WhenBookingIdEmpty_ShouldFailValidation()
    {
        var validator = new CancelBookingCommandValidator();
        var command = new CancelBookingCommand(Guid.Empty);

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "BookingId");
    }

    [Fact]
    public void CheckoutBookingCommandValidator_WhenSeatIdsEmpty_ShouldFailValidation()
    {
        var validator = new CheckoutBookingCommandValidator();
        var command = new CheckoutBookingCommand(Guid.NewGuid(), new List<Guid>());

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "SeatIds");
    }
}
