using Scheduled.Stock.Purchase.Domain.ValueObjects;
using Shouldly;

namespace Scheduled.Stock.Purchase.Domain.Tests;

public class TickerTests
{
    [Theory]
    [InlineData("AAPL1", "AAPL1")]
    [InlineData("goog12", "GOOG12")]
    [InlineData(" msft2 ", "MSFT2")]
    public void Should_Create_Valid_Ticker(string value, string expected)
    {
        var result = Ticker.Create(value);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.Value.ShouldBe(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Fail_When_Ticker_Is_Missing(string? value)
    {
        var result = Ticker.Create(value!);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(TickerErrors.Required.Code);
        result.Error.Message.ShouldBe(TickerErrors.Required.Message);
    }

    [Theory]
    [InlineData("ABC1")]
    [InlineData("ABCDE1")]
    [InlineData("ABCD")]
    [InlineData("ABCD123")]
    [InlineData("AB1D2")]
    [InlineData("ABCD!1")]
    public void Should_Fail_When_Ticker_Has_Invalid_Format(string value)
    {
        var result = Ticker.Create(value);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(TickerErrors.InvalidFormat.Code);
        result.Error.Message.ShouldBe(TickerErrors.InvalidFormat.Message);
    }

    [Fact]
    public void Should_Consider_Tickers_With_Same_Normalized_Value_As_Equal()
    {
        var ticker1 = Ticker.Create("goog12").Value;
        var ticker2 = Ticker.Create(" GOOG12 ").Value;

        ticker1.ShouldBe(ticker2);
        ticker1!.GetHashCode().ShouldBe(ticker2!.GetHashCode());
    }
}
