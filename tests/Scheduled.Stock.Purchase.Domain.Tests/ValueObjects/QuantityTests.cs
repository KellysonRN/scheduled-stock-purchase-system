using Scheduled.Stock.Purchase.Domain.ValueObjects;
using Shouldly;

namespace Scheduled.Stock.Purchase.Domain.Tests;

public class QuantityTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(100)]
    public void Should_Create_Valid_Quantity(int value)
    {
        var result = Quantity.Create(value);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.Value.ShouldBe(value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Should_Fail_When_Quantity_Is_NonPositive(int value)
    {
        var result = Quantity.Create(value);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(QuantityErrors.Negative.Code);
        result.Error.Message.ShouldBe(QuantityErrors.Negative.Message);
    }

    [Fact]
    public void Should_Consider_Quantities_With_Same_Value_As_Equal()
    {
        var first = Quantity.Create(7).Value;
        var second = Quantity.Create(7).Value;

        first.ShouldBe(second);
        first!.GetHashCode().ShouldBe(second!.GetHashCode());
    }
}
