namespace TicketWorld.Domain.UnitTests;

using TicketWorld.Domain.Entities;
using TicketWorld.Domain.Enums;
using TicketWorld.Domain.Exceptions;

public class PromoCodeTests
{
    [Fact]
    public void CalculateDiscount_WhenPercentage_AppliesCorrectDiscount()
    {
        var promo = PromoCode.Create(
            code: "SALE20",
            discountType: DiscountType.Percentage,
            discountValue: 20m,
            maxUses: 100,
            validFrom: DateTime.UtcNow.AddDays(-1),
            validUntil: DateTime.UtcNow.AddDays(1));

        var discount = promo.CalculateDiscount(1000m);

        Assert.Equal(200m, discount);
    }

    [Fact]
    public void CalculateDiscount_WhenMinOrderAmountNotMet_ThrowsDomainException()
    {
        var promo = PromoCode.Create(
            code: "MIN500",
            discountType: DiscountType.FixedAmount,
            discountValue: 100m,
            maxUses: 100,
            validFrom: DateTime.UtcNow.AddDays(-1),
            validUntil: DateTime.UtcNow.AddDays(1),
            minOrderAmount: 500m);

        var ex = Assert.Throws<DomainException>(() => promo.CalculateDiscount(300m));
        Assert.Contains("Order total must be at least", ex.Message);
    }

    [Fact]
    public void CalculateDiscount_WhenExpired_ThrowsDomainException()
    {
        var promo = PromoCode.Create(
            code: "EXPIRED",
            discountType: DiscountType.FixedAmount,
            discountValue: 50m,
            maxUses: 10,
            validFrom: DateTime.UtcNow.AddDays(-5),
            validUntil: DateTime.UtcNow.AddDays(-1));

        var ex = Assert.Throws<DomainException>(() => promo.CalculateDiscount(1000m));
        Assert.Contains("expired", ex.Message);
    }
}
