using Scheduled.Stock.Purchase.Domain.ValueObjects;

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

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(value, result.Value!.Value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Should_Fail_When_Quantity_Is_NonPositive(int value)
    {
        var result = Quantity.Create(value);

        Assert.True(result.IsFailure);
        Assert.Equal(QuantityErrors.Negative.Code, result.Error.Code);
        Assert.Equal(QuantityErrors.Negative.Message, result.Error.Message);
    }

    [Fact]
    public void Should_Consider_Quantities_With_Same_Value_As_Equal()
    {
        var first = Quantity.Create(7).Value;
        var second = Quantity.Create(7).Value;

        Assert.Equal(first, second);
        Assert.Equal(first!.GetHashCode(), second!.GetHashCode());
    }
}
