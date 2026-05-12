using Scheduled.Stock.Purchase.Domain.ValueObjects;
using Shouldly;

namespace Scheduled.Stock.Purchase.Domain.Tests;

public class MoneyTests
{
    [Fact]
    public void Should_Create_Valid_Money_With_Rounded_Value()
    {
        var result = Money.Create(10.129m);

        result.IsSuccess.ShouldBeTrue();
        result.Value!.Amount.ShouldBe(10.13m);
    }

    [Fact]
    public void Should_Fail_When_Money_Is_Negative()
    {
        var result = Money.Create(-1m);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(MoneyErrors.Negative.Code);
        result.Error.Message.ShouldBe(MoneyErrors.Negative.Message);
    }

    [Fact]
    public void Should_Add_Two_Money_Values()
    {
        var first = Money.Create(15.30m).Value!;
        var second = Money.Create(4.70m).Value!;

        var result = first.Add(second);

        result.IsSuccess.ShouldBeTrue();
        result.Value!.Amount.ShouldBe(20.00m);
    }

    [Fact]
    public void Should_Subtract_Money_When_Amount_Is_Sufficient()
    {
        var first = Money.Create(20.50m).Value!;
        var second = Money.Create(10.25m).Value!;

        var result = first.Subtract(second);

        result.IsSuccess.ShouldBeTrue();
        result.Value!.Amount.ShouldBe(10.25m);
    }

    [Fact]
    public void Should_Fail_To_Subtract_When_Amount_Is_Insufficient()
    {
        var first = Money.Create(5m).Value!;
        var second = Money.Create(5.01m).Value!;

        var result = first.Subtract(second);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(MoneyErrors.InvalidLength.Code);
        result.Error.Message.ShouldBe(MoneyErrors.InvalidLength.Message);
    }

    [Fact]
    public void Should_Multiply_Money_By_Integer_Factor()
    {
        var value = Money.Create(3.33m).Value!;

        var result = value.Multiply(3);

        result.IsSuccess.ShouldBeTrue();
        result.Value!.Amount.ShouldBe(9.99m);
    }

    [Fact]
    public void Should_Consider_Money_With_Same_Amount_As_Equal()
    {
        var first = Money.Create(12.34m).Value;
        var second = Money.Create(12.34m).Value;

        first.ShouldBe(second);
        first!.GetHashCode().ShouldBe(second!.GetHashCode());
    }
}
